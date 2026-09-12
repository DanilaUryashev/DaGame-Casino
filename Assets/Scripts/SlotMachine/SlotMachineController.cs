using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный "дирижёр" одной слот-машины. Вешается на объект машины в сцене вместе
/// с SlotMachineVisual. Хранит ссылку на конфиг (LUCK/FORTUNE/FATE), принимает
/// команду "крутить" от игрока, вызывает генерацию сетки и резолвинг результата,
/// оповещает визуал и остальные системы через события.
/// </summary>
public class SlotMachineController : MonoBehaviour
{
    [Header("Настройка")]
    public SlotMachineConfig config;

    [Header("Визуал")]
    public SlotMachineVisual visual;

    // Чармы, применённые игроком перед конкретным спином. Заполняется извне
    // (например твоим UI выбора чарма через QueueCharm) перед вызовом Spin().
    private List<SlotCharm> pendingCharms = new List<SlotCharm>();

    // Другие системы (звук, статистика, ачивки) подписываются на эти события,
    // а не вызываются напрямую отсюда — так их можно добавлять не трогая этот скрипт.
    public event System.Action<List<SpinResult>> OnSpinResolved;
    public event System.Action<SpinResult> OnDeathEffectTriggered;

    // Чисто для отладки/тестов — сырая сетка до подсчёта результатов (см. SlotMachineDebugTester).
    // Основной логике эта информация не нужна, можно смело игнорировать в проде.
    public event System.Action<SlotSymbol[,]> OnGridGenerated;

    // Добавить чарм в очередь перед спином (например по клику на иконку чарма в UI).
    public void QueueCharm(SlotCharm charm)
    {
        pendingCharms.Add(charm);
    }

    public void ClearQueuedCharms()
    {
        pendingCharms.Clear();
    }

    /// <summary>
    /// Вызови по нажатию кнопки "Крутить". Списывает стоимость и запускает спин.
    /// spinner — тот, кто платит и рискует (реализует ISlotSpinner на твоём PlayerController).
    /// </summary>
    public void Spin(ISlotSpinner spinner)
    {
        if (!spinner.TrySpendCurrency(config.spinCost))
        {
            Debug.Log("Недостаточно валюты для спина.");
            return;
        }

        PerformSpin(spinner);
    }

    // Собственно логика одного прогона — отдельно от Spin(), чтобы бесплатный спин
    // (чарм FreeSpin) мог вызвать её напрямую, не проходя повторно через списание валюты.
    private void PerformSpin(ISlotSpinner spinner)
    {
        // Шаг 1 — веса на этот спин (с учётом чармов, если есть)
        List<SymbolWeightEntry> workingWeights = CharmApplier.BuildWorkingWeights(config, pendingCharms);

        // Шаг 2 — генерация сетки
        SlotSymbol[,] grid = GridGenerator.GenerateGrid(config.columns, config.rows, workingWeights);
        OnGridGenerated?.Invoke(grid);

        // Шаг 3 — резолв + гарантия хотя бы одного совпадения, если такой чарм применён.
        bool guaranteeHit = pendingCharms.Exists(c => c.effectType == CharmEffectType.GuaranteeHit);
        List<SpinResult> results = RewardResolver.Resolve(grid, config);

        if (guaranteeHit && results.Count == 0)
        {
            grid = ForceOnePattern(grid, config);
            results = RewardResolver.Resolve(grid, config);
        }

        // Чармы одноразовые — использовались, очищаем очередь ДО возможного рекурсивного вызова.
        bool freeSpinQueued = pendingCharms.Exists(c => c.effectType == CharmEffectType.FreeSpin);
        ClearQueuedCharms();

        // Шаг 4 — применяем эффекты результата к игроку.
        foreach (var result in results)
        {
            if (result.isDeathEffect)
            {
                if (result.isTeamWipe)
                {
                    // TODO: замени на вызов своего менеджера команды/сессии, если вайп
                    // должен убивать всех, а не только того кто крутил.
                    spinner.Kill();
                }
                else
                {
                    spinner.ApplyDamage(result.damageAmount);
                }

                OnDeathEffectTriggered?.Invoke(result);
            }
            else
            {
                // TODO: здесь подключи свой существующий спавн предмета в мире —
                // предметы падают на землю рядом с машиной, а не в инвентарь напрямую.
            }
        }

        OnSpinResolved?.Invoke(results);
        visual?.PlaySpinAnimation(grid, results);

        if (freeSpinQueued)
        {
            PerformSpin(spinner); // бесплатно — валюта здесь уже не списывается повторно
        }
    }

    // Форсирует один паттерн вручную (для чарма GuaranteeHit) — берёт первый паттерн
    // из списка и красит его ячейки случайно выбранным не мусорным/не черепным символом.
    private SlotSymbol[,] ForceOnePattern(SlotSymbol[,] grid, SlotMachineConfig config)
    {
        if (config.patterns.Count == 0)
            return grid;

        PaylinePattern targetPattern = config.patterns[0];
        SlotSymbol forcedSymbol = config.symbols.Find(s => !s.isJunk && !s.isSkull);
        if (forcedSymbol == null)
            return grid;

        foreach (var cell in targetPattern.cells)
        {
            grid[cell.x, cell.y] = forcedSymbol;
        }

        return grid;
    }
}
