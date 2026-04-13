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
    [SerializeField] private LineRenderer hookLine;

    [Header("Doble Salto")]
    [SerializeField] private float doubleJumpDuration = 6f;

    [Header("Gravedad")]
    [SerializeField] private float heavyGravityScale = 15f;
    [SerializeField] private float heavyGravityDuration = 3f;

    [Header("Control Espejo")]
    [SerializeField] private float mirrorDuration = 4f;

    [Header("Invertir Controles")]
    [SerializeField] private float invertDuration = 4f;

    [Header("Jetpack")]
    [SerializeField] private float jetpackDuration = 5f;
    [SerializeField] private float jetpackForce = 8f;

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
        Collider2D platform = GetPlatformBelow(target);

        if (platform != null)
        {
            // desactivar renderer tambien para que se vea que desaparecio
            SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
            platform.enabled = false;
            if (sr) sr.enabled = false;

            yield return new WaitForSeconds(platformDisableDuration);

            platform.enabled = true;
            if (sr) sr.enabled = true;
        }
        else
        {
            Debug.Log("No habia plataforma debajo");
        }
    }

    private Collider2D GetPlatformBelow(PlatformPlayerController target)
    {
        Collider2D col = target.GetComponent<Collider2D>();
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);

        // aumentar distancia y agregar debug
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 3f, LayerMask.GetMask("Ground"));

        Debug.Log("RemovePlatform raycast hit: " + (hit.collider != null ? hit.collider.gameObject.name : "nada"));

        return hit.collider;
    }

    //  GANCHO 
    public IEnumerator ActivateHook(PlatformPlayerController user, PlatformPlayerController target)
    {
        // activar linea
        if (hookLine != null)
        {
            hookLine.enabled = true;
            hookLine.positionCount = 2;
        }

        float elapsed = 0f;
        float pullTime = 0.6f;

        while (elapsed < pullTime)
        {
            float dist = Vector2.Distance(user.transform.position, target.transform.position);
            if (dist < 1.5f) break;

            // actualizar visual de la linea
            if (hookLine != null)
            {
                hookLine.SetPosition(0, user.transform.position);
                hookLine.SetPosition(1, target.transform.position);
            }

            Vector2 dir = ((Vector2)user.transform.position - (Vector2)target.transform.position).normalized;
            target.ForceVelocity(dir * hookSpeed);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // desactivar linea
        if (hookLine != null)
            hookLine.enabled = false;
    }

    private IEnumerator PullTarget(PlatformPlayerController user, PlatformPlayerController target)
    {
        float elapsed = 0f;
        float pullTime = 0.4f;

        while (elapsed < pullTime)
        {
            // si el target ya llego cerca del user parar
            if (Vector2.Distance(user.transform.position, target.transform.position) < 1.5f)
                break;

            Vector2 dir = ((Vector2)user.transform.position - (Vector2)target.transform.position).normalized;
            target.ForceVelocity(dir * hookSpeed);
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

    public IEnumerator ActivateInvertControls(PlatformPlayerController target)
    {
        target.SetInvertControls(true);
        yield return new WaitForSeconds(invertDuration);
        target.SetInvertControls(false);
    }

    // JETPACK
    public IEnumerator ActivateJetpack(PlatformPlayerController user)
    {
        user.SetJetpack(true, jetpackForce);
        yield return new WaitForSeconds(jetpackDuration);
        user.SetJetpack(false, 0f);
    }
}
