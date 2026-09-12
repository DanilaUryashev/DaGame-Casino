using System.Collections.Generic;
using UnityEngine;

// Уровень "силы" паттерна. Влияет на то, какой пул бафов ему доступен,
// и (для черепов) насколько тяжёлый эффект он даёт.
public enum PatternTier
{
    Common,   // простая линия — обычно вообще без бафа
    Rare,     // диагональ, углы, зигзаг — баф послабее
    Jackpot   // ромб, крест, периметр, полная сетка — топ бафы / вайп для черепов
}

/// <summary>
/// Описание одной линии выигрыша: какие координаты сетки в неё входят,
/// насколько она "сильная" и какие модификаторы может дать при совпадении.
/// НЕ ScriptableObject — обычный список внутри SlotMachineConfig, редактируется
/// прямо в инспекторе конкретной машины (для LUCK/FORTUNE/FATE список может отличаться).
/// </summary>
[System.Serializable]
public class PaylinePattern
{
    // Для читаемости в инспекторе: "Линия", "Диагональ", "Ромб", "Периметр"...
    public string patternName;

    // x = столбец (0..columns-1), y = строка (0..rows-1).
    // Прописываются руками один раз при настройке ассета конфига.
    public Vector2Int[] cells;

    // Влияет на пул модификаторов ниже и на тяжесть эффекта, если паттерн собрался черепами.
    public PatternTier tier;

    // Пул модификаторов, которые МОГУТ выпасть, если паттерн совпал оружием (не черепом, не мусором).
    // Пустой список = паттерн даёт предмет без бафа (случай простой линии).
    public List<WeightedModifierEntry> modifierPool = new List<WeightedModifierEntry>();
}

/// <summary>
/// Один вариант в списке "какой тип модификатора может выпасть и с каким весом".
/// Например: { Durability, weight: 60 }, { Damage, weight: 30 }, { SpecialAffix, weight: 10 }
/// </summary>
[System.Serializable]
public class WeightedModifierEntry
{
    public ModifierType modifierType;
    public int weight = 10;
}
