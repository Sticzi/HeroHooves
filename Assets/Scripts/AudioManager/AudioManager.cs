using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public int targetSceneIndex; // The scene index that triggers the music    
    public static AudioManager instance;

    public AudioMixerGroup mixerGroup;
    public Sound[] sounds;
    private List<AudioSource> activeSources = new List<AudioSource>();

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixerGroup;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //StartMusic();
    }

    private void Start()
    {
        StartMusic();
    }

    public void StartMusic()
    {
        targetSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Active Scene Index: " + targetSceneIndex);
        Stop("LevelMusic");
        Stop("MainMenuMusic");

        if (targetSceneIndex == 0) // Assuming 0 is Main Menu
        {
            Debug.Log("Playing MainMenuMusic");
            Play("MainMenuMusic");
        }
        else if (targetSceneIndex == 1) // Assuming 1 is the Level
        {
            Debug.Log("Playing LevelMusic");
            Play("LevelMusic2");
        }
    }

    public void Play(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }

        if (s.allowMultiple)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = s.clip;
            newSource.loop = s.loop;
            newSource.outputAudioMixerGroup = s.mixerGroup;
            newSource.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            newSource.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            newSource.time = s.startTime; // Set playback position
            newSource.Play();

            activeSources.Add(newSource);

            if (s.duration > 0f)
            {
                DOVirtual.DelayedCall(s.duration, () => {
                    newSource.Stop();
                    activeSources.Remove(newSource);
                    Destroy(newSource);
                });
            }
        }
        else
        {
            s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.time = s.startTime; // Set playback position
            s.source.Play();

            if (s.duration > 0f)
            {
                DOVirtual.DelayedCall(s.duration, () => s.source.Stop());
            }
        }
    }

    public void Stop(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }

        if (s.allowMultiple)
        {
            // Usuniêcie wszystkich aktywnych instancji tego dŸwiêku
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                if (activeSources[i].clip == s.clip)
                {
                    activeSources[i].Stop();
                    Destroy(activeSources[i]);
                    activeSources.RemoveAt(i);
                }
            }
        }
        else
        {
            // Zatrzymanie pojedynczego Ÿród³a dŸwiêku
            if (s.source != null)
            {
                s.source.Stop();
                s.source.time = 0f; // Resetowanie pozycji odtwarzania
            }
        }
    }

}
