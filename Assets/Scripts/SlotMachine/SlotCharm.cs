using UnityEngine;

// Что именно делает чарм при применении перед спином.
public enum CharmEffectType
{
    RemoveJunk,         // временно снижает/убирает вес мусорных символов на этот спин
    BoostPatternWeight, // временно повышает шанс не-мусорных символов (см. CharmApplier TODO)
    GuaranteeHit,       // гарантирует хотя бы одно совпадение на этот спин
    FreeSpin            // даёт дополнительный спин без повторного списания стоимости
}

/// <summary>
/// Расходный предмет, который тратится ДО спина, чтобы временно подкрутить шансы машины.
/// Источники — крафт, квесты, дроп с самих машин (три канала сразу, как и обсуждали).
/// </summary>
[CreateAssetMenu(fileName = "NewSlotCharm", menuName = "SlotMachine/Charm")]
public class SlotCharm : ScriptableObject
{
    public string charmName;
    public Sprite icon;

    public CharmEffectType effectType;

    // Смысл зависит от effectType:
    // RemoveJunk         -> на сколько снизить вес мусорных символов, 0..1 (1 = убрать полностью)
    // BoostPatternWeight -> во сколько раз умножить вес не-мусорных символов
    // GuaranteeHit        -> не используется, это просто bool-эффект
    // FreeSpin            -> не используется, это просто bool-эффект
    public float effectStrength = 1f;

    // Используется только для BoostPatternWeight — какой тир паттернов имеется в виду
    // (сейчас в CharmApplier не используется напрямую, оставлено для твоей доработки под
    // более точечный буст, см. TODO в CharmApplier.cs).
    public PatternTier targetTier;
}
