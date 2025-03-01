using UnityEngine;
using UnityEngine.UI;
using static InputDeviceDetector;
using UnityEngine.InputSystem;

public class DeviceVibeChecker : MonoBehaviour
{   
    [SerializeField] private Sprite keyboardSprite;

    [SerializeField] private Sprite playstationSprite;
    [SerializeField] private Sprite xboxSprite;

    private Image actionIcon;

    private void OnEnable()
    {
        actionIcon = gameObject.GetComponent<Image>();
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.DeviceChanged += UpdateIcon;
            UpdateIcon(InputDeviceDetector.Instance.CurrentDevice);
        }
    }

    private void OnDisable()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.DeviceChanged -= UpdateIcon;
        }
    }

    private void UpdateIcon(InputDeviceType deviceType)
    {
        switch (deviceType)
        {
            case InputDeviceType.KeyboardMouse:
                actionIcon.sprite = keyboardSprite;
                break;
            case InputDeviceType.XboxGamepad:
                actionIcon.sprite = xboxSprite;
                break;
            case InputDeviceType.PlayStationGamepad:
                actionIcon.sprite = playstationSprite;
                break;
        }
    }

    private void UpdateIconSimple()
    {
        switch (FindObjectOfType<InputDeviceDetector>()._currentDevice.displayName)
        {
            case string s when s.Contains("Keyboard"):
                //actionIcon.sprite = keyboardSprite;
                break;
            case string s when s.Contains("Controller"):
                //actionIcon.sprite = xboxSprite;
                break;
        }
    }
}
