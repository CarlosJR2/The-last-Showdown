using UnityEngine;
using System.Collections;

public class PowerUpEffects : MonoBehaviour
{
    [Header("Jaula")]
    [SerializeField] private GameObject cagePrefab;
    [SerializeField] private float cageDuration = 5f;
    [SerializeField] private Transform hardPointTransform;

    [Header("Escudo")]
    [SerializeField] private float shieldDuration = 4f;
    [SerializeField] private float shieldKnockbackMultiplier = 3f;

    [Header("Quitar Plataforma")]
    [SerializeField] private float platformDisableDuration = 3f;

    [Header("Gancho")]
    [SerializeField] private float hookSpeed = 15f;

    [Header("Doble Salto")]
    [SerializeField] private float doubleJumpDuration = 6f;

    [Header("Gravedad")]
    [SerializeField] private float heavyGravityScale = 15f;
    [SerializeField] private float heavyGravityDuration = 3f;

    [Header("Control Espejo")]
    [SerializeField] private float mirrorDuration = 4f;

    // JAULA 
    public IEnumerator ActivateCage(Transform hardPoint)
    {
        GameObject cage = Instantiate(cagePrefab, hardPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(cageDuration);
        Destroy(cage);
    }

    // ESCUDO
    public IEnumerator ActivateShield(PlatformPlayerController user)
    {
        user.SetShield(true, shieldKnockbackMultiplier);
        yield return new WaitForSeconds(shieldDuration);
        user.SetShield(false, 1f);
    }

    // QUITAR PLATAFORMA
    public IEnumerator ActivateRemovePlatform(PlatformPlayerController target)
    {
        // buscar la plataforma debajo del target
        Collider2D platform = GetPlatformBelow(target);

        if (platform != null)
        {
            platform.enabled = false;
            yield return new WaitForSeconds(platformDisableDuration);
            platform.enabled = true;
        }
    }

    private Collider2D GetPlatformBelow(PlatformPlayerController target)
    {
        Collider2D col = target.GetComponent<Collider2D>();
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 1f, LayerMask.GetMask("Ground"));
        return hit.collider;
    }

    //  GANCHO 
    public IEnumerator ActivateHook(PlatformPlayerController user, PlatformPlayerController target)
    {
        // direccion del user al target
        Vector2 dir = (user.transform.position - target.transform.position).normalized;

        // aplicar fuerza al target hacia el user
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

        float elapsed = 0f;
        float hookTime = 0.3f;

        while (elapsed < hookTime)
        {
            dir = (user.transform.position - target.transform.position).normalized;
            targetRb.linearVelocity = dir * hookSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // DOBLE SALTO 
    public IEnumerator ActivateDoubleJump(PlatformPlayerController user)
    {
        user.SetDoubleJump(true);
        yield return new WaitForSeconds(doubleJumpDuration);
        user.SetDoubleJump(false);
    }

    //  GRAVEDAD AUMENTADA 
    public IEnumerator ActivateHeavyGravity(PlatformPlayerController target)
    {
        target.SetHeavyGravity(true, heavyGravityScale);
        yield return new WaitForSeconds(heavyGravityDuration);
        target.SetHeavyGravity(false, 0f);
    }

    //  CONTROL ESPEJO 
    public IEnumerator ActivateMirrorControl(PlatformPlayerController user, PlatformPlayerController target)
    {
        user.SetMirrorControl(true, target);
        yield return new WaitForSeconds(mirrorDuration);
        user.SetMirrorControl(false, null);
    }
}
