using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public int targetSceneIndex;
    public static AudioManager instance;

    [Header("Settings")]
    public int poolSize = 20;                  // ile AudioSource w puli
    public AudioMixerGroup defaultMixerGroup;

    [Header("Sounds")]
    public Sound[] sounds;

    private List<AudioSource> pool = new List<AudioSource>();
    private AudioSource musicSource;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Tworzymy pulê AudioSource
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = defaultMixerGroup;
            pool.Add(src);
        }

        // Dedykowany source dla muzyki
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.outputAudioMixerGroup = defaultMixerGroup;
    }

    // Pobranie wolnego AudioSource z puli
    private AudioSource GetPooledSource()
    {
        foreach (var src in pool)
        {
            if (!src.isPlaying) return src;
        }
        Debug.LogWarning("Audio pool exhausted! Consider increasing pool size.");
        return null;
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, x => x.name == name);
        if (s == null) { Debug.LogWarning($"Sound {name} not found"); return; }

        if (s.isMusic)
        {
            PlayMusic(s);
        }
        else
        {
            PlayEffect(s);
        }
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

    private void PlayEffect(Sound s)
    {
        var src = GetPooledSource();
        if (src == null) return;

        src.clip = s.clip;
        src.loop = s.loop;
        src.pitch = s.GetRandomPitch();
        src.volume = 0f; // start silent
        src.outputAudioMixerGroup = s.mixerGroup != null ? s.mixerGroup : defaultMixerGroup;

        src.time = s.startTime;
        src.Play();

        // fade in
        src.DOFade(s.GetRandomVolume(), s.fadeInSeconds);

        if (s.duration > 0f)
        {
            DOVirtual.DelayedCall(s.duration, () => StopWithFade(src, s.fadeOutTime));
        }
    }

    private void PlayMusic(Sound s)
    {
        // crossfade muzyki
        if (musicSource.isPlaying)
        {
            musicSource.DOFade(0f, s.fadeOutTime).OnComplete(() =>
            {
                musicSource.clip = s.clip;
                musicSource.pitch = s.GetRandomPitch();
                musicSource.volume = 0f;
                musicSource.loop = true;
                musicSource.Play();
                musicSource.DOFade(s.GetRandomVolume(), s.fadeInSeconds);
            });
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.pitch = s.GetRandomPitch();
            musicSource.volume = 0f;
            musicSource.loop = true;
            musicSource.Play();
            musicSource.DOFade(s.GetRandomVolume(), s.fadeInSeconds);
        }
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, x => x.name == name);
        if (s == null) return;

        if (s.isMusic)
        {
            musicSource.DOFade(0f, s.fadeOutTime).OnComplete(() => musicSource.Stop());
        }
        else
        {
            foreach (var src in pool)
            {
                if (src.isPlaying && src.clip == s.clip)
                {
                    StopWithFade(src, s.fadeOutTime);
                }
            }
        }
    }

    private void StopWithFade(AudioSource src, float fadeTime)
    {
        src.DOFade(0f, fadeTime).OnComplete(() => src.Stop());
    }
}
