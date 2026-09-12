using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject PlayerInterfaceGameObject;
    public Interface playerinterfaceSpt;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMulripler = 1.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravityMultipler = 1.0f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float upDownLookRange = 80f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameInput gameInput;

    

    private float interactionRange = 3f;
    Vector3 rayDirection; // направление камеры

    private Vector3 currentMovement;
    private float verticalRotation;
    private float CurrentSpeed => walkSpeed * (gameInput.SprintTriggered? sprintMulripler:1);

    private bool isGrounded => characterController.isGrounded;

    // Для отслеживания интерактивных объектов
    [SerializeField] private LayerMask interactionLayer = ~0; // Все слои по умолчанию
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;

    private void Start()
    {
        playerinterfaceSpt = PlayerInterfaceGameObject.GetComponent<Interface>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleInteraction();
    }

    private Vector3 CalculateWorldDirection()
    {
        Vector3 inputDirection = new Vector3(gameInput.MovementInput.x, 0f, gameInput.MovementInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection.normalized;
    }
    private void HandleJumping()
    {
        if (isGrounded)
        {
            currentMovement.y = -0.5f;
            if (gameInput.JumpTriggered)
            {
                currentMovement.y = jumpForce; 
            }
        }
        else
        {
            currentMovement.y += Physics.gravity.y * gravityMultipler * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        Vector3 worldDirrection = CalculateWorldDirection();
        currentMovement.x = worldDirrection.x * CurrentSpeed;
        currentMovement.z = worldDirrection.z * CurrentSpeed;

        HandleJumping();
        characterController.Move(currentMovement*Time.deltaTime);
    }


    private void ApplyHorizontalRotation(float rotateAmount)
    {
        transform.Rotate(0, rotateAmount, 0);
    }

    private void ApplyVerticallRotation(float rotateAmount)
    {
        verticalRotation = Mathf.Clamp(verticalRotation - rotateAmount, -upDownLookRange, upDownLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleRotation()
    {
        
        float mouseXRotation = gameInput.RotarionInput.x * mouseSensitivity;
        float mouseYRotation = gameInput.RotarionInput.y * mouseSensitivity;

        ApplyHorizontalRotation(mouseXRotation);
        ApplyVerticallRotation(mouseYRotation);
    }
    public void HandleInteraction()
    {
        RaycastHit hit;
        Vector3 rayOrigin = mainCamera.transform.position;
        rayDirection = mainCamera.transform.forward;

        bool hitSomething = Physics.Raycast(rayOrigin, rayDirection, out hit, interactionRange, interactionLayer);

        if (hitSomething)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            GameObject hitObject = hit.collider.gameObject;

            if (interactable != null)
            {
                // Если навели на новый объект
                if (currentInteractableObject != hitObject)
                {
                    // Выход из предыдущего объекта
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnPointerExit();
                    }

                    // Вход в новый объект
                    currentInteractable = interactable;
                    currentInteractableObject = hitObject;
                    currentInteractable.OnPointerEnter();
                }

                // Каждый кадр, пока наведены на объект
                currentInteractable.OnPointerStay();

                // Проверка нажатия для взаимодействия
                if (gameInput.InteractTriggered)
                {
                    Debug.Log("InteractTriggered is TRUE!");
                    currentInteractable.Interact();
                }
            }
            else
            {
                // Навели на объект без интерфейса, сбрасываем текущий
                ClearCurrentInteractable();
            }
        }
        else
        {
            // Луч ни во что не попал, сбрасываем
            ClearCurrentInteractable();
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnPointerExit();
            currentInteractable = null;
            currentInteractableObject = null;
            SwitchTitleItem("");
        }
    }

    public void SwitchTitleItem(string TitleItem)
    {
        playerinterfaceSpt.UITitleObject.text = TitleItem;
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        // Draw the ray in the Scene view (используем направление камеры)
        Gizmos.color = Color.green;
        Vector3 rayDirection = mainCamera.transform.forward * interactionRange;
        Gizmos.DrawRay(mainCamera.transform.position, rayDirection);

        // Draw a sphere at the end point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mainCamera.transform.position + rayDirection, 0.1f);

        // Optional: Draw a small sphere at camera position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mainCamera.transform.position, 0.05f);
    }


}
