using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Готовые генераторы координат для типовых паттернов слот-машины — чтобы не набирать
/// Vector2Int[] руками для каждой линии. Работает для любых columns x rows, но заточено
/// под стандартную пропорцию слотов (например 5x3).
///
/// ВАЖНО про диагонали: на НЕ квадратной сетке (5 столбцов x 3 строки) геометрическая
/// диагональ "от угла до угла" не имеет смысла — 5 столбцов физически не помещаются
/// на линию с 3 возможными строками без повторов. Поэтому "диагональные" линии здесь
/// заданы как V-образный маршрут (одна строка на каждый столбец) — это ровно то,
/// как задаются линии выплат в настоящих слот-машинах.
/// </summary>
public static class PatternPresets
{
    // Одна горизонтальная линия — все столбцы на одной и той же строке.
    public static Vector2Int[] HorizontalLine(int row, int columns)
    {
        Vector2Int[] cells = new Vector2Int[columns];
        for (int col = 0; col < columns; col++)
            cells[col] = new Vector2Int(col, row);
        return cells;
    }

    // Линия по произвольному маршруту: rowPerColumn[i] — номер строки для столбца i.
    // Из этого строятся V-образные, зигзагообразные и любые другие "ломаные" линии,
    // если готовые методы ниже не подходят под нужный тебе размер сетки.
    public static Vector2Int[] CustomRoute(int[] rowPerColumn)
    {
        Vector2Int[] cells = new Vector2Int[rowPerColumn.Length];
        for (int col = 0; col < rowPerColumn.Length; col++)
            cells[col] = new Vector2Int(col, rowPerColumn[col]);
        return cells;
    }

    // V-образная линия вниз: от краёв к центру, ныряя к нижней строке в середине.
    // Под 5x3 даёт маршрут {0,1,2,1,0}.
    public static Vector2Int[] VShapeDown(int columns, int rows)
    {
        int[] route = BuildVRoute(columns, rows, invert: false);
        return route != null ? CustomRoute(route) : null;
    }

    // Обратная V (домиком): от краёв к центру, поднимаясь к верхней строке в середине.
    public static Vector2Int[] VShapeUp(int columns, int rows)
    {
        int[] route = BuildVRoute(columns, rows, invert: true);
        return route != null ? CustomRoute(route) : null;
    }

    private static int[] BuildVRoute(int columns, int rows, bool invert)
    {
        int half = columns / 2;

        // Нужно нечётное количество столбцов (чтобы был чёткий центр) и достаточно строк,
        // чтобы маршрут дотянулся от края до центра не выходя за сетку.
        if (columns % 2 == 0 || half >= rows)
        {
            Debug.LogWarning("PatternPresets: V-образная линия не собирается автоматически под этот размер сетки — задай маршрут вручную через CustomRoute.");
            return null;
        }

        int[] route = new int[columns];
        for (int col = 0; col < columns; col++)
        {
            int distanceFromCenter = Mathf.Abs(col - half);
            route[col] = invert ? (rows - 1 - distanceFromCenter) : distanceFromCenter;
        }
        return route;
    }

    // 4 угла сетки.
    public static Vector2Int[] Corners(int columns, int rows)
    {
        return new Vector2Int[]
        {
            new Vector2Int(0, 0),
            new Vector2Int(columns - 1, 0),
            new Vector2Int(0, rows - 1),
            new Vector2Int(columns - 1, rows - 1)
        };
    }

    // Вся рамка по краям сетки.
    public static Vector2Int[] Perimeter(int columns, int rows)
    {
        var set = new HashSet<Vector2Int>();

        for (int col = 0; col < columns; col++)
        {
            set.Add(new Vector2Int(col, 0));
            set.Add(new Vector2Int(col, rows - 1));
        }

        for (int row = 1; row < rows - 1; row++)
        {
            set.Add(new Vector2Int(0, row));
            set.Add(new Vector2Int(columns - 1, row));
        }

        return set.ToArray();
    }

    // Ромб: верх-центр, лево-центр, самый центр, право-центр, низ-центр.
    public static Vector2Int[] Diamond(int columns, int rows)
    {
        if (columns < 3 || rows < 3)
        {
            Debug.LogWarning("PatternPresets: Ромб требует минимум 3x3 — задай координаты вручную для этого размера.");
            return null;
        }

        int midCol = columns / 2;
        int midRow = rows / 2;

        return new Vector2Int[]
        {
            new Vector2Int(midCol, 0),
            new Vector2Int(0, midRow),
            new Vector2Int(midCol, midRow),
            new Vector2Int(columns - 1, midRow),
            new Vector2Int(midCol, rows - 1)
        };
    }

    // Крест: вся центральная строка + вся центральная колонка.
    public static Vector2Int[] Cross(int columns, int rows)
    {
        int midCol = columns / 2;
        int midRow = rows / 2;

        var set = new HashSet<Vector2Int>();

        for (int col = 0; col < columns; col++)
            set.Add(new Vector2Int(col, midRow));

        for (int row = 0; row < rows; row++)
            set.Add(new Vector2Int(midCol, row));

        return set.ToArray();
    }

    // X: обе V-образные линии (вниз и вверх) сразу — визуально похоже на букву X
    // даже на не квадратной сетке, потому что обе линии пересекаются в центре.
    public static Vector2Int[] XShape(int columns, int rows)
    {
        int[] down = BuildVRoute(columns, rows, invert: false);
        int[] up = BuildVRoute(columns, rows, invert: true);

        if (down == null || up == null)
            return null;

        var set = new HashSet<Vector2Int>();
        for (int col = 0; col < columns; col++)
        {
            set.Add(new Vector2Int(col, down[col]));
            set.Add(new Vector2Int(col, up[col]));
        }

        return set.ToArray();
    }

    // Джекпот — вся сетка целиком, одним и тем же символом.
    public static Vector2Int[] FullGrid(int columns, int rows)
    {
        var list = new List<Vector2Int>();
        for (int col = 0; col < columns; col++)
            for (int row = 0; row < rows; row++)
                list.Add(new Vector2Int(col, row));
        return list.ToArray();
    }
}
