using UnityEngine;
using System.Collections;

public class PowerUpManager : MonoBehaviour
{
    public enum PowerUpType { Swap, Freeze, Wall, Magnet }

    [Header("References")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private DiskMovement diskMovement;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private PowerUpMovement powerUpPickup;

    [Header("Settings")]
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private float wallDuration = 5f;
    [SerializeField] private float magnetDuration = 3f;

    [Header("Debug")]
    [SerializeField] private PowerUpType player1PowerUp;
    [SerializeField] private PowerUpType player2PowerUp;
    [SerializeField] private bool player1HasPowerUp;
    [SerializeField] private bool player2HasPowerUp;
    [SerializeField] private bool player1Frozen;
    [SerializeField] private bool player2Frozen;
    [SerializeField] private bool wallActive;
    [SerializeField] private bool magnetActive;

    private GameObject activeWall;
    private PlayerController player1Controller;
    private PlayerController player2Controller;

    private void Awake()
    {
        player1Controller = player1.GetComponent<PlayerController>();
        player2Controller = player2.GetComponent<PlayerController>();
    }

    private void Update()
    {
        // player 1 activa power up
        if (player1HasPowerUp && player1Controller.GetInteractPressed())
            ActivatePowerUp(1);

        // player 2 activa power up
        if (player2HasPowerUp && player2Controller.GetInteractPressed())
            ActivatePowerUp(2);
    }

    // PICKUP 
    public void OnPlayerPickup(int player)
    {
        if (player == 1 && !player1HasPowerUp)
        {
            player1PowerUp = GetRandomPowerUp();
            player1HasPowerUp = true;
            powerUpPickup.gameObject.SetActive(false);
            Invoke(nameof(RespawnPickup), 2f);
        }
        else if (player == 2 && !player2HasPowerUp)
        {
            player2PowerUp = GetRandomPowerUp();
            player2HasPowerUp = true;
            powerUpPickup.gameObject.SetActive(false);
            Invoke(nameof(RespawnPickup), 2f);
        }
    }

    private void RespawnPickup()
    {
        powerUpPickup.Reposition();
    }

    private PowerUpType GetRandomPowerUp()
    {
        int r = Random.Range(0, 4);
        return (PowerUpType)r;
    }

    // ACTIVATE
    private void ActivatePowerUp(int player)
    {
        PowerUpType type = player == 1 ? player1PowerUp : player2PowerUp;

        // consumir el power up
        if (player == 1) player1HasPowerUp = false;
        else player2HasPowerUp = false;

        int opponent = player == 1 ? 2 : 1;

        switch (type)
        {
            case PowerUpType.Swap:
                ActivateSwap(player, opponent);
                break;
            case PowerUpType.Freeze:
                if (!player1Frozen && !player2Frozen)
                    StartCoroutine(ActivateFreeze(opponent));
                break;
            case PowerUpType.Wall:
                if (!wallActive)
                    StartCoroutine(ActivateWall());
                break;
            case PowerUpType.Magnet:
                if (!magnetActive)
                    StartCoroutine(ActivateMagnet(opponent));
                break;
        }
    }

    // SWAP
    private void ActivateSwap(int player, int opponent)
    {
        GameObject p1 = player == 1 ? player1 : player2;
        GameObject p2 = player == 1 ? player2 : player1;

        Vector3 temp = p1.transform.position;
        p1.transform.position = p2.transform.position;
        p2.transform.position = temp;
    }

    // FREEZE 
    private IEnumerator ActivateFreeze(int opponent)
    {
        PlayerController opponentController = opponent == 1 ? player1Controller : player2Controller;

        if (opponent == 1) player1Frozen = true;
        else player2Frozen = true;

        opponentController.SetFrozen(true);

        yield return new WaitForSeconds(freezeDuration);

        opponentController.SetFrozen(false);

        if (opponent == 1) player1Frozen = false;
        else player2Frozen = false;
    }

    //  WALL 
    private IEnumerator ActivateWall()
    {
        wallActive = true;
        activeWall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForSeconds(wallDuration);

        Destroy(activeWall);
        wallActive = false;
    }

    // MAGNET 
    private IEnumerator ActivateMagnet(int opponent)
    {
        magnetActive = true;
        GameObject target = opponent == 1 ? player1 : player2;

        float elapsed = 0f;
        while (elapsed < magnetDuration)
        {
            Vector2 dirToTarget = (target.transform.position - diskMovement.transform.position).normalized;
            diskMovement.SetDirection(dirToTarget);
            elapsed += Time.deltaTime;
            yield return null;
        }

        magnetActive = false;
    }
}