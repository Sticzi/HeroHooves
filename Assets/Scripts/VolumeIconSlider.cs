using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeIconSlider : MonoBehaviour
{

    [Header("UI")]
    private Slider slider;
    [SerializeField] private Image[] icons;
    [SerializeField] private Sprite enabledSprite;
    [SerializeField] private Sprite disabledSprite;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);
        OnSliderChanged(slider.value); // init
    }

    private void OnSliderChanged(float value)
    {
        int level = Mathf.RoundToInt(value);

        // Update icons
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].sprite = (i < level) ? enabledSprite : disabledSprite;
        }
    }
}
