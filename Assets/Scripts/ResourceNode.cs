using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResourceNode : MonoBehaviour
{
    public float respawnTime = 120f; // 2 минуты
    public GameObject timerUI;       // UI-таймер
    private Text timerText;
    private bool isDepleted = false;
    private float timer = 0f;

    void Start()
    {
        if (timerUI != null)
        {
            timerText = timerUI.GetComponentInChildren<Text>();
            timerUI.SetActive(false);
        }
    }

    public void Deplete()
    {
        if (isDepleted) return;

        isDepleted = true;
        gameObject.SetActive(false); // Отключаем руду
        if (timerUI != null) timerUI.SetActive(true);

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        timer = respawnTime;

        while (timer > 0)
        {
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(timer).ToString();

            timer -= Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(true);
        if (timerUI != null) timerUI.SetActive(false);
        isDepleted = false;
    }

    public bool IsDepleted()
    {
        return isDepleted;
    }
}