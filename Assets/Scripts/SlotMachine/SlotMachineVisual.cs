using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Всё, что игрок ВИДИТ: анимация прокрутки барабанов и подсветка результата.
/// Ничего не решает сама — просто красиво показывает то, что ей передал Controller.
/// Итоговая сетка уже посчитана заранее — анимация едет к уже готовому ответу,
/// а не генерирует его заново по ходу прокрутки.
/// </summary>
public class SlotMachineVisual : MonoBehaviour
{
    [System.Serializable]
    public class ReelColumn
    {
        // Контейнер с Mask/RectMask2D, показывающий только 3 видимые ячейки этой колонки.
        public RectTransform maskedViewport;

        // Сама "лента" символов внутри маски — длинная, двигается вверх/вниз при прокрутке.
        public RectTransform strip;

        // Заранее созданные Image внутри strip, по одному на каждую позицию ленты.
        // Последние [rows] элементов списка — те, что останутся видны в конце (финальный результат).
        public List<Image> stripSlots;
    }

    [Header("По одной колонке на столбец сетки (5 штук для 5х3)")]
    public ReelColumn[] columns;

    [Header("Тайминг анимации")]
    public float spinDurationPerColumn = 0.9f;
    public float delayBetweenColumns = 0.15f; // колонки останавливаются по очереди слева направо
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Спрайты для визуального шума во время прокрутки")]
    public Sprite[] randomSpinSprites;

    [Header("UI результата")]
    public Transform resultListParent; // куда спавнить строчки результата
    public GameObject resultRowPrefab; // префаб одной строки результата — сделай под свой UI

    /// <summary>
    /// Запускается контроллером после того, как итоговая сетка уже посчитана.
    /// </summary>
    public void PlaySpinAnimation(SlotSymbol[,] finalGrid, List<SpinResult> results)
    {
        StartCoroutine(SpinRoutine(finalGrid, results));
    }

    private IEnumerator SpinRoutine(SlotSymbol[,] finalGrid, List<SpinResult> results)
    {
        ClearResultUI();

        // Стартуем колонки с нарастающей задержкой, не ждём каждую последовательно —
        // так они реально крутятся параллельно и просто останавливаются по очереди.
        for (int col = 0; col < columns.Length; col++)
        {
            int columnIndex = col; // локальная копия для замыкания в корутине
            float startDelay = col * delayBetweenColumns;
            StartCoroutine(SpinSingleColumn(columns[columnIndex], finalGrid, columnIndex, startDelay));
        }

        float totalTime = (columns.Length - 1) * delayBetweenColumns + spinDurationPerColumn;
        yield return new WaitForSeconds(totalTime);

        ShowResults(results);

        SpinResult deathResult = results.Find(r => r.isDeathEffect);
        if (deathResult != null)
        {
            PlayDeathEffect(deathResult);
        }
    }

    private IEnumerator SpinSingleColumn(ReelColumn column, SlotSymbol[,] finalGrid, int columnIndex, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        int rows = finalGrid.GetLength(1);
        int stripLength = column.stripSlots.Count;

        // Заполняем всю ленту случайным визуальным шумом, КРОМЕ последних [rows] элементов —
        // туда сразу пишем финальный результат. Когда лента доедет до конца, там уже
        // физически лежит правильный ответ, никакой подмены в последний момент не нужно.
        for (int i = 0; i < stripLength; i++)
        {
            bool isFinalSlot = i >= stripLength - rows;

            if (isFinalSlot)
            {
                int rowIndex = i - (stripLength - rows);
                column.stripSlots[i].sprite = finalGrid[columnIndex, rowIndex].icon;
            }
            else if (randomSpinSprites.Length > 0)
            {
                column.stripSlots[i].sprite = randomSpinSprites[Random.Range(0, randomSpinSprites.Length)];
            }
        }

        // Двигаем ленту от начальной позиции до финальной через кривую замедления —
        // быстро в начале, плавно тормозит к концу.
        float cellHeight = column.maskedViewport.rect.height / rows;
        float startY = 0f;
        float endY = -(stripLength - rows) * cellHeight;

        float elapsed = 0f;
        Vector2 pos = column.strip.anchoredPosition;

        while (elapsed < spinDurationPerColumn)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsed / spinDurationPerColumn);
            pos.y = Mathf.Lerp(startY, endY, t);
            column.strip.anchoredPosition = pos;
            yield return null;
        }

        pos.y = endY;
        column.strip.anchoredPosition = pos;
    }

    private void ShowResults(List<SpinResult> results)
    {
        foreach (var result in results)
        {
            BuildResultRow(result);
        }
    }

    private void BuildResultRow(SpinResult result)
    {
        if (resultRowPrefab == null || resultListParent == null)
            return;

        GameObject row = Instantiate(resultRowPrefab, resultListParent);

        // TODO: достань компоненты из row (Text/Image) и заполни их данными result —
        // структура зависит от твоего конкретного префаба строки, поэтому оставлено
        // как точка интеграции, а не жёсткий код под несуществующий префаб.
    }

    private void ClearResultUI()
    {
        if (resultListParent == null)
            return;

        foreach (Transform child in resultListParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void PlayDeathEffect(SpinResult deathResult)
    {
        // TODO: подставь свою реальную презентацию — тряска экрана, красная вспышка, звук,
        // и если isTeamWipe — что-то более масштабное на весь экран для всей команды.
        Debug.Log(deathResult.isTeamWipe
            ? "TEAM WIPE — все умерли от джекпота черепов"
            : $"Урон от черепа: {deathResult.damageAmount}");
    }
}
