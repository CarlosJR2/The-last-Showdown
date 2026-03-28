using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    // Singleton
    public static GameManager Instance;

    // Datos de jugadores

    public int player1Score;
    public int player2Score;

    // Estado del juego

    public int currentRound = 1;
    public const int TOTAL_ROUNDS = 10;
    public const int BASE_POINTS = 10;

    private List<int> availableMinigames = new List<int>();
    private List<int> playedMinigames = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeMinigames();
    }

    private void InitializeMinigames()
    {
        availableMinigames.Clear();
        playedMinigames.Clear();

        for (int i = 1; i <= TOTAL_ROUNDS; i++)
        {
            availableMinigames.Add(i);
        }
    }

    // Puntos

    public void AddResult(int player, bool won)
    {
        int multiplier = (int)Mathf.Pow(2, currentRound - 1);
        int points = BASE_POINTS * multiplier;

        if (player == 1)
            player1Score += won ? points : -points;
        else
            player2Score += won ? points : -points;
    }

    public List<int> GetAvailableMinigames()
    {
        return new List<int>(availableMinigames);
    }

    public bool IsGameOver()
    {
        return currentRound > TOTAL_ROUNDS;
    }

    public void EndRound(int minigameId)
    {
        playedMinigames.Add(minigameId);
        availableMinigames.Remove(minigameId);
        currentRound++;
    }

    public void ResetGame()
    {
        player1Score = 0;
        player2Score = 0;
        currentRound = 1;
        InitializeMinigames();
    }

}
