using UnityEngine;

public class ZoneCameraController : MonoBehaviour
{
    [Header("Jugadores")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Zoom")]
    [SerializeField] private float minSize = 4f;      // zoom maximo cuando estan cerca
    [SerializeField] private float maxSize = 8f;      // zoom maximo cuando estan lejos (fijo por zona)
    [SerializeField] private float zoomSpeed = 3f;    // que tan rapido hace zoom

    [Header("Seguimiento")]
    [SerializeField] private float followSpeed = 5f;  // que tan rapido sigue a los jugadores
    [SerializeField] private float padding = 1.5f;    // espacio extra alrededor de los jugadores

    [Header("Debug")]
    [SerializeField] private float currentDistance;

    private Camera cam;
    private Vector3 zoneCenter;
    private bool zoneSet = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // el manager llama esto cuando cambia de zona
    public void SetZoneCenter(Vector3 center)
    {
        zoneCenter = center;
        zoneSet = true;

        // teleport instantaneo al centro de la nueva zona
        // asi no se bugea al cambiar
        Vector3 pos = zoneCenter;
        pos.z = transform.position.z;
        transform.position = pos;

        // resetear zoom al maximo para que arranque bien
        cam.orthographicSize = maxSize;
    }

    private void LateUpdate()
    {
        if (!zoneSet || player1 == null || player2 == null) return;

        // punto medio entre los dos jugadores
        Vector3 midPoint = (player1.position + player2.position) / 2f;

        // la camara sigue el punto medio suavemente
        Vector3 targetPos = new Vector3(midPoint.x, midPoint.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // calcular distancia entre jugadores para el zoom
        currentDistance = Vector3.Distance(player1.position, player2.position);

        // mientras mas separados estan, mas zoom out
        // padding agrega espacio extra para que no queden pegados al borde
        float targetSize = Mathf.Max(currentDistance / 2f + padding, minSize);

        // clampear al maximo permitido
        targetSize = Mathf.Clamp(targetSize, minSize, maxSize);

        // aplicar zoom suavemente
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);
    }
}