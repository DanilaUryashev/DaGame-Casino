using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ядро всей механики: смотрит на уже заполненную сетку, решает какие паттерны совпали,
/// склеивает результаты по правилу стакинга, крутит модификаторы и обрабатывает черепа.
/// Не знает ничего про визуал — только данные на входе, список результатов на выходе.
/// </summary>
public static class RewardResolver
{
    public static List<SpinResult> Resolve(SlotSymbol[,] grid, SlotMachineConfig config)
    {
        // Шаг 1 — для каждого паттерна собираем список ID символов, попавших в его координаты.
        // Идея: проходим паттерны и их ячейки, записываем ID в список конкретно этого паттерна.
        Dictionary<PaylinePattern, List<int>> patternHits = new Dictionary<PaylinePattern, List<int>>();

        foreach (var pattern in config.patterns)
        {
            // Список обязательно чистый на каждый спин — иначе результаты прошлого спина
            // останутся и сравнение "все совпали" будет врать.
            patternHits[pattern] = new List<int>();
        }

        foreach (var pattern in config.patterns)
        {
            foreach (var cellCoord in pattern.cells)
            {
                SlotSymbol symbolAtCell = grid[cellCoord.x, cellCoord.y];
                patternHits[pattern].Add(symbolAtCell.symbolId);
            }
        }

        // Шаг 2 — какие паттерны реально "сыграли": все записанные ID одинаковые
        // и их количество равно количеству ячеек паттерна (защитная проверка).
        List<PaylinePattern> winningPatterns = new List<PaylinePattern>();

        foreach (var pattern in config.patterns)
        {
            List<int> ids = patternHits[pattern];

            if (ids.Count != pattern.cells.Length)
                continue;

            bool allSame = true;
            int firstId = ids[0];

            for (int i = 1; i < ids.Count; i++)
            {
                if (ids[i] != firstId)
                {
                    allSame = false;
                    break;
                }
            }

            if (allSame)
                winningPatterns.Add(pattern);
        }

        // Шаг 3 — стакинг: паттерны с ОДИНАКОВЫМ выигрышным символом объединяются в один
        // результат (один предмет с суммой бафов от всех сыгравших паттернов), паттерны
        // с разными символами дают отдельные результаты. Группируем по symbolId.
        Dictionary<int, List<PaylinePattern>> groupedBySymbol = new Dictionary<int, List<PaylinePattern>>();

        foreach (var pattern in winningPatterns)
        {
            int symbolId = patternHits[pattern][0]; // все элементы одинаковые, берём первый

            if (!groupedBySymbol.ContainsKey(symbolId))
                groupedBySymbol[symbolId] = new List<PaylinePattern>();

            groupedBySymbol[symbolId].Add(pattern);
        }

        // Шаг 4 — превращаем каждую группу в конкретный SpinResult.
        List<SpinResult> results = new List<SpinResult>();

        foreach (var group in groupedBySymbol)
        {
            int symbolId = group.Key;
            List<PaylinePattern> patternsForThisSymbol = group.Value;

            // Череп — отдельная ветка, не про предметы.
            if (symbolId == config.skullSymbolId)
            {
                results.Add(BuildSkullResult(patternsForThisSymbol));
                continue;
            }

            // Мусор никогда не даёт результат, даже если случайно собрался в паттерн.
            if (symbolId == config.junkSymbolId)
                continue;

            results.Add(BuildItemResult(symbolId, patternsForThisSymbol, config));
        }

        return results;
    }

    // Собирает предмет + бафы для одной группы сыгравших паттернов с одинаковым символом.
    private static SpinResult BuildItemResult(int symbolId, List<PaylinePattern> patterns, SlotMachineConfig config)
    {
        SlotSymbol matchedSymbol = FindSymbolById(symbolId, config);

        SpinResult result = new SpinResult();
        result.label = matchedSymbol != null ? matchedSymbol.name : "Unknown";
        result.contributingPatterns = patterns;

        ItemInstance item = new ItemInstance(matchedSymbol != null ? matchedSymbol.linkedItem : null);

        // Каждый сыгравший паттерн отдельно крутит свой модификатор (если у него есть пул).
        // Так и получается стакинг — несколько паттернов на один символ = несколько бафов на предмет.
        foreach (var pattern in patterns)
        {
            if (pattern.modifierPool == null || pattern.modifierPool.Count == 0)
                continue; // простая линия без бафа — ничего не крутим

            ModifierType rolledType = RollWeightedModifierType(pattern.modifierPool);
            if (rolledType == null || rolledType.possibleValues == null || rolledType.possibleValues.Length == 0)
                continue;

            float rolledValue = RollWeightedModifierValue(rolledType.possibleValues);
            item.modifiers.Add(new AppliedModifier(rolledType, rolledValue));
        }

        result.itemAwarded = item;
        return result;
    }

    // Собирает эффект урона/смерти для сыгравших паттернов черепа.
    private static SpinResult BuildSkullResult(List<PaylinePattern> patterns)
    {
        SpinResult result = new SpinResult();
        result.label = "Skull";
        result.isDeathEffect = true;
        result.contributingPatterns = patterns;

        // Берём самый "тяжёлый" тир среди сыгравших паттернов черепа — он решает тяжесть эффекта.
        // Jackpot-тир черепом = вайп команды, остальное — масштабируемый урон.
        PatternTier worstTier = PatternTier.Common;
        foreach (var pattern in patterns)
        {
            if (pattern.tier > worstTier)
                worstTier = pattern.tier;
        }

        switch (worstTier)
        {
            case PatternTier.Common:
                result.damageAmount = 10f; // заглушка — подставь свои цифры баланса
                break;
            case PatternTier.Rare:
                result.damageAmount = 25f;
                break;
            case PatternTier.Jackpot:
                result.isTeamWipe = true;
                break;
        }

        return result;
    }

    // Взвешенный рандом по списку WeightedModifierEntry (какая категория модификатора выпала).
    private static ModifierType RollWeightedModifierType(List<WeightedModifierEntry> pool)
    {
        int totalWeight = 0;
        foreach (var entry in pool)
            totalWeight += entry.weight;

        if (totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);

        foreach (var entry in pool)
        {
            if (roll < entry.weight)
                return entry.modifierType;

            roll -= entry.weight;
        }

        return null;
    }

    // Взвешенный рандом по списку ModifierValue (конкретное число внутри уже выбранной категории).
    private static float RollWeightedModifierValue(ModifierValue[] values)
    {
        int totalWeight = 0;
        foreach (var v in values)
            totalWeight += v.weight;

        if (totalWeight <= 0)
            return values[0].value;

        int roll = Random.Range(0, totalWeight);

        foreach (var v in values)
        {
            if (roll < v.weight)
                return v.value;

            roll -= v.weight;
        }

        return values[0].value;
    }

    private static SlotSymbol FindSymbolById(int id, SlotMachineConfig config)
    {
        foreach (var symbol in config.symbols)
        {
            if (symbol.symbolId == id)
                return symbol;
        }
        return null;
    }
}
