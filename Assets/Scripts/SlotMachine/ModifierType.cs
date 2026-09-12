using UnityEngine;

/// <summary>
/// Одна КАТЕГОРИЯ модификатора (например "Урон", "Прочность", "Спецэффект").
/// ScriptableObject — создаёшь один ассет на каждую категорию.
/// Конкретное числовое значение выбирается из possibleValues в момент выпадения.
/// </summary>
[CreateAssetMenu(fileName = "NewModifierType", menuName = "SlotMachine/Modifier Type")]
public class ModifierType : ScriptableObject
{
    // Название для UI игрока, например "+Урон", "+Прочность", "Проклятие искры"
    public string displayName;

    // Возможные конкретные значения этой категории и их веса.
    // Пример для "Damage": { value: 10, weight: 50 }, { value: 25, weight: 30 }, { value: 50, weight: 5 }
    public ModifierValue[] possibleValues;
}

/// <summary>
/// Одно конкретное значение внутри категории модификатора + вес его выпадения.
/// Не ScriptableObject — просто структура данных внутри ModifierType.
/// </summary>
[System.Serializable]
public struct ModifierValue
{
    public float value;
    public int weight;
}
