using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Dropdown resolutionDropdown;
    public Slider generalVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;
    public Resolution[] resolutions;
    private float previousNumber;
    private int number;

    void Start()
    {
        // Set the sliders to match the current mixer values
        float volume;

        if (audioMixer.GetFloat("volume", out float dB))
        {
            if (dB <= -79f)
            {
                volume = 0;
            }
            else
            {
                float normalized = Mathf.InverseLerp(-50f, 0f, dB); // maps dB to 0..1
                volume = Mathf.RoundToInt(normalized * 10f);        // maps 0..1 → 0..10
            }
            generalVolumeSlider.value = volume;
        }

        if (audioMixer.GetFloat("music", out dB))
        {
            if (dB <= -79f)
            {
                volume = 0;
            }
            else
            {
                float normalized = Mathf.InverseLerp(-50f, 0f, dB); // maps dB to 0..1
                volume = Mathf.RoundToInt(normalized * 10f);        // maps 0..1 → 0..10
            }
            musicVolumeSlider.value = volume;
        }

        if (audioMixer.GetFloat("sfx", out dB))
        {
            if (dB <= -79f)
            {
                volume = 0;
            }
            else
            {
                float normalized = Mathf.InverseLerp(-50f, 0f, dB); // maps dB to 0..1
                volume = Mathf.RoundToInt(normalized * 10f);        // maps 0..1 → 0..10
            }
            sfxVolumeSlider.value = volume;
        }

        // Set up resolution dropdown options
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        //string previousOption;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height + " " +resolutions[i].refreshRate + "Hz";
            //if (option != previousOption)
            //{

            //}
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        options.Reverse();

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = resolutions.Length - currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void ScrollStep()
    {
        number = resolutionDropdown.value;
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            string goName = EventSystem.current.currentSelectedGameObject.name;
            // "Item 19: 720x480 75Hz"

            string[] parts = goName.Split(' ');
            // ["Item", "19:", "720x480", "75Hz"]

            // remove ":" and parse to int
            string numberText = parts[1].TrimEnd(':');
            number = int.Parse(numberText);            
        }
        Scrollbar scrollbar = GetComponentInChildren<Scrollbar>();

        if (previousNumber < number)
        {
            if (number > 3)
            {
                float length = resolutions.Length;
                float offset = 1 / (length - 4f);
                float step = 1 - (offset * (number - 3));

                scrollbar.value = step;
            }
        }
        else
        {
            if (number > 2)
            {
                float length = resolutions.Length;
                float offset = 1 / (length - 4f);
                float step = 1 - (offset * (number - 3));

                scrollbar.value = step;
            }
        }

        previousNumber = number;
    }

    public void SetGeneralVolume(float volume)
    {
        float normalized = volume / 10;   
        float dB = (normalized > 0) ? Mathf.Lerp(-50f, 0f, normalized) : -80f;
        audioMixer.SetFloat("volume", dB);
    }

    public void SetMusicVolume(float volume)
    {
        float normalized = volume / 10;
        float dB = (normalized > 0) ? Mathf.Lerp(-50f, 0f, normalized) : -80f;
        audioMixer.SetFloat("music", dB);
    }

    public void SetSFXVolume(float volume)
    {
        float normalized = volume / 10;
        float dB = (normalized > 0) ? Mathf.Lerp(-50f, 15f, normalized) : -80f;
        audioMixer.SetFloat("sfx", dB);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutions.Length-resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
