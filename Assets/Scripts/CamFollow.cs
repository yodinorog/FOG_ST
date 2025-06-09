using UnityEngine;
using System.Collections.Generic;

public class Follow : MonoBehaviour
{
    public Transform player;         // Ссылка на персонажа
    public Transform enemy;          // Ссылка на врага (назначается при начале боя)
    public float smoothSpeed = 0.2f; // Скорость плавного перемещения камеры
    public float fixedHeight = 10f;  // Высота камеры
    public Vector3 offset = new Vector3(0, 0, -10f); // Смещение камеры относительно игрока
    public float minDistance = 15f;  // Минимальная дистанция камеры
    public float maxDistance = 30f;  // Максимальная дистанция камеры

    private Vector3 desiredPosition; // Желаемая позиция камеры
    private float targetDistance;    // Целевое расстояние камеры
    private bool inCombat = false;   // Флаг боя

    private List<Renderer> transparentObjects = new List<Renderer>(); // Текущие прозрачные объекты

    void LateUpdate()
    {
        if (inCombat && (enemy == null || Vector3.Distance(player.position, enemy.position) > 40f))
        {
            EndCombat();
        }

        CheckForObstacles();

        if (player == null) return;

        if (inCombat && enemy != null)
        {
            // Средняя точка между игроком и врагом
            Vector3 midpoint = (player.position + enemy.position) / 2;

            // Расстояние между ними
            float distance = Vector3.Distance(player.position, enemy.position);

            // Приближение камеры в бою
            float combatDistance = Mathf.Clamp(distance, minDistance * 0.8f, maxDistance * 0.8f);
            targetDistance = combatDistance;

            // Камера ближе и ниже в бою (offset влияет и на высоту)
            desiredPosition = midpoint + offset.normalized * targetDistance;
        }
        else
        {
            // Камера следует за игроком с базовым отступом
            desiredPosition = player.position + offset;
        }

        // Убираем фиксированную высоту — камера теперь свободна по Y
        // desiredPosition.y = fixedHeight; ← эту строку удаляем

        // Плавное перемещение
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    /// <summary>
    /// Включает режим боя и устанавливает врага.
    /// </summary>
    public void StartCombat(Transform targetEnemy)
    {
        enemy = targetEnemy;
        inCombat = true;
    }

    /// <summary>
    /// Отключает режим боя.
    /// </summary>
    public void EndCombat()
    {
        enemy = null;
        inCombat = false;
    }

    void CheckForObstacles()
    {
        // Восстанавливаем прозрачность объектов, которые ранее были прозрачными
        ResetTransparency();

        // Рассчитываем направление от игрока к камере
        Vector3 directionToCamera = (transform.position - player.position).normalized;

        float totalDistance = Vector3.Distance(player.position, transform.position);
        float offsetFromPlayer = 3f; // на сколько метров отступить от игрока

        // Новая стартовая точка — чуть дальше от игрока
        Vector3 rayStart = player.position + directionToCamera * offsetFromPlayer;
        float shortenedDistance = totalDistance - offsetFromPlayer;


        RaycastHit[] hits = Physics.RaycastAll(rayStart, directionToCamera, shortenedDistance);

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && !transparentObjects.Contains(renderer))
            {
                // Добавляем объект в список прозрачных и меняем прозрачность
                transparentObjects.Add(renderer);
                SetTransparency(renderer, 0.5f);
            }
        }
    }

    /// <summary>
    /// Устанавливает прозрачность объекту.
    /// </summary>
    void SetTransparency(Renderer renderer, float alpha)
    {
        foreach (Material mat in renderer.materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color color = mat.color;
                color.a = alpha;
                mat.color = color;

                // Настраиваем рендер для прозрачности
                mat.SetFloat("_Mode", 3); // Режим Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000; // Прозрачный рендеринг
            }
        }
    }

    /// <summary>
    /// Восстанавливает прозрачность всех прозрачных объектов.
    /// </summary>
    void ResetTransparency()
    {
        foreach (Renderer renderer in transparentObjects)
        {
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = 1f; // Возвращаем полную непрозрачность
                        mat.color = color;

                        // Восстанавливаем рендер для непрозрачности
                        mat.SetFloat("_Mode", 0); // Режим Opaque
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = -1; // Сброс рендера к стандартному
                    }
                }
            }
        }

        // Очищаем список прозрачных объектов
        transparentObjects.Clear();
    }
}