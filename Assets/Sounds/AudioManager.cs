using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] musicTracks;

    public static AudioManager instance;

    private Dictionary<string, List<AudioSource>> activeOneShots = new Dictionary<string, List<AudioSource>>();
    private int maxSimultaneous = 5;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
        }

        foreach (Sound m in musicTracks)
        {
            m.source = gameObject.AddComponent<AudioSource>();
            m.source.clip = m.clip;
            m.source.volume = m.volume;
            m.source.pitch = m.pitch;
            m.source.loop = m.loop;
            m.source.playOnAwake = false;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // Для ударов и взрывов добавим рандомный питч и компрессию
        if (name.ToLower().Contains("hit") || name.ToLower().Contains("explosion"))
        {
            float randomPitch = UnityEngine.Random.Range(0.95f, 1.05f);
            float baseVolume = s.volume;

            if (!activeOneShots.ContainsKey(name))
                activeOneShots[name] = new List<AudioSource>();

            List<AudioSource> sources = activeOneShots[name];
            sources.RemoveAll(src => src == null || !src.isPlaying);

            int activeCount = sources.Count;

            float compressionFactor = 1f / Mathf.Log(activeCount + 2); // +2 для смещения и избежания log(0)
            float finalVolume = baseVolume * Mathf.Clamp(compressionFactor, 0.6f, 1f);

            AudioSource tempSource = gameObject.AddComponent<AudioSource>();
            tempSource.clip = s.clip;
            tempSource.volume = finalVolume;
            tempSource.pitch = randomPitch;
            tempSource.loop = false;
            tempSource.playOnAwake = false;
            tempSource.spatialBlend = 0f;
            tempSource.Play();

            sources.Add(tempSource);
            Destroy(tempSource, s.clip.length + 0.1f);
        }
        else
        {
            if (!s.source.isPlaying)
                s.source.Play();
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicTracks, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void SetMusicVolume(float volume)
    {
        foreach (Sound m in musicTracks)
        {
            m.source.volume = volume;
        }
    }

    public void SetSoundVolume(float volume)
    {
        foreach (Sound s in sounds)
        {
            s.source.volume = volume;
        }
    }
}