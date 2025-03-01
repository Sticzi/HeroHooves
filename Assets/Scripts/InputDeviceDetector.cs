using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class InputDeviceDetector : MonoBehaviour
{
    public static InputDeviceDetector Instance { get; private set; }

    public enum InputDeviceType
    {
        KeyboardMouse,
        XboxGamepad,
        PlayStationGamepad
    }

    public event System.Action<InputDeviceType> DeviceChanged; // Zmieniona nazwa eventu
    public InputDeviceType CurrentDevice { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private Sprite keyboardSprite;
    [SerializeField] private Sprite xboxSprite;
    [SerializeField] private Sprite playstationSprite;
    [SerializeField] private Image actionIcon;

    private PlayerInputActions _inputActions;
    public InputDevice _currentDevice;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        _inputActions.KnightActionMap.Attack.performed += OnActionPerformed;
        _inputActions.HorseActionMap.Move.performed += OnActionPerformed;
        _inputActions.UI.Submit.performed += OnActionPerformed;

        InputSystem.onDeviceChange += HandleDeviceChange; // Zmieniona nazwa metody
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
            _inputActions.Disable();
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        UpdateCurrentDevice(ctx.control.device);
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change) // Zmieniona nazwa metody
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            UpdateCurrentDevice(device);
        }
    }

    private void UpdateCurrentDevice(InputDevice device)
    {
        if (_currentDevice == device) return;
        _currentDevice = device;

        InputDeviceType newDeviceType;

        if (device is Keyboard || device is Mouse)
        {
            actionIcon.sprite = keyboardSprite;
            newDeviceType = InputDeviceType.KeyboardMouse;
        }
        else if (device is Gamepad)
        {
            if (device.displayName.Contains("DualShock") || device.displayName.Contains("DualSense"))
            {
                actionIcon.sprite = playstationSprite;
                newDeviceType = InputDeviceType.PlayStationGamepad;
            }
            else
            {
                actionIcon.sprite = xboxSprite;
                newDeviceType = InputDeviceType.XboxGamepad;
            }
        }
        else
        {
            return;
        }

        CurrentDevice = newDeviceType;
        DeviceChanged?.Invoke(newDeviceType); // Wywo³anie eventu

        ShowAndFadeIcon();
    }

    private void ShowAndFadeIcon()
    {
        actionIcon.DOFade(1f, 0.3f)
            .OnComplete(() => actionIcon.DOFade(0f, 1.5f));
    }
}
