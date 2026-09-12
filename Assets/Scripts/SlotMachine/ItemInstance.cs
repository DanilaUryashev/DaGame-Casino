using System.Collections.Generic;

/// <summary>
/// Обёртка вокруг ScriptableObject предмета для КОНКРЕТНОГО экземпляра с его бафами.
/// Нужна потому что ItemSO — один общий ассет на ВСЕ копии предмета в игре
/// (нельзя менять его поля напрямую, иначе баф применится ко всем таким предметам разом).
///
/// ВАЖНО: предполагается, что твой класс предмета называется ItemSO. Если у тебя он
/// называется иначе — переименуй свой класс или замени ItemSO на своё имя во всех файлах.
///
/// Кладёшь ItemInstance туда, где раньше лежал голый ItemSO — в слот инвентаря,
/// в предмет на земле и т.д. Если инвентарь сейчас хранит ItemSO напрямую — это
/// единственное место, которое придётся доработать под бафы.
/// </summary>
[System.Serializable]
public class ItemInstance
{
    public ItemSO baseItem;
    public List<AppliedModifier> modifiers = new List<AppliedModifier>();

    public ItemInstance(ItemSO baseItem)
    {
        this.baseItem = baseItem;
    }

    // Суммарный бонус конкретной категории модификатора на этом экземпляре.
    // Пример: itemInstance.GetTotalModifierValue(damageModifierType)
    public float GetTotalModifierValue(ModifierType type)
    {
        float total = 0f;
        foreach (var mod in modifiers)
        {
            if (mod.type == type)
                total += mod.value;
        }
        return total;
    }
}

/// <summary>
/// Один применённый к предмету баф: категория + конкретное значение.
/// </summary>
[System.Serializable]
public class AppliedModifier
{
    public ModifierType type;
    public float value;

    public AppliedModifier(ModifierType type, float value)
    {
        this.type = type;
        this.value = value;
    }
}
