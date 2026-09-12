using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon Stats")]
    public int damage;
    public float attackSpeed;
    public float range;
    public float criticalChance; // шанс крита
}