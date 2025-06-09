using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;


public class InGameMenu : MonoBehaviour
{
    public Slider progressBar;
    public GameObject loadingScreen;

    [Header("Audio Settings")]
    public Slider musicVolumeSlider;          // Ползунок громкости музыки
    public Slider soundVolumeSlider;          // Ползунок громкости звуков
    private AudioManager audioManager;
    private RoadGenerator roadGenerator;

    [Header("Graphics Settings")]
    public Toggle lowGraphicsToggle;          // Переключатель низкой графики
    public Toggle mediumGraphicsToggle;       // Переключатель средней графики
    public Toggle highGraphicsToggle;         // Переключатель высокой графики

    public GameObject fullMap;
    public GameObject scaledMap;

    public PlayableDirector cutsceneDirector;

    public void Start()
    {
        //cutsceneDirector = GameObject.Find("StartingCutscene").GetComponent<PlayableDirector>();
        audioManager = FindObjectOfType<AudioManager>();
        roadGenerator = FindObjectOfType<RoadGenerator>();

        if (audioManager != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(audioManager.SetMusicVolume);
            soundVolumeSlider.onValueChanged.AddListener(audioManager.SetSoundVolume);
        }

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);

        lowGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(0); });
        mediumGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(1); });
        highGraphicsToggle.onValueChanged.AddListener(delegate { SetGraphicsQuality(2); });

        SetGraphicsQuality(2);
    }

    public void Sure()
    {
        Time.timeScale = 1f;

        audioManager.Play("Menu");
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        //RemoveMenuObjectsFromDontDestroyOnLoad();
    }

    public void NotSure(GameObject exitmenu)
    {
        audioManager.Play("Menu");
        exitmenu.SetActive(false);
    }

    public void ExitMenuOpen(GameObject exitmenu)
    {
        audioManager.Play("Menu");
        exitmenu.SetActive(true);
    }

    public void ShowMenu(GameObject menu)
    {
        FindObjectOfType<AudioManager>().Play("Menu");
        menu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume(GameObject menu)
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
        FindObjectOfType<AudioManager>().Play("Menu");
    }

    public void Settings(GameObject settings)
    {
        settings.SetActive(false);
    }

    public void MainMenu(GameObject LoadingScreen)
    {
        loadingScreen.SetActive(true);
        Time.timeScale = 1f;
        StartCoroutine(LoadLevelAsinc());

    }

    private IEnumerator LoadLevelAsinc()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
        while (!asyncLoad.isDone)
        {
            progressBar.value = asyncLoad.progress;
            yield return null;
        }
    }

    public void ScaleUp()
    {
        scaledMap.SetActive(true);
        fullMap.SetActive(false);
    }

    public void ScaleDown()
    {
        scaledMap.SetActive(false);
        fullMap.SetActive(true);
    }

    public void Exit()
    {
        Time.timeScale = 1f;
        Application.Quit();
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
        PlayerPrefs.Save(); // сохраняем на диск
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

    public void RegenerateRoad()
    {
        roadGenerator.Generate();
        FindObjectOfType<AudioManager>().Play("Menu");
    }

    public void SaveRoad(GameObject menu)
    {
        Time.timeScale = 1f;
        roadGenerator.BuildRoad();
        menu.SetActive(false);
        FindObjectOfType<AudioManager>().Play("Menu");
        //cutsceneDirector.Play();
    }
}
