using UnityEngine;

public class ModifiersSpinner : MonoBehaviour
{
    [Header("Configuración de Minijuegos")]
    [SerializeField] private int totalModifiers = 2;

    [Header("Configuración Visual")]
    [SerializeField] private float minSpinPower = 40f;
    [SerializeField] private float maxSpinPower = 80f;
    [SerializeField] private float stopPower = 2f;

    private Rigidbody2D rb;
    private bool hasSpun = false;
    private float stoppedTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularDamping = 0.2f;
        SpinIt();
    }

    public void SpinIt()
    {
        float randomPower = Random.Range(minSpinPower, maxSpinPower);
        rb.AddTorque(randomPower, ForceMode2D.Impulse);
        hasSpun = true;
        stoppedTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.angularVelocity > 0)
        {
            rb.angularVelocity -= stopPower * Time.deltaTime;
            if (rb.angularVelocity < 0) rb.angularVelocity = 0;
        }

        if (hasSpun && rb.angularVelocity <= 0) //cuando se queda quieto
        {
            stoppedTimer += Time.deltaTime; //mini pausa
            if (stoppedTimer >= 0.5f)
            {
                hasSpun = false;
                stoppedTimer = 0f;
                SelectedModifier();
            }
        }
    }

    private void SelectedModifier()
    {
        float rawAngle = transform.rotation.eulerAngles.z;

        
        float angleOffset = 90f;// esto se hace para corregir donde esta el puntero de la ruleta y detecte bien dodne cae
        float normalizedAngle = (rawAngle + angleOffset) % 360f;

        float degreesPerSlice = 360f / totalModifiers;
        int modifierId = Mathf.FloorToInt(normalizedAngle / degreesPerSlice) + 1;

        Debug.Log($"Raw: {rawAngle:F1}° | Normalizado: {normalizedAngle:F1}° | Modificador: {modifierId}");

        switch (modifierId)
        {
            case 1:
                Debug.Log("Cayó en Multiplicador");
                //GameManager.Instance.pointsModifier = 2f; fijate por aca jr
                break;
            case 2:
                Debug.Log("Cayó en Drenaje");
                //GameManager.Instance.pointsModifier = 0.5f; fijate por aca jr
                break;
        }
    }
}
