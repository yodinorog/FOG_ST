using System;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera; // Камера для миникарты
    public RectTransform minimapBounds; // Границы миникарты (UI элемент RawImage или другой RectTransform для миникарты)
    public GameObject enemyIconPrefab; // Префаб иконки врага (например, Image)
    public GameObject lootIconPrefab; // Префаб иконки для выпавших предметов
    public Transform playerTransform; // Игрок как центральная точка миникарты

    public LayerMask enemyLayer; // Слой врагов
    public LayerMask lootLayer;  // Слой предметов

    private List<GameObject> spawnedEnemyIcons = new List<GameObject>(); // Список сгенерированных иконок врагов
    private List<GameObject> spawnedLootIcons = new List<GameObject>();  // Список сгенерированных иконок предметов

    private List<Transform> enemies = new List<Transform>(); // Список врагов на сцене
    private List<Transform> lootItems = new List<Transform>(); // Список предметов на сцене

    void Start()
    {
        // Инициализируем врагов и предметы, фильтруем по слоям
        FindObjectsByLayer(enemyLayer, enemies); // Поиск врагов по слою
        FindObjectsByLayer(lootLayer, lootItems); // Поиск предметов по слою

        // Создаем иконки для врагов
        foreach (Transform enemy in enemies)
        {
            GameObject icon = Instantiate(enemyIconPrefab, minimapBounds);
            spawnedEnemyIcons.Add(icon);
        }

        // Создаем иконки для предметов
        foreach (Transform loot in lootItems)
        {
            GameObject icon = Instantiate(lootIconPrefab, minimapBounds);
            spawnedLootIcons.Add(icon);
        }
    }

    void Update()
    {
        // Обновляем положение всех иконок врагов
        for (int i = 0; i < enemies.Count; i++)
        {
            UpdateIconPosition(enemies[i], spawnedEnemyIcons[i]);
        }

        // Обновляем положение всех иконок предметов
        for (int i = 0; i < lootItems.Count; i++)
        {
            UpdateIconPosition(lootItems[i], spawnedLootIcons[i]);
        }
    }

    // Метод для поиска объектов по слою и добавления их в список
    void FindObjectsByLayer(LayerMask layer, List<Transform> objectList)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(); // Получаем все объекты на сцене
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & layer) != 0) // Проверяем, находится ли объект на нужном слое
            {
                objectList.Add(obj.transform); // Добавляем объект в список
            }
        }
    }

    // Обновляет положение иконки на миникарте
    void UpdateIconPosition(Transform trackedObject, GameObject icon)
    {
        // Преобразуем мировые координаты объекта в координаты экрана относительно камеры миникарты
        Vector3 screenPos = minimapCamera.WorldToViewportPoint(trackedObject.position);

        // Проверяем, вышел ли объект за пределы камеры
        bool isOutOfBounds = screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1 || screenPos.z < 0;

        // Ограничиваем координаты по границам миникарты
        screenPos.x = Mathf.Clamp(screenPos.x, 0, 1);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, 1);

        // Преобразуем координаты в UI координаты (относительно размеров миникарты)
        Vector2 minimapPosition = new Vector2(screenPos.x * minimapBounds.sizeDelta.x, screenPos.y * minimapBounds.sizeDelta.y);

        // Устанавливаем позицию иконки на миникарте
        icon.GetComponent<RectTransform>().anchoredPosition = minimapPosition;

        // Если объект вне миникарты, можно поменять цвет иконки или добавить обводку
        if (isOutOfBounds)
        {
            icon.GetComponent<Image>().color = Color.red; // Например, меняем цвет иконки, если объект за пределами миникарты
        }
        else
        {
            icon.GetComponent<Image>().color = Color.white; // Возвращаем цвет иконки, если объект на карте
        }
    }
}