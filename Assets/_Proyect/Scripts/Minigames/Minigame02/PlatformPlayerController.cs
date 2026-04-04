using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlatformPlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityScale = 3f;

    [Header("Golpe")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float selfKnockback = 4f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private float invulnerableTime = 2f;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Player1_Platform";

    [Header("Debug")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isInvulnerable;
    [SerializeField] private bool isDead;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool isKnockedBack = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private Vector2 moveInput;

    private PlatformPlayerController otherPlayer;
    private Vector3 spawnPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        SetupInput();
    }

    private void OnDisable()
    {
        moveAction?.Disable();

        if (jumpAction != null)
        {
            jumpAction.performed -= OnJump;
            jumpAction.Disable();
        }

        if (attackAction != null)
        {
            attackAction.performed -= OnAttack;
            attackAction.Disable();
        }
    }

    private void SetupInput()
    {
        if (inputActions == null) return;

        var map = inputActions.FindActionMap(actionMapName);
        if (map == null)
        {
            Debug.LogError("Action map no encontrado: " + actionMapName);
            return;
        }

        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");
        attackAction = map.FindAction("Attack");

        moveAction?.Enable();

        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJump;
        }

        if (attackAction != null)
        {
            attackAction.Enable();
            attackAction.performed += OnAttack;
        }
    }

    private void Update()
    {
        if (isDead) return;
        if (moveAction == null) return;
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.gravityScale = gravityScale;

        // si esta en knockback no sobreescribir la velocidad
        if (!isKnockedBack)
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // --- SALTO ---

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead || isKnockedBack) return;
        if (isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- GOLPE ---

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead || !canAttack || otherPlayer == null) return;

        float distancia = Vector2.Distance(transform.position, otherPlayer.transform.position);

        if (distancia <= attackRange)
        {
            float dirX = otherPlayer.transform.position.x > transform.position.x ? 1f : -1f;
            Vector2 knockDir = new Vector2(dirX, 0.3f).normalized;

            otherPlayer.ReceiveKnockback(knockDir);

            // retroceso propio
            rb.linearVelocity = new Vector2(-dirX * selfKnockback, selfKnockback * 0.3f);
            StartCoroutine(KnockbackDuration());

            StartCoroutine(AttackCooldown());
        }
    }

    public void ReceiveKnockback(Vector2 direction)
    {
        if (isInvulnerable) return;
        rb.linearVelocity = direction * knockbackForce;
        StartCoroutine(KnockbackDuration());
    }

    private IEnumerator KnockbackDuration()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- PINCHOS ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike") && !isDead && !isInvulnerable)
            StartCoroutine(Die());
    }

    // --- SUELO ---

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

    // --- MUERTE Y RESPAWN ---

    private IEnumerator Die()
    {
        isDead = true;
        isKnockedBack = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPoint;
        rb.gravityScale = gravityScale;
        sr.enabled = true;
        isDead = false;

        StartCoroutine(Invulnerable());
    }

    private IEnumerator Invulnerable()
    {
        isInvulnerable = true;

        float elapsed = 0f;
        while (elapsed < invulnerableTime)
        {
            sr.enabled = !sr.enabled;
            elapsed += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        sr.enabled = true;
        isInvulnerable = false;
    }

    // --- METODOS PUBLICOS ---

    public void SetSpawnPoint(Vector3 point)
    {
        spawnPoint = point;
    }

    public void SetOtherPlayer(PlatformPlayerController other)
    {
        otherPlayer = other;
    }

    public void ForceRespawn()
    {
        if (!isDead)
            StartCoroutine(Die());
    }
}