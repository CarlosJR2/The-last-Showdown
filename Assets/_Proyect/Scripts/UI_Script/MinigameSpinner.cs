using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MinigameSpinner : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private float minSpinPower = 40f;
    [SerializeField] private float maxSpinPower = 80f;
    [SerializeField] private float stopPower = 2f;  // Cuánto rozamiento tiene (Drag)

    private Rigidbody2D rb;
    private Transform tr;
    private bool hasSpun = false; // Para saber si ya arranco
    private float t;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<Transform>();
        rb.angularDamping = 0.2f; //el rozamiento con el que frena
        SpinIt();
    }

    public void SpinIt()
    {
        //fuerza aleatoria en un rango
        float randomPower = Random.Range(minSpinPower, maxSpinPower);
        Debug.Log("la spinPower es " +  randomPower);
        rb.AddTorque(randomPower, ForceMode2D.Impulse);
        hasSpun = true;
    }

    private void Update()
    {
        //Mientras esté girando, aplicamos el frenado manual
        if (rb.angularVelocity > 0)
        {
            // Restamos velocidad poco a poco
            rb.angularVelocity -= stopPower * Time.deltaTime;

            // Evitamos que pase a valores negativos
            if (rb.angularVelocity < 0) rb.angularVelocity = 0;
        }

        // 2. Detectar cuando se detiene totalmente
        if (hasSpun && rb.angularVelocity <= 0)
        {
            t += Time.deltaTime;
            if (t >= 0.5f)
            {
                SelectedMinigame();
                hasSpun = false; // Reset para la próxima vez
                t = 0;
                transform.rotation = Quaternion.identity;
            }
        }
    }

    private void SelectedMinigame()
    {
        float angle = transform.rotation.eulerAngles.z;

        // 8 minijuegos, cada uno ocupa 45 grados (360 / 8) -> quiero keke
        int totalSlices = 8;
        float degreesPerSlice = 360f / totalSlices;

        int winningIndex = Mathf.FloorToInt(angle / degreesPerSlice); // el minijuego es igual a la division entre tods los angles y en donde puede caer

        Debug.Log("Cayó en la porción número: " + (winningIndex + 1)); // + 1 porque si toca el minijuego 1 para el Index seria la pos 0

        switch (winningIndex) //switch para un futuro 
        {
            case 1:
                SceneLoader.Instance.LoadMinigame(01);
                break;
            case 2:
                SceneLoader.Instance.LoadMinigame(01);
                break;
            case 3:
                SceneLoader.Instance.LoadMinigame(01);
                break;
            case 4:
                SceneLoader.Instance.LoadMinigame(01);
                break;
            case 5:
                SceneLoader.Instance.LoadMinigame(02);
                break;
            case 6:
                SceneLoader.Instance.LoadMinigame(02);
                break;
            case 7:
                SceneLoader.Instance.LoadMinigame(02);
                break;
            case 8:
                SceneLoader.Instance.LoadMinigame(02);
                break;
        }
    }
}