using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string firstScene = "Minigame_02";

    private void Start()
    {
        // usando Start en vez de Awake para darle tiempo al GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager not found.");
            return;
        }

        SceneManager.LoadScene(firstScene);
    }
}