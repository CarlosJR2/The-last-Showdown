using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
  
    public static SceneLoader Instance; //es un singletone, todos pueden acceder

    private void Awake()
    {
        if (Instance != null && Instance != this) //se crea si no esta o se deja como esta
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // LOAD SCENES 
    public void LoadMinigame(int minigameId)
    {
        string sceneName = "Minigame" + minigameId.ToString("D2");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadRuleta()
    {
        SceneManager.LoadScene("Ruleta");
    }

    public void LoadResults()
    {
        SceneManager.LoadScene("Results");
    }

    public void LoadFinalScreen()
    {
        SceneManager.LoadScene("FinalScreen");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}