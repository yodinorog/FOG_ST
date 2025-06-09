using System;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip[] stepClips; // Массив звуков шагов
    public AudioSource audioSource; // Источник звука
    public float baseStepInterval = 0.5f; // Базовый интервал между шагами
    private float stepTimer = 0f; // Таймер для отслеживания шагов

    private MovementInput movementInput; // Ссылка на скрипт MovementInput

    void Start()
    {
        movementInput = GetComponent<MovementInput>(); // Получаем компонент MovementInput
    }

    public void OnPlayerStep()
    {
        if (movementInput.Speed > 0.1f)
            PlayStepSound();
    }

    // Метод для воспроизведения случайного звука шага
    void PlayStepSound()
    {
        if (stepClips.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, stepClips.Length);
            AudioClip stepClip = stepClips[randomIndex];

            if (stepClip != null)
            {
                audioSource.PlayOneShot(stepClip);
            }
        }
    }
}