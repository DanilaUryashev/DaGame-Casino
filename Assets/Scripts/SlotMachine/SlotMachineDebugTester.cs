using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Тестовый компонент — крутит слот-машину и печатает сетку + результаты в консоль,
/// без необходимости строить UI. Повесь на любой объект в сцене (например тот же,
/// где стоит SlotMachineController), назначь ссылку на controller, и в Play Mode
/// через правый клик на компонент в инспекторе -> "Test Spin".
///
/// Это ровно шаги 2-4 из README ("Порядок сборки") — просто готовым инструментом
/// вместо ручных Debug.Log на каждом шаге.
/// </summary>
public class SlotMachineDebugTester : MonoBehaviour
{
    public SlotMachineController controller;

    private void OnEnable()
    {
        if (controller != null)
        {
            controller.OnGridGenerated += PrintGrid;
            controller.OnSpinResolved += PrintResults;
        }
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnGridGenerated -= PrintGrid;
            controller.OnSpinResolved -= PrintResults;
        }
    }

    [ContextMenu("Test Spin")]
    public void TestSpin()
    {
        if (controller == null)
        {
            Debug.LogError("SlotMachineDebugTester: не назначен controller.");
            return;
        }

        controller.Spin(new DummySpinner());
    }

    private void PrintGrid(SlotSymbol[,] grid)
    {
        int columns = grid.GetLength(0);
        int rows = grid.GetLength(1);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== СЕТКА ===");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                SlotSymbol symbol = grid[col, row];
                sb.Append((symbol != null ? symbol.name : "?").PadRight(12));
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    private void PrintResults(List<SpinResult> results)
    {
        if (results.Count == 0)
        {
            Debug.Log("=== РЕЗУЛЬТАТ: пусто, ничего не выпало ===");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== РЕЗУЛЬТАТ: {results.Count} результат(ов) ===");

        foreach (var result in results)
        {
            if (result.isDeathEffect)
            {
                sb.AppendLine(result.isTeamWipe
                    ? "  ЧЕРЕП — ВАЙП КОМАНДЫ"
                    : $"  ЧЕРЕП — урон {result.damageAmount}");
                continue;
            }

            string itemName = (result.itemAwarded != null && result.itemAwarded.baseItem != null)
                ? result.itemAwarded.baseItem.name
                : "(без привязанного ItemSO)";

            int modifierCount = result.itemAwarded != null ? result.itemAwarded.modifiers.Count : 0;

            sb.AppendLine($"  {result.label} -> {itemName}, паттернов: {result.contributingPatterns.Count}, бафов: {modifierCount}");

            if (result.itemAwarded != null)
            {
                foreach (var mod in result.itemAwarded.modifiers)
                {
                    sb.AppendLine($"      + {mod.type.displayName}: {mod.value}");
                }
            }
        }

        Debug.Log(sb.ToString());
    }

    // Заглушка ISlotSpinner для тестов — бесконечная валюта, эффекты просто логируются,
    // никто реально не платит и не умирает.
    private class DummySpinner : ISlotSpinner
    {
        public bool TrySpendCurrency(int amount)
        {
            Debug.Log($"[Test] Списано (условно) {amount} валюты.");
            return true;
        }

        public void ApplyDamage(float amount)
        {
            Debug.Log($"[Test] Получен урон: {amount}");
        }

        public void Kill()
        {
            Debug.Log("[Test] ИГРОК УБИТ (тестовый вызов).");
        }
    }
}
