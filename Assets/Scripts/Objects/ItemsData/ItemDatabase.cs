using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    [Header("Item Collections")]
    public List<WeaponData> weapons = new List<WeaponData>();
    public List<ConsumableData> consumables = new List<ConsumableData>();
    public List<TreasureData> treasures = new List<TreasureData>();

    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else Destroy(gameObject);
    }

    void InitializeDatabase()
    {
        LoadAllItems();
        foreach (var item in GetAllItems())
            if (!itemDictionary.ContainsKey(item.itemName))
                itemDictionary.Add(item.itemName, item);
    }

    void LoadAllItems()
    {
        weapons = Resources.LoadAll<WeaponData>("Items/Weapons").ToList();
        consumables = Resources.LoadAll<ConsumableData>("Items/Consumables").ToList();
        treasures = Resources.LoadAll<TreasureData>("Items/Treasures").ToList();
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> all = new List<ItemData>();
        all.AddRange(weapons);
        all.AddRange(consumables);
        all.AddRange(treasures);
        return all;
    }

    public ItemData GetItemByName(string name)
    {
        return itemDictionary.ContainsKey(name) ? itemDictionary[name] : null;
    }

    public List<ItemData> GetItemsByType(System.Type type)
    {
        if (type == typeof(WeaponData)) return new List<ItemData>(weapons);
        if (type == typeof(ConsumableData)) return new List<ItemData>(consumables);
        if (type == typeof(TreasureData)) return new List<ItemData>(treasures);
        return new List<ItemData>();
    }
}