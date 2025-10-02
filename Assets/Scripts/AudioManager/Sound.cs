using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume;
    [Range(0f, 1f)] public float volumeVariance;

    [Range(.1f, 3f)] public float pitch;
    [Range(0f, 1f)] public float pitchVariance;

    public float startTime = 0f;
    public float duration = 0f;
    public bool loop = false;
    public bool isMusic = false;

    [Header("Fade")]
    public float fadeInSeconds = 0;
    public float fadeOutTime = 0f;

    public AudioMixerGroup mixerGroup;

    public float GetRandomVolume()
    {
        return volume * (1f + Random.Range(-volumeVariance / 2f, volumeVariance / 2f));
    }

    public float GetRandomPitch()
    {
        return pitch * (1f + Random.Range(-pitchVariance / 2f, pitchVariance / 2f));
    }
}
