using UnityEngine;

public class DiskMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float initialSpeed = 4f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 0.1f;

    [Header("Debug")]
    [SerializeField] private float currentSpeed;

    private Rigidbody2D rb;
    private Vector2 direction;
    private DodgeDisk dodgeDisk;
    private bool moving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dodgeDisk = FindFirstObjectByType<DodgeDisk>();
    }

    private void FixedUpdate()
    {
        if (!moving) return;

        // aumento constante de velocidad
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, maxSpeed);
        rb.linearVelocity = direction * currentSpeed;
    }

    public void Launch()
    {
        direction = Random.insideUnitCircle.normalized;

        while (Mathf.Abs(direction.x) < 0.2f || Mathf.Abs(direction.y) < 0.2f)
        {
            direction = Random.insideUnitCircle.normalized;
        }

        currentSpeed = initialSpeed;
        moving = true;
        rb.linearVelocity = direction * currentSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        direction = Vector2.Reflect(direction, normal).normalized;
        rb.linearVelocity = direction * currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player1"))
            dodgeDisk.TryHitPlayer(1);
        else if (other.CompareTag("Player2"))
            dodgeDisk.TryHitPlayer(2);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        rb.linearVelocity = direction * currentSpeed;
    }

    public void Stop()
    {
        moving = false;
        rb.linearVelocity = Vector2.zero;
        direction = Vector2.zero;
    }
}