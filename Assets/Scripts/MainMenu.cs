using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsPanel;          // Панель настроек
    public GameObject mainMenuPanel;          // Панель главного меню
    public GameObject customizationPanel;     // Панель кастомизации
    public Button playButton;                 // Кнопка "Играть"
    public Button settingsButton;             // Кнопка "Настройки"
    public Button backButton;                 // Кнопка "Назад" в меню настроек
    public Button customizationButton;        // Кнопка для перехода в кастомизацию

    [Header("Language Settings")]
    public Button languageToggleButton;           // Кнопка переключения языка
    public Image languageIconImage;               // Изображение иконки языка
    public Sprite russianIcon;                    // Иконка русского языка
    public Sprite englishIcon;                    // Иконка английского языка

    private enum Language { Russian, English }
    private Language currentLanguage = Language.English;

    [Header("Player Stats UI")]
    public GameObject playerStatsPanel;           // Панель с характеристиками игрока
    public Button toggleStatsButton;              // Кнопка включения/выключения
    public Text damageText;                       // Текст урона
    public Text speedText;                        // Текст скорости
    public Image damageIcon;                      // Иконка урона
    public Image speedIcon;                       // Иконка скорости

    public Sprite damageSprite;                   // Спрайт урона
    public Sprite speedSprite;                    // Спрайт скорости

    private bool statsVisible = false;

    [Header("Audio Settings")]
    public Slider musicVolumeSlider;          // Ползунок громкости музыки
    public Slider soundVolumeSlider;          // Ползунок громкости звуков
    public AudioManager audioManager;

    [Header("Graphics Settings")]
    public Toggle lowGraphicsToggle;          // Переключатель низкой графики
    public Toggle mediumGraphicsToggle;       // Переключатель средней графики
    public Toggle highGraphicsToggle;         // Переключатель высокой графики

    [Header("Shadow Settings")]
    public Toggle noShadowsToggle;            // Переключатель "Без теней"
    public Toggle mediumShadowsToggle;        // Переключатель "Средние тени"
    public Toggle highShadowsToggle;          // Переключатель "Высокие тени"

    [Header("Customization Transition")]
    public RectTransform mainMenuTransform;   // RectTransform главного меню
    public RectTransform customizationTransform; // RectTransform панели кастомизации
    public Camera playerCamera;               // Камера для отображения модели игрока
    public float slideDuration = 0.5f;        // Длительность анимации
    public float cameraOffset = 1f;           // Сдвиг камеры по оси X

    private bool isCustomizationOpen = false; // Флаг для отслеживания состояния кастомизации

    [Header("Character")]
    public Transform characterTransform; // Ссылка на объект персонажа для вращения
    public float rotationSpeed = 5f;     // Скорость вращения персонажа

    private bool isDragging = false;
    private float previousMouseX;

    private RectTransform parentTransform; // RectTransform родительского меню

    [Header("Customization Shop")]
    public List<Item> customizationItems; // Список предметов для кастомизации
    private GameObject currentEquippedHelmet;
    private Transform headTransform; // Сюда прикрепляем шлем
    public Text crystallsText;

    public CharacterCustomization customization; // назначается в инспекторе или ищется автоматически

    [Header("Shop Pages")]
    public GameObject helmetPage;
    public GameObject colorPage;
    public GameObject abilitiesPage;

    [Header("Toggle Group")]
    public ToggleGroup shopToggleGroup;

    

    private void Awake()
    {
        // Автопоиск если не задано
        if (customization == null)
        {
            customization = FindObjectOfType<CharacterCustomization>();
        }
    }

    private void Start()
    {
        headTransform = GameObject.FindGameObjectWithTag("Head")?.transform;

        if (headTransform == null)
        {
            Debug.LogWarning("Не найден объект с тегом 'Head'. Убедитесь, что голова игрока имеет этот тег.");
        }

        languageToggleButton.onClick.AddListener(ToggleLanguage);
        toggleStatsButton.onClick.AddListener(TogglePlayerStats);
        playerStatsPanel.SetActive(false);

        audioManager = FindObjectOfType<AudioManager>();

        if (audioManager != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
            soundVolumeSlider.onValueChanged.AddListener(audioManager.SetSoundVolume);
        }

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        //customizationPanel.SetActive(false);

        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings); 
        backButton.onClick.AddListener(CloseSettings);
        customizationButton.onClick.AddListener(ToggleCustomization);

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);

        lowGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(0); });
        mediumGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(1); });
        highGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(2); });

        int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", 1); // по умолчанию среднее качество

        lowGraphicsToggle.isOn = savedQuality == 0;
        mediumGraphicsToggle.isOn = savedQuality == 1;
        highGraphicsToggle.isOn = savedQuality == 2;

        //noShadowsToggle.onValueChanged.AddListener(delegate { SetShadows(0); });
        //mediumShadowsToggle.onValueChanged.AddListener(delegate { SetShadows(1); });
        //highShadowsToggle.onValueChanged.AddListener(delegate { SetShadows(2); });

        LoadSettings();

        parentTransform = customizationTransform.parent as RectTransform;

        customizationTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentTransform.rect.width);
        // Установка PosX равным ширине customizationTransform
        Vector2 newPosition = customizationTransform.anchoredPosition;
        newPosition.x = customizationTransform.rect.width; // установка PosX на ширину
        customizationTransform.anchoredPosition = newPosition;
    }

    private void Update()
    {
        crystallsText.text = "" + PlayerDataManager.Crystalls;
        // Обрабатываем вращение персонажа, если происходит перетаскивание
        if (isDragging)
        {
            float deltaX = Input.mousePosition.x - previousMouseX;
            characterTransform.Rotate(0, -deltaX * rotationSpeed * Time.deltaTime, 0);
            previousMouseX = Input.mousePosition.x;
        }
    }

    public void OnMouseDown()
    {
        isDragging = true;
        previousMouseX = Input.mousePosition.x;
    }

    public void OnMouseUp()
    {
        isDragging = false;
    }

    public void StartGame()
    {
        audioManager.Play("Menu");
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        RemoveMenuObjectsFromDontDestroyOnLoad();
    }

    private void RemoveMenuObjectsFromDontDestroyOnLoad()
    {
        var objectsToDestroy = new string[] { "Menu" };
        foreach (var objName in objectsToDestroy)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    public void OpenSettings()
    {
        audioManager.Play("Menu");
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        audioManager.Play("Menu");
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void SetMusicVolume(float volume)
    {
        audioManager?.SetMusicVolume(volume);
    }

    private void SetSoundVolume(float volume)
    {
        audioManager?.SetSoundVolume(volume);
    }

    private void SetGraphicsQuality(int qualityIndex)
    {
        audioManager.Play("Menu");
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }

    public void OnLowGraphicsToggleChanged(bool isOn)
    {
        if (isOn) SetGraphicsQuality(0);
    }

    public void OnMediumGraphicsToggleChanged(bool isOn)
    {
        if (isOn) SetGraphicsQuality(1);
    }

    public void OnHighGraphicsToggleChanged(bool isOn)
    {
        if (isOn) SetGraphicsQuality(2);
    }

    private void SetShadows(int shadowIndex)
    {
        audioManager.Play("Menu");
        switch (shadowIndex)
        {
            case 0:
                QualitySettings.shadows = ShadowQuality.Disable;
                break;
            case 1:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                break;
            case 2:
                QualitySettings.shadows = ShadowQuality.All;
                break;
        }
    }

    private void LoadSettings()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundVolumeSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", soundVolumeSlider.value);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        audioManager.Play("Menu");
        SaveSettings();
    }

    // Функция для переключения между главным меню и окном кастомизации
    public void ToggleCustomization()
    {
        if (isCustomizationOpen)
        {
            StartCoroutine(SlidePanels(customizationTransform, mainMenuTransform, Vector2.right));
            MoveCamera(-cameraOffset);
        }
        else
        {
            StartCoroutine(SlidePanels(mainMenuTransform, customizationTransform, Vector2.left));
            MoveCamera(cameraOffset);
        }
        isCustomizationOpen = !isCustomizationOpen;
    }

    private IEnumerator SlidePanels(RectTransform currentPanel, RectTransform nextPanel, Vector2 direction)
    {
        Vector2 startPos = currentPanel.anchoredPosition;
        Vector2 endPos = startPos + direction * Screen.width;

        Vector2 nextStartPos = nextPanel.anchoredPosition - direction * Screen.width;
        nextPanel.anchoredPosition = nextStartPos;
        nextPanel.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            currentPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            nextPanel.anchoredPosition = Vector2.Lerp(nextStartPos, Vector2.zero, t);
            yield return null;
        }

        currentPanel.gameObject.SetActive(false);
    }

    private void MoveCamera(float offset)
    {
        Vector3 targetPosition = playerCamera.transform.position;
        targetPosition.x += offset;
        StartCoroutine(MoveCameraToPosition(targetPosition));
    }

    private IEnumerator MoveCameraToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = playerCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / slideDuration);
            yield return null;
        }
    }

    public void ToggleLanguage()
    {
        currentLanguage = currentLanguage == Language.English ? Language.Russian : Language.English;

        switch (currentLanguage)
        {
            case Language.English:
                languageIconImage.sprite = englishIcon;
                // Здесь можешь добавить переключение текстов, если нужно
                break;
            case Language.Russian:
                languageIconImage.sprite = russianIcon;
                // Здесь можешь добавить переключение текстов, если нужно
                break;
        }

        Debug.Log("Language switched to: " + currentLanguage);
    }

    public void TogglePlayerStats()
    {
        statsVisible = !statsVisible;
        playerStatsPanel.SetActive(statsVisible);
    }

    public void UpdatePlayerStats(float damage, float speed)
    {
        damageText.text = "Урон: " + damage;
        speedText.text = "Скорость: " + speed;
        damageIcon.sprite = damageSprite;
        speedIcon.sprite = speedSprite;
    }

    // Метод вызывается при изменении Toggle'ов
    public void OnShopTabChanged()
    {
        // Скрыть все страницы
        helmetPage.SetActive(false);
        colorPage.SetActive(false);
        abilitiesPage.SetActive(false);

        // Проверить, какой Toggle выбран
        foreach (var toggle in shopToggleGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.isOn)
            {
                string toggleName = toggle.name.ToLower();

                if (toggleName.Contains("helmet"))
                    helmetPage.SetActive(true);
                else if (toggleName.Contains("color"))
                    colorPage.SetActive(true);
                else if (toggleName.Contains("ability") || toggleName.Contains("skill"))
                    abilitiesPage.SetActive(true);

                break; // Найден активный, можно выйти
            }
        }
    }

    public void OnColorButtonClick(int colorIndex)
    {
        customization.SetCharacterColor(colorIndex);
    }
}