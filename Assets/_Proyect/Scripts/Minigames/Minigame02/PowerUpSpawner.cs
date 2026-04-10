using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float respawnDelay = 5f;

    [Header("Spawn Points por zona - mismo orden que las zonas")]
    [SerializeField] private Transform[][] zoneSpawnPoints;

    [System.Serializable]
    public class ZoneSpawnPoints
    {
        public Transform[] points;
    }

    [SerializeField] private ZoneSpawnPoints[] zones;

    [Header("Prefab del PowerUp")]
    [SerializeField] private GameObject powerUpPrefab;

    private List<Transform> availablePoints = new List<Transform>();
    private int currentZoneIndex = 0;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    // el manager llama esto cuando cambia de zona
    public void SetActiveZone(int zoneIndex)
    {
        currentZoneIndex = zoneIndex;

        // resetear puntos disponibles a los de la zona activa
        availablePoints.Clear();
        foreach (Transform point in zones[zoneIndex].points)
            availablePoints.Add(point);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (availablePoints.Count > 0)
                SpawnPowerUp();
        }
    }

    private void SpawnPowerUp()
    {
        int index = Random.Range(0, availablePoints.Count);
        Transform point = availablePoints[index];
        availablePoints.RemoveAt(index);

        GameObject obj = Instantiate(powerUpPrefab, point.position, Quaternion.identity);
        PowerUpPickup pickup = obj.GetComponent<PowerUpPickup>();
        pickup.Initialize(this, point);
    }

    public void OnPickupCollected(Transform point)
    {
        StartCoroutine(RespawnPoint(point));
    }

    private IEnumerator RespawnPoint(Transform point)
    {
        yield return new WaitForSeconds(respawnDelay);

        // solo agregar de vuelta si sigue siendo la zona activa
        if (zones[currentZoneIndex].points.Length > 0)
            availablePoints.Add(point);
    }
}