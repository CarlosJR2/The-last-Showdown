using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    // Singleton
    public static GameManager Instance; //se accede desde cualquier script a GameManager y todo lo que tenga adentro mientras sea publico

    // Datos de jugadores

    public int player1Score;
    public int player2Score;

    // Puntos del minijuego actual

    public int player1RoundPoints;
    public int player2RoundPoints;

    // Modificador de la ruleta
    public float pointsModifier = 1f;

    // Estado del juego

    public int currentRound = 1;
    public const int TOTAL_ROUNDS = 3;
    public const int BASE_POINTS = 10;

    private List<int> availableMinigames = new List<int>();
    private List<int> playedMinigames = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) //crea el GameManager si no existe, y si existe lo deja como esta
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeMinigames();
    }

    private void InitializeMinigames() //carga todos los minijuegos disponible a la lista availableMinigames
    {
        availableMinigames.Clear();
        playedMinigames.Clear();

        for (int i = 1; i <= TOTAL_ROUNDS; i++) //recorre la lista y la llena
        {
            availableMinigames.Add(i);
        }
    }

    // Puntos

    public void AddResult(int player, bool won) //es posible que este metodo se elimine
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
        return currentRound > TOTAL_ROUNDS; //va a ser true cuando las rondas actuales superen a las totales (11 > 10)
    }

    public void EndRound(int minigameId) // se llama para agregar un minijuego a la lista de jugados y removerlo de la lista de disponibles
    {
        playedMinigames.Add(minigameId);
        availableMinigames.Remove(minigameId);
        currentRound++;
    }

    public void ResetGame() //se llama para para resetear
    {
        player1Score = 0;
        player2Score = 0;
        currentRound = 1;
        InitializeMinigames();
    }

    // Sumar puntos dentro del minijuego
    public void AddPoints(int player, int points) //se usa para chequear quien gano los puntos, por ejemplo,
                                                  //si el P1 agarra una moneda se llamaria a este metodo asi: GameManager.Instance.AddPoints(1,1),
                                                  //y el valor de puntos se guarda en la cantidad de puntos en la ronda, todavia no en la de puntos globales
    {
        if (player == 1) player1RoundPoints += points;
        else player2RoundPoints += points;
    }

    // Al terminar el minijuego, aplicar modificador y sumar al total
    public void FinishMinigame()
    {
        int winner = player1RoundPoints > player2RoundPoints ? 1 : 2; //Si hay empate va a ganar el P2

        // Bonus al ganador con modificador
        int bonus = (int)(BASE_POINTS * pointsModifier); //se determina cuanto valen los puntos dependiendo si la ruleta dijo que valen doble o no
        if (winner == 1) player1RoundPoints += bonus; //se agregan los puntos a fin del minijuego dependiendo que dijo la ruleta
        else player2RoundPoints += bonus;

        // Sumar al marcador general
        player1Score += player1RoundPoints;
        player2Score += player2RoundPoints;

        // Resetear para el próximo minijuego
        player1RoundPoints = 0;
        player2RoundPoints = 0;
        pointsModifier = 1f;
    }

}
