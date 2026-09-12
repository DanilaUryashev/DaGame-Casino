using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    [Header("Consumable Stats")]
    public int healAmount;      // сколько восстанавливает здоровья
    public int manaAmount;       // или маны
    public float cooldown;       // перезарядка после использования
    public float duration;       // длительность эффекта (если баф)
    public ParticleSystem useEffect; // визуальный эффект
}