using UnityEngine;
using UnityEngine.Events;

public class ObjectPickUp : MonoBehaviour, IInteractable
{

   
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private UnityEvent onPointerEnter;
    [SerializeField] private UnityEvent onPointerExit;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;
    private Renderer objectRenderer;
    private GameObject player;
    private PlayerController playerController;
    [Header("Object Charact")]
    [SerializeField] private string objectName;
   // [SerializeField] private ItemBehavior itemBehavior; 
    [SerializeField] private ItemData itemData;
    private void Huita()
    {
        player = GameObject.FindWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        objectName = itemData.itemName;

    }

    private void Start()
    {
        Huita();
        objectRenderer = GetComponent<Renderer>();
        if (defaultMaterial == null && objectRenderer != null)
            defaultMaterial = objectRenderer.material;
    }

    public void Interact()
    {

        onInteract?.Invoke();
        Debug.Log("Предмет " + objectName);

        // Здесь ваша логика взаимодействия
        playerController.playerinterfaceSpt.inventoryCells.SwithItemIcon(itemData.icon);
    }

    public void OnPointerEnter()
    {
        Debug.Log($"Looking at {objectName}");
        onPointerEnter?.Invoke();
        playerController.SwitchTitleItem(objectName);
        //// Визуальная обратная связь - подсветка
        //if (objectRenderer != null && highlightMaterial != null)
        //    objectRenderer.material = highlightMaterial;

    }

    public void OnPointerStay()
    {
        // Можно добавить визуальные эффекты, например пульсацию
        // Или обновление UI подсказки
    }

    public void OnPointerExit()
    {
        Debug.Log($"Stopped looking at {objectName}");
        onPointerExit?.Invoke();

        // Возвращаем исходный материал
        if (objectRenderer != null && defaultMaterial != null)
            objectRenderer.material = defaultMaterial;
    }
}
