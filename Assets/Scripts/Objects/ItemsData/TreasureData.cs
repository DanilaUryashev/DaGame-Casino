using UnityEngine;

[CreateAssetMenu(fileName = "New Treasure", menuName = "Inventory/Treasure")]
public class TreasureData : ItemData
{
    [Header("Treasure")]
    public int sellPrice;        // цена продажи
    public string treasureType;  // например, "Gem", "Gold", "Artifact" (опционально)
}