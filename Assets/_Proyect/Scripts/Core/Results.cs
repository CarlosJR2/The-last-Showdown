using UnityEngine;
using TMPro;


public class Results : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;

    private void Start()
    {
        player1ScoreText.text = "Jugador 1: " + GameManager.Instance.player1Score + " pts";
        player2ScoreText.text = "Jugador 2: " + GameManager.Instance.player2Score + " pts";
    }
    
    public void OnSalirButton()
    {
        Application.Quit();
    }
}