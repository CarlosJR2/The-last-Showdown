using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlatformPlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravityScale = 4f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCutMultiplier = 0.85f;
    [SerializeField] private float fallMultiplier = 3f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Golpe")]
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float selfKnockback = 4f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("DNA")]
    public bool hasDNA = false;

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

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpHeld = false;

    // shield
    private bool shieldActive = false;
    private float shieldMultiplier = 1f;

    // doble salto
    private bool doubleJumpEnabled = false;
    private bool usedDoubleJump = false;

    // gravedad pesada
    private bool heavyGravityActive = false;
    private float heavyGravityValue = 0f;

    // control espejo
    private bool mirrorActive = false;
    private PlatformPlayerController mirrorTarget = null;
    private bool isForcedMove = false;
    private Vector2 forcedMoveInput = Vector2.zero;

    // FIX mirror jump: flag procesado en Update con fuerza completa
    private bool mirrorJumpPending = false;

    // controles invertidos
    private bool invertControls = false;

    // jetpack
    private bool jetpackActive = false;
    private float jetpackForce = 0f;

    // FIX hook: override de velocidad total
    private bool hasRawVelocityOverride = false;
    private Vector2 rawVelocityOverride = Vector2.zero;

    [Header("PowerUp")]
    [SerializeField] private PowerUpPickup.PowerUpType currentPowerUp;
    [SerializeField] private bool hasPowerUp = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private Vector2 moveInput;

    private PlatformPlayerController otherPlayer;
    private Vector3 spawnPoint;
    private KingOfHill manager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnEnable() { SetupInput(); }

    private void OnDisable()
    {
        moveAction?.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.canceled -= OnJumpCanceled;
            jumpAction.Disable();
        }
        if (attackAction != null) { attackAction.performed -= OnAttack; attackAction.Disable(); }
        if (interactAction != null) { interactAction.performed -= OnInteract; interactAction.Disable(); }
    }

    private void SetupInput()
    {
        if (inputActions == null) return;
        var map = inputActions.FindActionMap(actionMapName);
        if (map == null) { Debug.LogError("Action map no encontrado: " + actionMapName); return; }

        moveAction = map.FindAction("Move");
        jumpAction = map.FindAction("Jump");
        attackAction = map.FindAction("Attack");
        interactAction = map.FindAction("Interact");

        moveAction?.Enable();
        if (jumpAction != null) { jumpAction.Enable(); jumpAction.performed += OnJumpPerformed; jumpAction.canceled += OnJumpCanceled; }
        if (attackAction != null) { attackAction.Enable(); attackAction.performed += OnAttack; }
        if (interactAction != null) { interactAction.Enable(); interactAction.performed += OnInteract; }
    }

    private void Update()
    {
        if (isDead) return;
        if (moveAction == null) return;

        moveInput = moveAction.ReadValue<Vector2>();
        CheckGround();

        if (invertControls) moveInput = -moveInput;

        if (isGrounded) { coyoteTimeCounter = coyoteTime; if (doubleJumpEnabled) usedDoubleJump = false; }
        else coyoteTimeCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
            if (isGrounded || coyoteTimeCounter > 0f) ExecuteJump();
        }

        // mirror jump: ejecutar con fuerza completa
        if (mirrorJumpPending)
        {
            mirrorJumpPending = false;
            StartCoroutine(MirrorJumpCoroutine());
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        rb.gravityScale = heavyGravityActive ? heavyGravityValue : gravityScale;

        // FIX hook: prioridad maxima, sobreescribe todo
        if (hasRawVelocityOverride)
        {
            rb.linearVelocity = rawVelocityOverride;
            hasRawVelocityOverride = false;
            ApplyBetterGravity();
            return;
        }

        if (jetpackActive && jumpHeld)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jetpackForce);

        if (!isKnockedBack)
        {
            Vector2 inputToUse = isForcedMove ? forcedMoveInput : moveInput;
            rb.linearVelocity = new Vector2(inputToUse.x * moveSpeed, rb.linearVelocity.y);
            isForcedMove = false;
        }

        ApplyBetterGravity();
        ApplyMirrorControl();
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isDead) return;
        jumpHeld = true;

        if (isGrounded || coyoteTimeCounter > 0f)
        {
            ExecuteJump();
            usedDoubleJump = false;
            // propagar salto al mirror como evento puntual
            if (mirrorActive && mirrorTarget != null) mirrorTarget.TriggerMirrorJump();
        }
        else if (doubleJumpEnabled && !usedDoubleJump)
        {
            ExecuteJump();
            usedDoubleJump = true;
            if (mirrorActive && mirrorTarget != null) mirrorTarget.TriggerMirrorJump();
        }
        else
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context) { jumpHeld = false; }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        isGrounded = false;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead || !canAttack || otherPlayer == null) return;
        float distancia = Vector2.Distance(transform.position, otherPlayer.transform.position);
        if (distancia <= attackRange)
        {
            float dirX = otherPlayer.transform.position.x > transform.position.x ? 1f : -1f;
            Vector2 knockDir = new Vector2(dirX, 0.3f).normalized;
            otherPlayer.ReceiveKnockback(knockDir);
            rb.linearVelocity = new Vector2(-dirX * selfKnockback, selfKnockback * 0.3f);
            StartCoroutine(KnockbackDuration());
            StartCoroutine(AttackCooldown());
        }
    }

    public void ReceiveKnockback(Vector2 direction)
    {
        if (isInvulnerable) return;
        if (shieldActive) { otherPlayer.ReceiveKnockback(-direction * shieldMultiplier); return; }
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike") && !isDead && !isInvulnerable)
            StartCoroutine(Die());
    }

    private void CheckGround()
    {

        if (rb.linearVelocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }

        Vector2 leftOrigin = new Vector2(col.bounds.min.x, col.bounds.min.y);
        Vector2 rightOrigin = new Vector2(col.bounds.max.x, col.bounds.min.y);

        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance, groundLayer);

        LayerMask headLayer = 1 << LayerMask.NameToLayer("PlayerHead");
        RaycastHit2D headLeft = Physics2D.Raycast(leftOrigin, Vector2.down, groundCheckDistance, headLayer);
        RaycastHit2D headRight = Physics2D.Raycast(rightOrigin, Vector2.down, groundCheckDistance, headLayer);

        bool onGround = hitLeft.collider != null || hitRight.collider != null;
        bool onHead = (headLeft.collider != null && headLeft.collider.transform.root != transform) ||
                      (headRight.collider != null && headRight.collider.transform.root != transform);

        isGrounded = onGround || onHead;
    }

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

    private void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log(gameObject.name + " presiono interact");
        UsePowerUp();
    }

    private void UsePowerUp()
    {
        if (!hasPowerUp || manager == null)
        {
            Debug.Log(gameObject.name + " intento usar power up - hasPowerUp: " + hasPowerUp + " manager null: " + (manager == null));
            return;
        }
        Debug.Log(gameObject.name + " usando power up: " + currentPowerUp);
        hasPowerUp = false;
        manager.ActivatePowerUp(currentPowerUp, this, otherPlayer);
    }

    public bool HasPowerUp() => hasPowerUp;

    public void ReceivePowerUp(PowerUpPickup.PowerUpType type)
    {
        currentPowerUp = type;
        hasPowerUp = true;
        Debug.Log(gameObject.name + " recibio power up: " + type);
    }

    public void SetShield(bool active, float multiplier) { shieldActive = active; shieldMultiplier = multiplier; }
    public void SetDoubleJump(bool active) { doubleJumpEnabled = active; usedDoubleJump = false; }
    public void SetHeavyGravity(bool active, float gravityValue) { heavyGravityActive = active; heavyGravityValue = gravityValue; }
    public void SetMirrorControl(bool active, PlatformPlayerController target) { mirrorActive = active; mirrorTarget = target; }

    // FIX mirror jump: setar pending, se ejecuta en Update con fuerza completa
    public void TriggerMirrorJump() { mirrorJumpPending = true; }

    private void ApplyMirrorControl()
    {
        if (!mirrorActive || mirrorTarget == null) return;
        mirrorTarget.ForceMove(moveInput);
        // salto va por TriggerMirrorJump, no aqui
    }

    public void ForceMove(Vector2 input) { isForcedMove = true; forcedMoveInput = input; }
    public void ForceJump() { if (isGrounded) ExecuteJump(); }
    public void ForceVelocity(Vector2 velocity) { isForcedMove = true; rb.linearVelocity = velocity; }

    // FIX hook: sobreescribe velocidad total sin pasar por logica de movimiento
    public void ForceVelocityRaw(Vector2 velocity) { hasRawVelocityOverride = true; rawVelocityOverride = velocity; }

    public void SetInvertControls(bool active) { invertControls = active; }
    public void SetJetpack(bool active, float force) { jetpackActive = active; jetpackForce = force; }

    // Exponer collider para que PowerUpEffects pueda hacer IgnoreCollision
    public Collider2D GetCollider() => col;
    public Rigidbody2D GetRigidbody() => rb;

    public void SetSpawnPoint(Vector3 point) { spawnPoint = point; }
    public void SetOtherPlayer(PlatformPlayerController other) { otherPlayer = other; }
    public void SetManager(KingOfHill m) { manager = m; }
    public void ForceRespawn() { if (!isDead) StartCoroutine(Die()); }
    public void ApplyMoveDebuff(float debuff) { moveSpeed *= debuff; }

    public bool HasDNA() => hasDNA;
    public void PickDNA() { hasDNA = true; }
    public void DropDNA() { hasDNA = false; }
    // mirror jump como coroutine para mantener jumpHeld simulado
    // sin esto ApplyBetterGravity corta el salto porque jumpHeld=false en el target
    private IEnumerator MirrorJumpCoroutine()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        // simular jumpHeld por un tiempo corto para que la gravedad no corte el salto
        // el tiempo equivale aproximadamente a la mitad del arco de salto
        bool prevJumpHeld = jumpHeld;
        jumpHeld = true;
        yield return new WaitForSeconds(0.25f);
        jumpHeld = prevJumpHeld;
    }

}
