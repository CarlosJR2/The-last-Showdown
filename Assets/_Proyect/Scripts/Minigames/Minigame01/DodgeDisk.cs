using UnityEngine;
using TMPro;
using System.Collections;

public class DodgeDisk : MonoBehaviour
{
    
    [Header("Minigame Settings")]
    [SerializeField] private float gameDuration = 90f;
    [SerializeField] private float pointInterval = 5f;
    [SerializeField] private float invulnerableTime = 3f;

   
    [Header("References")]
    [SerializeField] private DiskMovement diskMovement;
    [SerializeField] private Collider2D diskCollider;
    [SerializeField] private Transform diskSpawnPoint;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private Collider2D player1Collider;
    [SerializeField] private Collider2D player2Collider;
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;

   
    [Header("Debug")]
    [SerializeField] private float gameTimer;
    [SerializeField] private float pointTimer;
    [SerializeField] private bool player1Invulnerable;
    [SerializeField] private bool player2Invulnerable;
    [SerializeField] private float invulnTimer1;
    [SerializeField] private float invulnTimer2;
    [SerializeField] private bool gameRunning;

   
    private void Start()
    {
        StartMinigame();
    }

    private void StartMinigame()
    {
        gameTimer = gameDuration;
        pointTimer = pointInterval;
       
        gameRunning = true;

        player1.transform.position = player1SpawnPoint.position;
        player2.transform.position = player2SpawnPoint.position;

        diskMovement.transform.position = diskSpawnPoint.position;
        diskMovement.Launch();
        UpdateUI();
    }

    private void Update()
    {
        if (!gameRunning) return;
        UpdateTimers();
        UpdateUI();
    }

    
    private void UpdateTimers()
    {
        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            EndMinigame();
            return;
        }

        pointTimer -= Time.deltaTime;
        if (pointTimer <= 0f)
        {
            GivePointsToBothPlayers();
            pointTimer = pointInterval;
        }

      

        if (player1Invulnerable)
        {
            invulnTimer1 -= Time.deltaTime;
            if (invulnTimer1 <= 0f)
            {
                player1Invulnerable = false;
                Physics2D.IgnoreCollision(diskCollider, player1Collider, false);
            }
        }

        if (player2Invulnerable)
        {
            invulnTimer2 -= Time.deltaTime;
            if (invulnTimer2 <= 0f)
            {
                player2Invulnerable = false;
                Physics2D.IgnoreCollision(diskCollider, player2Collider, false);
            }
        }
    }

   
        


    public void TryHitPlayer(int player)
    {
        if (player == 1 && !player1Invulnerable)
        {
            GameManager.Instance.AddResult(1, false);
            RespawnPlayer(1);
        }
        else if (player == 2 && !player2Invulnerable)
        {
            GameManager.Instance.AddResult(2, false);
            RespawnPlayer(2);
        }
    }


    private void RespawnPlayer(int player)
    {
        if (player == 1)
        {
            player1.transform.position = player1SpawnPoint.position;
            player1Invulnerable = true;
            invulnTimer1 = invulnerableTime;
            Physics2D.IgnoreCollision(diskCollider, player1Collider, true);
            StartCoroutine(FlashPlayer(player1));
        }
        else
        {
            player2.transform.position = player2SpawnPoint.position;
            player2Invulnerable = true;
            invulnTimer2 = invulnerableTime;
            Physics2D.IgnoreCollision(diskCollider, player2Collider, true);
            StartCoroutine(FlashPlayer(player2));
        }
    }

    
    private void GivePointsToBothPlayers()
    {
        GameManager.Instance.AddResult(1, true);
        GameManager.Instance.AddResult(2, true);
    }

    
    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (player1ScoreText != null)
            player1ScoreText.text = "P1: " + GameManager.Instance.player1Score;

        if (player2ScoreText != null)
            player2ScoreText.text = "P2: " + GameManager.Instance.player2Score;
    }

    
    private void EndMinigame()
    {
        gameRunning = false;
        diskMovement.Stop();
        GameManager.Instance.EndRound(1);
        SceneLoader.Instance.LoadResults();
    }

 
    private IEnumerator FlashPlayer(GameObject player)
    {
        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < invulnerableTime)
        {
            visible = !visible;
            sr.enabled = visible;
            elapsed += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        sr.enabled = true;
    }
}