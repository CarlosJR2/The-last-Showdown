using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = "Minigame_01";

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found. Make sure it exists in the Bootstrap scene.");
            return;
        }

        SceneManager.LoadScene(firstScene);
    }
}