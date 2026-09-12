using UnityEngine;
using UnityEngine.UI;

public class InventoryCells : MonoBehaviour
{
    public bool occupiedCells;
    [SerializeField] private Image itemIcon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if( !occupiedCells)
        {
            Color color = itemIcon.color;
            // Установить альфа (0 = полностью прозрачный, 1 = полностью непрозрачный)
            color.a = 0f; // 0% прозрачности
            itemIcon.color = color;
        }
        else
        {
            Color color = itemIcon.color;
            // Установить альфа (0 = полностью прозрачный, 1 = полностью непрозрачный)
            color.a = 1f; // 100% прозрачности
            itemIcon.color = color;
        }
    }
    public void SwithItemIcon(Sprite iconSprite)
    {
        Debug.Log("color=red ASDASSADKJD");
        itemIcon.sprite = iconSprite;
        occupiedCells = true;
        UpdateOccupiedCells();
    }
    private void UpdateOccupiedCells()
    {
        if (!occupiedCells)
        {
            Color color = itemIcon.color;
            // Установить альфа (0 = полностью прозрачный, 1 = полностью непрозрачный)
            color.a = 0f; // 0% прозрачности
            itemIcon.color = color;
        }
        else
        {
            Color color = itemIcon.color;
            // Установить альфа (0 = полностью прозрачный, 1 = полностью непрозрачный)
            color.a = 1f; // 100% прозрачности
            itemIcon.color = color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
