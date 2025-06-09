using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForestCamp : MonoBehaviour
{
    public Transform campCenter;              // Центр лагеря
    public float respawnRadius = 10f;         // Радиус лагеря
    float respawnTime = 60f;           // Время до респауна
    public GameObject timerPrefab;            // Префаб таймера
    public float visibilityDistance = 10f;    // Дистанция для видимости

    public string campType;                   // Тип лагеря (легкий, средний и т.п.)
    public string location;                   // Локация лагеря (лес, пустыня и т.п.)
    public int campBudget;                    // Бюджет для респауна мобов

    private ForestUnitDatabase unitDatabase;  // База данных мобов
    private List<GameObject> spawnedUnits = new List<GameObject>(); // Список мобов в лагере
    private GameObject timerInstance;         // Экземпляр таймера
    private Image timerImage;                 // Круговой индикатор таймера
    private Text timerText;                   // Текст таймера
    private float countdown;
    private bool isRespawning = false;
    public bool boss = false;

    private CanvasGroup timerCanvasGroup;

    [Header("Visual Settings")]
    public float radiusLineYOffset = 0.05f;       // Насколько поднять линию над землёй
    public int lineSegments = 64;                 // Количество точек круга
    public float lineWidth = 0.02f;               // Толщина линии
    public Color lineColor = Color.green;         // Цвет линии
    private LineRenderer campRadiusRenderer;      // LineRenderer радиуса лагеря

    void Start()
    {
        unitDatabase = FindObjectOfType<ForestUnitDatabase>(); // Находим базу данных юнитов
        if (boss) respawnTime = 300f;
        countdown = respawnTime;

        // Создаем и настраиваем таймер в UI игрока
        timerInstance = Instantiate(timerPrefab, GameObject.Find("Canvas").transform);
        timerImage = timerInstance.GetComponentInChildren<Image>();
        timerText = timerInstance.GetComponentInChildren<Text>();

        timerCanvasGroup = timerInstance.GetComponent<CanvasGroup>();
        timerCanvasGroup.alpha = 1f;

        // Создаём LineRenderer на объекте лагеря
        campRadiusRenderer = gameObject.AddComponent<LineRenderer>();
        campRadiusRenderer.positionCount = lineSegments + 1;
        campRadiusRenderer.loop = true;
        campRadiusRenderer.useWorldSpace = true;
        campRadiusRenderer.startWidth = lineWidth;
        campRadiusRenderer.endWidth = lineWidth;
        campRadiusRenderer.material = new Material(Shader.Find("Sprites/Default"));
        campRadiusRenderer.startColor = lineColor;
        campRadiusRenderer.endColor = lineColor;

        DrawCampRadius();
    }

    void DrawCampRadius()
    {
        if (campCenter == null) return;

        Vector3[] positions = new Vector3[lineSegments + 1];
        float angleStep = 360f / lineSegments;

        for (int i = 0; i <= lineSegments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * respawnRadius;
            float z = Mathf.Sin(angle) * respawnRadius;

            positions[i] = campCenter.position + new Vector3(x, radiusLineYOffset, z);
        }

        campRadiusRenderer.SetPositions(positions);
    }

    void Update()
    {
        UpdateTimerPosition();
        RemoveDeadUnits();

        if (!AnyForestUnitInCamp() && !isRespawning)
        {
            StartCoroutine(StartRespawnCountdown());
        }

        // Проверяем, насколько далеко игрок от лагеря, и скрываем таймер при необходимости
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        float distanceToCamp = Vector3.Distance(player.position, campCenter.position);
        timerCanvasGroup.alpha = (distanceToCamp > visibilityDistance) ? 0f : 1f;
    }

    // Удаляет из списка уничтоженные юниты
    void RemoveDeadUnits()
    {
        spawnedUnits.RemoveAll(unit => unit == null);
    }

    // Проверяет, есть ли в лагере живые юниты
    bool AnyForestUnitInCamp()
    {
        foreach (var unit in spawnedUnits)
        {
            if (unit != null) return true;
        }
        return false;
    }

    // Запуск таймера респауна
    IEnumerator StartRespawnCountdown()
    {
        isRespawning = true;
        countdown = respawnTime;
        timerImage.fillAmount = 1;
        timerInstance.SetActive(true);

        while (countdown > 0)
        {
            countdown -= Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }

        timerInstance.SetActive(false);

        SpawnUnitsByBudget(campBudget, campType, location);
        isRespawning = false;
    }

    // Обновление позиции таймера
    void UpdateTimerPosition()
    {
        timerInstance.transform.position = Camera.main.WorldToScreenPoint(campCenter.position);
    }

    // Обновление UI таймера
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(countdown / 60F);
        int seconds = Mathf.FloorToInt(countdown % 60F);
        timerText.text = $"{minutes:00}:{seconds:00}";
        timerImage.fillAmount = countdown / respawnTime;
    }

    // Спавн мобов в лагере
    void SpawnUnitsByBudget(int budget, string type, string location)
    {
        List<ForestUnit> matchingUnits = unitDatabase.GetUnitsByCriteria(type, location);
        int remainingBudget = budget;

        while (remainingBudget > 0 && matchingUnits.Count > 0)
        {
            ForestUnit unitToSpawn = matchingUnits[Random.Range(0, matchingUnits.Count)];

            if (unitToSpawn.cost <= remainingBudget)
            {
                GameObject newUnit = Instantiate(unitToSpawn.prefab, GetRandomPositionInRadius(), Quaternion.identity);

                // Привязываем к лагерю и добавляем в список
                EnemyAI enemyAI = newUnit.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.campCenter = campCenter; // Привязываем центр лагеря
                    enemyAI.HP = unitToSpawn.health;
                    enemyAI.damage = unitToSpawn.damage;
                }

                spawnedUnits.Add(newUnit);
                remainingBudget -= unitToSpawn.cost;
            }
            else
            {
                matchingUnits.Remove(unitToSpawn);
            }
        }
    }

    // Возвращает случайную точку внутри лагеря
    Vector3 GetRandomPositionInRadius()
    {
        Vector2 randomCircle = Random.insideUnitCircle * respawnRadius;
        return campCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    void OnDrawGizmosSelected()
    {
        if (campCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(campCenter.position, 3f); // 3f — радиус, заданный в EnemyAI
        }
    }
}