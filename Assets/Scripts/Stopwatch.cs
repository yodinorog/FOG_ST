using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Stopwatch : MonoBehaviour
{
    public Unit[] units;

    public Text timerText;         // UI элемент для отображения времени
    public Text waveText;          // UI элемент для отображения номера волны
    public GameObject enemyPrefab; // Префаб врага, который будет спавниться
    public Vector3 spawnPoint;   // Точка спавна врагов
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

    void Awake()
    {
        foreach (Unit u in units)
        {
            Debug.Log($"Unit: {u.prefab.name}, Cost: {u.cost}");
        }
    }

    void Start()
    {
        

        // Инициализация таймера
        timerText.text = "00:00";
        waveText.text = "Wave: 1"; // Отображение начальной волны
        StartCoroutine(SpawnEnemies()); // Спавним врагов
        StartNewWaveIndicator();        // Запускаем индикаторы волн
    }

    void Update()
    {
        // Увеличиваем время каждую секунду
        elapsedTime += Time.deltaTime;

        // Вычисляем количество минут и секунд
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);

        // Обновляем отображаемое время
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Проверяем, прошла ли полная минута
        if (minutes > minutesPassed)
        {
            minutesPassed = minutes;
            isTenSecondsBeforeMinuteCalled = false; // Сбрасываем флаг
            OnMinutePassed(); // Вызываем функцию после полной минуты
        }

        // Проверяем, если до следующей минуты осталось 10 секунд
        if (seconds == 50 && !isTenSecondsBeforeMinuteCalled)
        {
            isTenSecondsBeforeMinuteCalled = true; // Устанавливаем флаг, чтобы не вызвать функцию несколько раз
            OnTenSecondsBeforeMinute(); // Вызываем функцию за 10 секунд до новой минуты
        }

        // Обновляем положение индикаторов волн
        UpdateWaveIndicators();
    }

    // Функция, которая вызывается после каждой полной минуты
    void OnMinutePassed()
    {
        Debug.Log("Прошла ещё одна минута! Волна " + (minutesPassed + 1));
        UpdateWaveText(); // Обновляем текст с номером волны
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
    void UpdateWaveText()
    {
        waveText.text = "Wave: " + minutesPassed;
    }

    void Spawn(int cost)
    {
        Unit u = Array.Find(units, unit => unit.cost == cost);
        if (u == null)
        {
            Debug.LogWarning($"Unit с cost={cost} не найден в массиве units!");
            return;
        }
        spawnPoint = new Vector3(0, 1.5f, 71.75f);
        Instantiate(u.prefab, spawnPoint, Quaternion.identity);
        Debug.Log($"Заспавнен юнит {u.prefab.name} с cost={cost}");
    }

    // Корутин для спавна врагов с задержкой
    IEnumerator SpawnEnemies()
    {
        int waveNumber = minutesPassed;
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

            Debug.Log($"Wave Power: {wavePower}, Current Cost: {i}");
            Spawn(i);
            wavePower -= i;
            count++;

            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log($"Волна завершена. Всего заспавнено врагов: {count}");
    }

    // Начинаем новый индикатор волны
    void StartNewWaveIndicator()
    {
        // Создаем новый индикатор волны на правой стороне полосы
        GameObject waveIndicator = Instantiate(waveIndicatorPrefab, waveTimerBar);
        RectTransform indicatorTransform = waveIndicator.GetComponent<RectTransform>();
        indicatorTransform.anchoredPosition = new Vector2(barWidth / 2, 0); // Помещаем на правый край полосы
        waveIndicators.Add(waveIndicator); // Добавляем в список активных индикаторов
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
}