using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Отвечает ТОЛЬКО за одно: заполнить сетку случайными символами с учётом весов.
/// Ничего не знает про паттерны, бафы или визуал — чистая генерация данных.
/// </summary>
public static class GridGenerator
{
    /// <summary>
    /// Генерирует сетку columns x rows. Возвращает [столбец, строка] с выбранными символами.
    /// symbolWeights передаётся отдельно от конфига (не берётся напрямую из SlotMachineConfig),
    /// чтобы чармы могли временно подменить веса через CharmApplier, не трогая сам конфиг.
    /// </summary>
    public static SlotSymbol[,] GenerateGrid(int columns, int rows, List<SymbolWeightEntry> symbolWeights)
    {
        SlotSymbol[,] grid = new SlotSymbol[columns, rows];

        // Суммарный вес считаем один раз — не пересчитываем на каждую ячейку.
        int totalWeight = 0;
        foreach (var entry in symbolWeights)
            totalWeight += entry.currentWeight;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                grid[col, row] = PickWeightedRandomSymbol(symbolWeights, totalWeight);
            }
        }

        return grid;
    }

    // Классический взвешенный рандом: кидаем "стрелку" от 0 до totalWeight,
    // идём по списку и вычитаем вес каждого элемента, пока стрелка не "провалится" в текущий.
    private static SlotSymbol PickWeightedRandomSymbol(List<SymbolWeightEntry> entries, int totalWeight)
    {
        if (totalWeight <= 0)
        {
            Debug.LogWarning("GridGenerator: суммарный вес символов равен 0, проверь настройки.");
            return entries.Count > 0 ? entries[0].symbol : null;
        }

        int roll = Random.Range(0, totalWeight);

        foreach (var entry in entries)
        {
            if (roll < entry.currentWeight)
                return entry.symbol;

            roll -= entry.currentWeight;
        }

        // Сюда не должны попасть при корректных весах — safety net, чтобы не вернуть null.
        Debug.LogWarning("GridGenerator: не удалось выбрать символ по весам, проверь настройки.");
        return entries.Count > 0 ? entries[0].symbol : null;
    }
}

/// <summary>
/// Пара "символ + его текущий рабочий вес". currentWeight отделён от SlotSymbol.weight,
/// чтобы чармы могли временно его менять на один спин, не трогая сам ассет символа.
/// </summary>
[System.Serializable]
public class SymbolWeightEntry
{
    public SlotSymbol symbol;
    public int currentWeight;

    public SymbolWeightEntry(SlotSymbol symbol, int currentWeight)
    {
        this.symbol = symbol;
        this.currentWeight = currentWeight;
    }
}
