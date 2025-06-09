using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySound : MonoBehaviour
{
    public AudioClip[] stepClips;
    public AudioClip[] spawnClips;
    public AudioClip[] deathClips;
    public AudioClip[] attackClips;
    public AudioClip[] damageClips;

    public AudioSource audioSource;
    public float stepInterval = 0.5f;
    private float stepTimer = 0f;
    private NavMeshAgent agent;

    // Статический список всех активных EnemySound
    private static List<EnemySound> activeSounds = new List<EnemySound>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        PlayRandomSpawnSound();
    }

    private void OnEnable()
    {
        activeSounds.Add(this);
    }

    private void OnDisable()
    {
        activeSounds.Remove(this);
    }

    public void PlayRandomAttackSound()
    {
        PlayRandomSoundFromArray(attackClips);
    }

    public void PlayRandomDamageSound()
    {
        PlayRandomSoundFromArray(damageClips);
    }

    public void PlayRandomDeathSound()
    {
        PlayRandomSoundFromArray(deathClips);
    }

    public void PlayRandomSpawnSound()
    {
        PlayRandomSoundFromArray(spawnClips);
    }

    public float GetDeathSoundLength()
    {
        int randomIndex = Random.Range(0, deathClips.Length);
        return deathClips[randomIndex].length;
    }

    private void PlayRandomSoundFromArray(AudioClip[] clips)
    {
        if (clips.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        // Подсчитываем количество активных источников, которые сейчас играют
        int activeCount = 0;
        foreach (var enemySound in activeSounds)
        {
            if (enemySound != null && enemySound.audioSource.isPlaying)
                activeCount++;
        }

        // Нелинейная (логарифмическая) компрессия
        float baseVolume = 1f;
        float compressionFactor = 1f / Mathf.Log10(activeCount + 10f); // +10 для сглаживания
        float finalVolume = baseVolume * Mathf.Clamp(compressionFactor, 0.5f, 1f); // Минимум 50%

        audioSource.PlayOneShot(clip, finalVolume);
    }
}