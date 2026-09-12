using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;
    [SerializeField] private PlayerController playerController;

    [Header("Action Map Name Reference")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Input Action Asset")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string interact = "Interact";


    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotarionInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }

    private void Awake()
    {
        InputActionMap mapReference = playerControls.FindActionMap(actionMapName);

        movementAction = mapReference.FindAction(movement);
        rotationAction = mapReference.FindAction(rotation);
        jumpAction = mapReference.FindAction(jump);
        sprintAction = mapReference.FindAction(sprint);
        interactAction = mapReference.FindAction(interact);
        SubscribeActionValuesToInputEvents();
    }
    private void SubscribeActionValuesToInputEvents()
    {
        //movement
        movementAction.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        movementAction.canceled += inputInfo => MovementInput = Vector2.zero;
        //rotation
        rotationAction.performed += inputInfo => RotarionInput = inputInfo.ReadValue<Vector2>();
        rotationAction.canceled += inputInfo => RotarionInput = Vector2.zero;
        //jump
        jumpAction.performed += inputInfo => JumpTriggered = true;
        jumpAction.canceled += inputInfo => JumpTriggered = false;
        // sprint
        sprintAction.performed += inputinfo =>
        {
            Debug.Log($"Sprint action performed! Button pressed");
            SprintTriggered = true;
        };
        sprintAction.canceled += inputInfo =>
        {
            Debug.Log($"Sprint action canceled! Button released");
            SprintTriggered = false;
        };
        interactAction.performed += inputInfo =>
        {

            InteractTriggered = true;
        };
        interactAction.canceled += inputInfo => InteractTriggered = false;
    }

    private void OnEnable()
    {
        playerControls.FindActionMap(actionMapName).Enable();
    }

    private void OnDisable()
    {
        playerControls.FindActionMap(actionMapName).Disable();
    }
}
