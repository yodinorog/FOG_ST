using UnityEngine;
using UnityEngine.UI;

public class ComboBarController : MonoBehaviour
{
    public Image comboFillImage;
    public float fillTime = 1.0f;
    public float activeWindowRatio = 0.25f; // Последние 25% — окно атаки

    private float timer = 0f;
    private bool isRunning = false;

    public void StartFilling(float totalDuration)
    {
        fillTime = totalDuration;
        timer = 0f;
        isRunning = true;
        comboFillImage.fillAmount = 0f;
        comboFillImage.color = Color.yellow;
    }

    void Update()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        float fill = Mathf.Clamp01(timer / fillTime);
        comboFillImage.fillAmount = fill;

        // Меняем цвет на зелёный в окне атаки
        if (fill >= 1f - activeWindowRatio)
        {
            comboFillImage.color = Color.green;
        }

        if (fill >= 1f)
        {
            isRunning = false;
        }
    }
}