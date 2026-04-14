using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Results : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI player1ScoreText;
    [SerializeField] private TextMeshProUGUI player2ScoreText;
    [SerializeField] private Button goToRuletaButton;   // el botón que ya tenías
    [SerializeField] private Button goToFinalButton;    // botón nuevo para la pantalla final

    private void Start()
    {
        player1ScoreText.text = "Jugador 1: " + GameManager.Instance.player1Score + " pts";
        player2ScoreText.text = "Jugador 2: " + GameManager.Instance.player2Score + " pts";

        bool hayMinijuegosRestantes = GameManager.Instance.GetAvailableMinigames().Count > 0;

        goToRuletaButton.gameObject.SetActive(hayMinijuegosRestantes);
        goToFinalButton.gameObject.SetActive(!hayMinijuegosRestantes);
    }
    
    public void OnSalirButton()
    {
        Application.Quit();
    }

    public void LoadRuleta()
    {
        SceneManager.LoadScene("Select_Minigame");
    }
    public void LoadFinal()
    {
        SceneLoader.Instance.LoadFinalScreen();
    }
}