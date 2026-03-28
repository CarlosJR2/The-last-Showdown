using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityScale = 3f;

    [Header("TopDown Movement")]
    [SerializeField] private float topDownSpeed = 5f;

    [Header("Movement Mode")]
    [SerializeField] private MovementMode movementMode = MovementMode.Platform;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player1_Platform";

    [Header("Debug")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private Vector2 moveInput;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    public enum MovementMode { Platform, TopDown }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActions no asignado en " + gameObject.name);
            return;
        }
        SetupInput(actionMapName);
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }
    }

    private void Update()
    {
        if (moveAction == null) return;
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (movementMode == MovementMode.Platform)
            HandlePlatformMovement();
        else
            HandleTopDownMovement();
    }

    private void HandlePlatformMovement()
    {
        rb.gravityScale = gravityScale;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleTopDownMovement()
    {
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(moveInput.x * topDownSpeed, moveInput.y * topDownSpeed);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    private void SetupInput(string mapName)
    {
        moveAction?.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }

        var map = inputActions.FindActionMap(mapName);
        if (map == null)
        {
            Debug.LogError("Action Map no encontrado: " + mapName);
            return;
        }

        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");

        moveAction?.Enable();
        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJump;
        }
    }

    public void SetMovementMode(MovementMode mode, string mapName)
    {
        movementMode = mode;
        actionMapName = mapName;
        SetupInput(mapName);
    }
}