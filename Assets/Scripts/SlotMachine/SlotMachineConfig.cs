using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Полный набор настроек одной слот-машины. Один и тот же код (Controller, GridGenerator,
/// RewardResolver) работает для LUCK/FORTUNE/FATE — им просто подсовывается разный конфиг.
/// Создай 3 ассета: LUCK_Config, FORTUNE_Config, FATE_Config.
/// </summary>
[CreateAssetMenu(fileName = "NewSlotMachineConfig", menuName = "SlotMachine/Machine Config")]
public class SlotMachineConfig : ScriptableObject
{
    [Header("Общее")]
    public string machineName; // "LUCK" / "FORTUNE" / "FATE" — для UI и дебага
    public int spinCost = 3;   // сколько валюты/жетонов стоит один спин

    [Header("Сетка")]
    public int columns = 5;
    public int rows = 3;

    [Header("Символы")]
    // Все символы, участвующие в ЭТОЙ конкретной машине (LUCK не включает символы FORTUNE
    // и наоборот). Веса берутся из самого SlotSymbol.weight каждого элемента.
    public List<SlotSymbol> symbols = new List<SlotSymbol>();

    [Header("Паттерны")]
    public List<PaylinePattern> patterns = new List<PaylinePattern>();

    [Header("Резервные ID (должны совпадать с тем, что проставлено в SlotSymbol)")]
    public int junkSymbolId = -1;
    public int skullSymbolId = -2;
}
