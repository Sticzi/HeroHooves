﻿using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = .75f;
    [Range(0f, 1f)]
    public float volumeVariance = .1f;

    [Range(.1f, 3f)]
    public float pitch = 1f;
    [Range(0f, 1f)]
    public float pitchVariance = .1f;

    public float startTime = 0f;
    public float duration = 0f;
    public bool loop = false;
    public bool allowMultiple = false; // Dodajemy tę opcję tutaj, żeby nie zmieniać AudioManagera

    public AudioMixerGroup mixerGroup;

    [HideInInspector]
    public AudioSource source;
}
