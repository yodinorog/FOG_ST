using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public Unit[] units;

    public Text timerText;         // UI элемент для отображения времени
    public Text waveText;          // UI элемент для отображения номера волны
    public GameObject enemyPrefab; // Префаб врага, который будет спавниться
    public Vector3 spawnPoint;     // Точка спавна врагов
    public float spawnDelay = 0.5f; // Задержка между спавном каждого врага

    private float elapsedTime = 0f; // Общее прошедшее время
    private int minutesPassed = 0;  // Количество полных минут (номера волн)
    private bool isTenSecondsBeforeMinuteCalled = false; // Флаг для вызова функции за 10 секунд до минуты

    [Header("Wave Timer Bar")]
    public GameObject waveIndicatorPrefab;   // Префаб для волны (иконка)
    public RectTransform waveTimerBar;       // Полоса прогресса для волны
    public float barWidth = 500f;            // Ширина полосы прогресса
    public float waveIndicatorSpeed = 60f;   // Скорость движения индикатора волны (1 минута)

    private List<GameObject> waveIndicators = new List<GameObject>(); // Список активных индикаторов

    public GameObject winCutscene; // Объект с победной сценой
    public int currentWave;        // Текущая волна
    public int minutes;
    public int seconds;

    private float timeSinceStart = -60f; // начинаем с -1:00

    public float GetElapsedTime()
    {
        return timeSinceStart;
    }

    void Awake()
    {
        foreach (Unit u in units)
        {
            Debug.Log($"Unit: {u.prefab.name}, Cost: {u.cost}");
        }
        Time.timeScale = 0f;

    }

    void Start()
    {
        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", -1);
        if (savedQuality >= 0)
        {
            QualitySettings.SetQualityLevel(savedQuality);
        }
        // Инициализация таймера
        UpdateTimerUI(timeSinceStart);
        UpdateWaveText(1); // Отображение начальной волны
        StartCoroutine(SpawnEnemies()); // Спавним врагов
        StartNewWaveIndicator();        // Запускаем индикаторы волн

        var roadManager = FindObjectOfType<RoadGenerator>();
        roadManager.Generate();
    }

    void Update()
    {
        timeSinceStart += Time.deltaTime;

        minutes = Mathf.FloorToInt(timeSinceStart / 60f);
        seconds = Mathf.FloorToInt(timeSinceStart % 60f);

        UpdateTimerUI(timeSinceStart);

        if (timeSinceStart < 0f) return; // ← Ждём, пока не наступит 0:00

        if (minutes > minutesPassed)
        {
            minutesPassed = minutes;
            isTenSecondsBeforeMinuteCalled = false;
            OnMinutePassed();
        }

        if (seconds == 50 && !isTenSecondsBeforeMinuteCalled)
        {
            isTenSecondsBeforeMinuteCalled = true;
            OnTenSecondsBeforeMinute();
        }

        UpdateWaveIndicators();

        if (currentWave >= 30 && IsBossDefeated())
        {
            EndGame();
        }
    }

    // Проверка, убит ли босс на 10-й волне
    bool IsBossDefeated()
    {
        GameObject boss = GameObject.FindWithTag("FBoss");
        return boss == null; // Если босса нет в сцене, значит он побежден
    }

    // Функция, которая вызывается после каждой полной минуты
    void OnMinutePassed()
    {
        Debug.Log("Прошла ещё одна минута! Волна " + (minutesPassed + 1));
        UpdateWaveText(minutesPassed + 1); // Обновляем текст с номером волны
        StartCoroutine(SpawnEnemies()); // Начинаем спавнить врагов с задержкой
        FindObjectOfType<AudioManager>().Play("Wave");
        StartNewWaveIndicator(); // Запускаем новый индикатор для волны
    }

    // Функция, которая вызывается за 10 секунд до полной минуты
    void OnTenSecondsBeforeMinute()
    {
        Debug.Log("Осталось 10 секунд до новой минуты!");
    }

    // Обновляем текст с номером текущей волны
    void UpdateWaveText(int waveNumber)
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + waveNumber;
        }
    }

    // Обновляем отображение времени на таймере
    void UpdateTimerUI(float seconds)
    {
        int displaySeconds = Mathf.FloorToInt(seconds);
        int minutes = displaySeconds / 60;
        int secs = Mathf.Abs(displaySeconds % 60); // важно: ABS

        string sign = displaySeconds < 0 ? "-" : "";
        timerText.text = $"{sign}{Mathf.Abs(minutes):D2}:{secs:D2}";
    }

    void Spawn(int cost)
    {
        Unit u = Array.Find(units, unit => unit.cost == cost);
        if (u == null)
        {
            Debug.LogWarning($"Unit с cost={cost} не найден в массиве units!");
            return;
        }
        Instantiate(u.prefab, spawnPoint, Quaternion.identity);
        Debug.Log($"Заспавнен юнит {u.prefab.name} с cost={cost}");
    }

    // Корутин для спавна врагов с задержкой
    IEnumerator SpawnEnemies()
    {
        int waveNumber = minutesPassed;
        currentWave = waveNumber;
        int wavePower = minutesPassed * 10;
        int half = wavePower / 2;

        int i = 0;
        int count = 0;

        while (wavePower >= 2)
        {
            if (wavePower <= half)
            {
                if (waveNumber >= 27) i = 20;
                else if (waveNumber >= 24) i = 16;
                else if (waveNumber >= 17) i = 8;
                else if (waveNumber >= 7) i = 4;
                else if (waveNumber >= 4) i = 3;
            }
            else
            {
                if (waveNumber % 10 == 0) i = wavePower; // Boss           
                else if (waveNumber >= 21) i = 12;
                else if (waveNumber >= 14) i = 6;
                else if (waveNumber >= 11) i = 5;
                else if (waveNumber >= 1) i = 2;
            }

            Spawn(i);
            wavePower -= i;
            count++;

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // Начинаем новый индикатор волны
    void StartNewWaveIndicator()
    {
        // Создаем новый индикатор волны на правой стороне полосы
        //GameObject waveIndicator = Instantiate(waveIndicatorPrefab, waveTimerBar);
        //RectTransform indicatorTransform = waveIndicator.GetComponent<RectTransform>();
        //indicatorTransform.anchoredPosition = new Vector2(barWidth / 2, 0); // Помещаем на правый край полосы
        //waveIndicators.Add(waveIndicator); // Добавляем в список активных индикаторов
    }

    // Обновляем положение всех индикаторов волн
    void UpdateWaveIndicators()
    {
        for (int i = waveIndicators.Count - 1; i >= 0; i--)
        {
            GameObject waveIndicator = waveIndicators[i];
            RectTransform indicatorTransform = waveIndicator.GetComponent<RectTransform>();

            // Рассчитываем новое положение индикатора
            float newPosition = indicatorTransform.anchoredPosition.x - (barWidth / 60f) * Time.deltaTime;
            indicatorTransform.anchoredPosition = new Vector2(newPosition, 0);

            // Удаляем индикатор, если он ушел за левую границу
            if (newPosition < -(barWidth / 2))
            {
                Destroy(waveIndicator);
                waveIndicators.RemoveAt(i); // Удаляем из списка
            }
        }
    }

    // Завершение игры
    private void EndGame()
    {
        Debug.Log("Игра завершена! Победа!");
        winCutscene.SetActive(true); // Включаем победную сцену
        //Time.timeScale = 0f;        // Останавливаем время
    }
}