using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float floatAmplitude = 0.15f;
    public float floatSpeed = 1f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2f;

    [Header("Raycast Settings")]
    public LayerMask groundLayer;

    private Material itemMaterial;
    private Vector3 basePosition;

    void Start()
    {
        // Настройка свечения
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            itemMaterial = renderer.material;
            itemMaterial.EnableKeyword("_EMISSION");
            itemMaterial.SetColor("_EmissionColor", glowColor * Mathf.LinearToGammaSpace(glowIntensity));
        }

        // Получаем центр коллайдера объекта
        Collider col = GetComponent<Collider>();
        Vector3 origin = transform.position;
        if (col != null)
        {
            origin = col.bounds.center;
        }

        // Проверяем расстояние до земли от центра объекта
        RaycastHit hit;
        if (Physics.Raycast(origin + Vector3.up * 0.5f, Vector3.down, out hit, 10f, groundLayer))
        {
            float centerY = hit.point.y + 0.6f; // Центральная точка плюс расстояние для парения
            basePosition = new Vector3(transform.position.x, centerY, transform.position.z);
        }
        else
        {
            basePosition = transform.position;
        }

        transform.position = basePosition;
    }

    void Update()
    {
        FloatItem();
        RotateItem();
    }

    void FloatItem()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(basePosition.x, basePosition.y + offset, basePosition.z);
    }

    void RotateItem()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}