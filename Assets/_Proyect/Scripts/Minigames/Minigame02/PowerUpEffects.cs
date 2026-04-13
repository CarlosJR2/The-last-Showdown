using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class PowerUpEffects : MonoBehaviour
{
    [Header("Jaula")]
    [SerializeField] private GameObject cagePrefab;
    [SerializeField] private float cageDuration = 5f;

    [Header("Escudo")]
    [SerializeField] private float shieldDuration = 4f;
    [SerializeField] private float shieldKnockbackMultiplier = 3f;

    [Header("Quitar Plataforma")]
    [SerializeField] private float platformDisableDuration = 3f;
    // La layer del tilemap (la misma que usa groundLayer en el controller)
    [SerializeField] private LayerMask platformLayer;

    [Header("Gancho")]
    [SerializeField] private float hookSpeed = 15f;
    [SerializeField] private LineRenderer hookLine;
    // Layer de obstaculos que bloquean el gancho (paredes, plataformas)
    // Asignar la misma layer que platformLayer + cualquier otra pared
    [SerializeField] private LayerMask hookObstacleLayer;

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
    // FIX: usa Physics2D.IgnoreCollision entre el jugador y el TilemapCollider
    // para que el jugador caiga a traves de la plataforma sin romper todo el tilemap
    public IEnumerator ActivateRemovePlatform(PlatformPlayerController target)
    {
        Collider2D playerCol = target.GetCollider();
        Collider2D platformCol = GetPlatformBelow(target);

        if (platformCol != null)
        {
            Rigidbody2D targetRb = target.GetRigidbody();

            // guardar Y de los pies del jugador antes de activar
            float feetY = playerCol.bounds.min.y;

            // 1) ignorar colision
            Physics2D.IgnoreCollision(playerCol, platformCol, true);

            // 2) esperar que Unity registre el cambio de colision
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // 3) mover SOLO un poquito hacia abajo para sacar al jugador del borde
            // del CompositeCollider sin que parezca un teleport
            // el CompositeCollider tiene un borde fino - con 0.15f alcanza
            target.transform.position += Vector3.down * 0.15f;

            // 4) gravedad normal se encarga del resto, solo asegurar que no este quieto
            if (targetRb != null && Mathf.Abs(targetRb.linearVelocity.y) < 0.1f)
                targetRb.linearVelocity = new Vector2(targetRb.linearVelocity.x, -1f);

            // 5) esperar duracion del power up
            yield return new WaitForSeconds(platformDisableDuration);

            // 6) restaurar: chequear si el jugador esta dentro de la plataforma
            // con un OverlapPoint en el centro del jugador
            float waitMax = 3f;
            float waited = 0f;
            while (waited < waitMax)
            {
                Vector2 center = playerCol.bounds.center;
                // si no hay overlap en el centro del jugador con la layer de plataforma, es seguro
                Collider2D overlap = Physics2D.OverlapPoint(center, platformLayer);
                if (overlap == null)
                    break;
                waited += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForFixedUpdate();
            Physics2D.IgnoreCollision(playerCol, platformCol, false);
        }
        else
        {
            Debug.Log("No habia plataforma debajo del target");
        }
    }

    private Collider2D GetPlatformBelow(PlatformPlayerController target)
    {
        Collider2D col = target.GetCollider();
        // origin desde el centro-inferior del jugador
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);

        // buscar hacia abajo con la layer del tilemap
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 3f, platformLayer);

        Debug.Log("RemovePlatform raycast hit: " + (hit.collider != null ? hit.collider.gameObject.name : "nada"));

        return hit.collider;
    }

    // GANCHO
    // FIX: chequear linea de vision entre user y target antes de jalar
    // si hay obstaculo en el medio, el gancho se cancela
    public IEnumerator ActivateHook(PlatformPlayerController user, PlatformPlayerController target)
    {
        // chequear si hay linea de vision libre
        Vector2 userPos = user.transform.position;
        Vector2 targetPos = target.transform.position;
        Vector2 dir = targetPos - userPos;
        float dist = dir.magnitude;

        // raycast desde user hacia target buscando obstaculos
        // ignoramos los colliders de los propios jugadores usando una query especial
        RaycastHit2D[] hits = Physics2D.RaycastAll(userPos, dir.normalized, dist, hookObstacleLayer);

        bool blocked = false;
        foreach (var hit in hits)
        {
            // ignorar si el collider es del user o del target
            if (hit.collider == user.GetCollider()) continue;
            if (hit.collider == target.GetCollider()) continue;
            blocked = true;
            break;
        }

        if (blocked)
        {
            Debug.Log("Gancho bloqueado por obstaculo");
            // opcional: mostrar la linea brevemente para feedback visual
            if (hookLine != null)
            {
                hookLine.enabled = true;
                hookLine.positionCount = 2;
                hookLine.SetPosition(0, userPos);
                // mostrar hasta el punto de impacto
                hookLine.SetPosition(1, hits[0].point);
                yield return new WaitForSeconds(0.2f);
                hookLine.enabled = false;
            }
            yield break;
        }

        // si hay linea de vision, ejecutar el gancho
        if (hookLine != null)
        {
            hookLine.enabled = true;
            hookLine.positionCount = 2;
        }

        float elapsed = 0f;
        float pullTime = 0.6f;

        while (elapsed < pullTime)
        {
            float currentDist = Vector2.Distance(user.transform.position, target.transform.position);
            if (currentDist < 1.5f) break;

            if (hookLine != null)
            {
                hookLine.SetPosition(0, user.transform.position);
                hookLine.SetPosition(1, target.transform.position);
            }

            // jalar al target hacia el user
            Vector2 pullDir = ((Vector2)user.transform.position - (Vector2)target.transform.position).normalized;
            target.ForceVelocityRaw(pullDir * hookSpeed);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (hookLine != null)
            hookLine.enabled = false;
    }

    // DOBLE SALTO 
    public IEnumerator ActivateDoubleJump(PlatformPlayerController user)
    {
        user.SetDoubleJump(true);
        yield return new WaitForSeconds(doubleJumpDuration);
        user.SetDoubleJump(false);
    }

    // GRAVEDAD AUMENTADA 
    public IEnumerator ActivateHeavyGravity(PlatformPlayerController target)
    {
        target.SetHeavyGravity(true, heavyGravityScale);
        yield return new WaitForSeconds(heavyGravityDuration);
        target.SetHeavyGravity(false, 0f);
    }

    // CONTROL ESPEJO 
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
