using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public int targetSceneIndex; // The scene index that triggers the music    
    public static AudioManager instance;

    public AudioMixerGroup mixerGroup;
    public Sound[] sounds;

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
            Play("LevelMusic");
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

        s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

        s.source.Play();
    }

    public void Stop(string sound)
    {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found!");
            return;
        }

        s.source.Stop();
    }
}
