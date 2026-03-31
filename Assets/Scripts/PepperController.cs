using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 0.1f;
    public float maxLookAngle = 80f;

    [Header("References")]
    public Camera playerCamera;
    public InputActionAsset inputActions;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;

    private float cameraPitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        var playerMap = inputActions.FindActionMap("Player");
        moveAction = playerMap.FindAction("Move");
        lookAction = playerMap.FindAction("Look");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;


        Vector3 velocity = move * moveSpeed;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        // Vertical (camera)
        cameraPitch -= lookInput.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Horizontal (body)
        transform.Rotate(Vector3.up * lookInput.x);
    }
}
