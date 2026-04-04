using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KingOfHill : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float gameDuration = 120f;
    [SerializeField] private float zoneChangeDuration = 30f;
    [SerializeField] private float pointsPerSecond = 2f;
    [SerializeField] private float flashDuration = 1f;

    [Header("Zonas")]
    [SerializeField] private Transform[] zones;
    [SerializeField] private HardPoint[] hardPoints;

    [Header("Spawns")]
    [SerializeField] private Transform[] player1Spawns;
    [SerializeField] private Transform[] player2Spawns;

    [Header("Jugadores")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Camara")]
    [SerializeField] private ZoneCameraController zoneCamera;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private Image flashImage;

    [Header("Debug")]
    [SerializeField] private int currentZoneIndex = 0;
    [SerializeField] private float gameTimer;
    [SerializeField] private float zoneTimer;
    [SerializeField] private bool gameRunning;

    private float pointAccumulator1 = 0f;
    private float pointAccumulator2 = 0f;
    private PlatformPlayerController p1Controller;
    private PlatformPlayerController p2Controller;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
            
        p1Controller = player1.GetComponent<PlatformPlayerController>();
        p2Controller = player2.GetComponent<PlatformPlayerController>();

        p1Controller.SetOtherPlayer(p2Controller);
        p2Controller.SetOtherPlayer(p1Controller);

        // elegir zona ANTES de llamar StartMinigame
        // para que los spawns esten seteados cuando los jugadores se inicializan
        currentZoneIndex = Random.Range(0, zones.Length);

        // setear spawns inmediatamente
        p1Controller.SetSpawnPoint(player1Spawns[currentZoneIndex].position);
        p2Controller.SetSpawnPoint(player2Spawns[currentZoneIndex].position);

        // teleport inmediato antes de que corra cualquier fisica
        player1.transform.position = player1Spawns[currentZoneIndex].position;
        player2.transform.position = player2Spawns[currentZoneIndex].position;

        StartMinigame();
    }

    private void StartMinigame()
    {
        gameTimer = gameDuration;
        zoneTimer = zoneChangeDuration;
        gameRunning = true;

        // zona ya elegida en Start, solo activar
        ActivateZone(currentZoneIndex, teleport: false);

        UpdateUI();
    }

    private void Update()
    {
        if (!gameRunning) return;

        gameTimer -= Time.deltaTime;
        zoneTimer -= Time.deltaTime;

        HandleHardPointPoints();

        if (zoneTimer <= 0f)
        {
            StartCoroutine(ChangeZone());
            zoneTimer = zoneChangeDuration;
        }

        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            EndMinigame();
        }

        UpdateUI();
    }

    private void HandleHardPointPoints()
    {
        HardPoint activePoint = hardPoints[currentZoneIndex];

        bool p1Inside = activePoint.IsPlayer1Inside;
        bool p2Inside = activePoint.IsPlayer2Inside;

        Debug.Log("P1 inside: " + p1Inside + " P2 inside: " + p2Inside);

        if (p1Inside && !p2Inside)
        {
            pointAccumulator1 += pointsPerSecond * Time.deltaTime;
            if (pointAccumulator1 >= 1f)
            {
                int pts = Mathf.FloorToInt(pointAccumulator1);
                GameManager.Instance.AddPoints(1, pts);
                pointAccumulator1 -= pts;
            }
        }
        else if (p2Inside && !p1Inside)
        {
            pointAccumulator2 += pointsPerSecond * Time.deltaTime;
            if (pointAccumulator2 >= 1f)
            {
                int pts = Mathf.FloorToInt(pointAccumulator2);
                GameManager.Instance.AddPoints(2, pts);
                pointAccumulator2 -= pts;
            }
        }
    }

    private IEnumerator ChangeZone()
    {
        gameRunning = false;

        yield return StartCoroutine(Flash());

        int newZone;
        do { newZone = Random.Range(0, zones.Length); }
        while (newZone == currentZoneIndex && zones.Length > 1);

        currentZoneIndex = newZone;
        ActivateZone(currentZoneIndex, teleport: true);

        gameRunning = true;
    }

    private void ActivateZone(int index, bool teleport)
    {
        if (teleport)
        {
            player1.transform.position = player1Spawns[index].position;
            player2.transform.position = player2Spawns[index].position;

            // actualizar spawn point en los controllers
            p1Controller.SetSpawnPoint(player1Spawns[index].position);
            p2Controller.SetSpawnPoint(player2Spawns[index].position);
        }

        zoneCamera.SetZoneCenter(zones[index].position, index);
    }

    private IEnumerator Flash()
    {
        flashImage.gameObject.SetActive(true);
        float half = flashDuration / 2f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / half));
            yield return null;
        }

        flashImage.color = Color.black;
        yield return new WaitForSeconds(0.1f);

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            flashImage.color = new Color(0, 0, 0, Mathf.Clamp01(1f - t / half));
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
    }

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(gameTimer / 60f);
        int seconds = Mathf.FloorToInt(gameTimer % 60f);
        if (timerText) timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (player1ScoreText)
            player1ScoreText.text = "P1: " + GameManager.Instance.player1RoundPoints;
        if (player2ScoreText)
            player2ScoreText.text = "P2: " + GameManager.Instance.player2RoundPoints;
    }

    private void EndMinigame()
    {
        gameRunning = false;
        GameManager.Instance.FinishMinigame();
        SceneLoader.Instance.LoadResults();
    }
}