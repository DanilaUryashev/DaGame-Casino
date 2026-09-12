using UnityEditor;
using UnityEngine;

/// <summary>
/// Кастомный инспектор для SlotMachineConfig — добавляет кнопку, которая автоматически
/// генерирует набор стандартных паттернов (линии, V-вниз/вверх, X, углы, периметр,
/// крест, ромб, джекпот на всю сетку) под текущий размер сетки (columns x rows),
/// вместо того чтобы прописывать координаты каждой ячейки руками.
///
/// ВАЖНО: этот файл должен лежать в папке с именем "Editor" (SlotMachine/Editor/) —
/// так требует Unity для скриптов редактора. Если перенесёшь файл в другое место
/// без папки Editor — получишь ошибку компиляции в билде (UnityEditor недоступен в игре).
/// </summary>
[CustomEditor(typeof(SlotMachineConfig))]
public class SlotMachineConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SlotMachineConfig config = (SlotMachineConfig)target;

        GUILayout.Space(10);
        GUILayout.Label("Генератор паттернов", EditorStyles.boldLabel);

        if (GUILayout.Button("Заполнить стандартными паттернами"))
        {
            GenerateStandardPatterns(config);
            EditorUtility.SetDirty(config);
        }

        if (GUILayout.Button("Очистить все паттерны"))
        {
            config.patterns.Clear();
            EditorUtility.SetDirty(config);
        }
    }

    private void GenerateStandardPatterns(SlotMachineConfig config)
    {
        config.patterns.Clear();

        int cols = config.columns;
        int rows = config.rows;

        // Простые линии — без бафа по умолчанию (пустой modifierPool), это база.
        for (int row = 0; row < rows; row++)
        {
            AddIfValid(config, $"Линия {row + 1}", PatternPresets.HorizontalLine(row, cols), PatternTier.Common);
        }

        AddIfValid(config, "V вниз", PatternPresets.VShapeDown(cols, rows), PatternTier.Rare);
        AddIfValid(config, "V вверх", PatternPresets.VShapeUp(cols, rows), PatternTier.Rare);
        AddIfValid(config, "X", PatternPresets.XShape(cols, rows), PatternTier.Jackpot);
        AddIfValid(config, "Углы", PatternPresets.Corners(cols, rows), PatternTier.Rare);
        AddIfValid(config, "Периметр", PatternPresets.Perimeter(cols, rows), PatternTier.Jackpot);
        AddIfValid(config, "Крест", PatternPresets.Cross(cols, rows), PatternTier.Jackpot);
        AddIfValid(config, "Ромб", PatternPresets.Diamond(cols, rows), PatternTier.Jackpot);
        AddIfValid(config, "Джекпот (вся сетка)", PatternPresets.FullGrid(cols, rows), PatternTier.Jackpot);

        Debug.Log($"Сгенерировано {config.patterns.Count} паттернов для {config.machineName} ({cols}x{rows}). Пулы модификаторов (modifierPool) пустые — заполни их вручную в инспекторе под каждый паттерн, кроме простых линий.");
    }

    private void AddIfValid(SlotMachineConfig config, string name, Vector2Int[] cells, PatternTier tier)
    {
        if (cells == null || cells.Length == 0)
            return;

        config.patterns.Add(new PaylinePattern
        {
            patternName = name,
            cells = cells,
            tier = tier
        });
    }
}
