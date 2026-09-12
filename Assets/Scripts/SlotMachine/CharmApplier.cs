using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Строит рабочую (временную) копию весов символов на основе конфига + применённых чармов.
/// Никогда не трогает сам SlotMachineConfig — только создаёт список-копию, которую
/// GridGenerator использует ОДИН раз для этого конкретного спина.
/// </summary>
public static class CharmApplier
{
    public static List<SymbolWeightEntry> BuildWorkingWeights(SlotMachineConfig config, List<SlotCharm> activeCharms)
    {
        List<SymbolWeightEntry> working = new List<SymbolWeightEntry>();

        // Копируем базовые веса из самих символов — это "дефолт", если чармов нет.
        foreach (var symbol in config.symbols)
        {
            working.Add(new SymbolWeightEntry(symbol, symbol.weight));
        }

        if (activeCharms == null)
            return working;

        foreach (var charm in activeCharms)
        {
            ApplyCharmEffect(charm, working);
        }

        return working;
    }

    private static void ApplyCharmEffect(SlotCharm charm, List<SymbolWeightEntry> working)
    {
        switch (charm.effectType)
        {
            case CharmEffectType.RemoveJunk:
                // effectStrength от 0 до 1: 1 = полностью убрать мусор из пула на этот спин.
                foreach (var entry in working)
                {
                    if (entry.symbol.isJunk)
                    {
                        entry.currentWeight = Mathf.RoundToInt(entry.currentWeight * (1f - charm.effectStrength));
                    }
                }
                break;

            case CharmEffectType.BoostPatternWeight:
                // TODO: сейчас упрощённо — повышаем шанс ВСЕХ не-мусорных и не-черепных
                // символов сразу, что косвенно повышает шанс любого паттерна вообще.
                // Буст ИМЕННО одного конкретного паттерна/тира (charm.targetTier) требует
                // более сложной логики (например повторной генерации, если нужный паттерн
                // не выпал) — оставлено на твою доработку, когда решишь как это должно
                // ощущаться в игре.
                foreach (var entry in working)
                {
                    if (!entry.symbol.isJunk && !entry.symbol.isSkull)
                    {
                        entry.currentWeight = Mathf.RoundToInt(entry.currentWeight * charm.effectStrength);
                    }
                }
                break;

            case CharmEffectType.GuaranteeHit:
            case CharmEffectType.FreeSpin:
                // Эти два не меняют веса — обрабатываются отдельно в SlotMachineController
                // (GuaranteeHit — постобработка сетки после генерации, FreeSpin — повторный вызов).
                break;
        }
    }
}
