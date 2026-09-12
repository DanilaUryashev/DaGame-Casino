using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public bool BigItem=false;
    [TextArea] public string description;

    [Header("World & Audio")]
    public GameObject worldPrefab;
    public AudioClip useSound;

    [HideInInspector] public string guid;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
            guid = System.Guid.NewGuid().ToString();
    }
}