using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class VolumeIconBar : MonoBehaviour
{
    [SerializeField] private Image[] icons;
    [SerializeField] private Slider slider;


    private void Start()
    {
        // Hook up clicks for each icon
        for (int i = 0; i < icons.Length; i++)
        {
            int index = i;
            Button btn = icons[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SetVolumeLevel(index + 1));
            }
        }
    }

    public void SetVolumeLevel(int level)
    {
        slider.value = level;
    }
}
