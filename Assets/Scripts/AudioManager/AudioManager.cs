using UnityEngine.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public int targetSceneIndex;
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
            s.source.volume = 0f; // start always silent (for fade in)
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

        if (targetSceneIndex == 0)
        {
            Debug.Log("Playing MainMenuMusic");
            Play("MainMenuMusic");
        }
        else if (targetSceneIndex == 1)
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

        float targetVolume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));

        if (s.allowMultiple)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.clip = s.clip;
            newSource.loop = s.loop;
            newSource.outputAudioMixerGroup = s.mixerGroup;
            newSource.volume = 0f;
            newSource.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            newSource.time = s.startTime;
            newSource.Play();

            // fade in
            newSource.DOFade(targetVolume, s.fadeInSeconds);

            activeSources.Add(newSource);

            if (s.duration > 0f)
            {
                DOVirtual.DelayedCall(s.duration, () =>
                {
                    StopWithFade(newSource, s.fadeOutTime);
                });
            }
        }
        else
        {
            s.source.volume = 0f;
            s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.time = s.startTime;
            s.source.Play();

            // fade in
            s.source.DOFade(targetVolume, s.fadeInSeconds);

            if (s.duration > 0f)
            {
                DOVirtual.DelayedCall(s.duration, () =>
                {
                    Stop(sound);
                });
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
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                if (activeSources[i].clip == s.clip)
                {
                    StopWithFade(activeSources[i], s.fadeOutTime);
                    activeSources.RemoveAt(i);
                }
            }
        }
        else
        {
            if (s.source != null && s.source.isPlaying)
            {
                s.source.DOFade(0f, s.fadeOutTime).OnComplete(() =>
                {
                    s.source.Stop();
                    s.source.time = 0f;
                });
            }
        }
    }

    private void StopWithFade(AudioSource source, float fadeOutTime)
    {
        if (source == null) return;

        source.DOFade(0f, fadeOutTime).OnComplete(() =>
        {
            source.Stop();
            Destroy(source);
        });
    }
}
