using UnityEngine;
using LDtkUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PressurePlate : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private ContactFilter2D _contactFilter;
    [SerializeField] private float _checkInterval = 0.1f;

    [Header("Platform Control")]
    [SerializeField] private LaunchingPlatform _targetPlatform;

    [Header("Visuals")]
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private Sprite deactivatedSprite;

    private Rigidbody2D _rb;
    private bool _isPressed;
    private float _lastCheckTime;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        InitializePlatformReference();
        // Set initial sprite
        if (_spriteRenderer != null && deactivatedSprite != null)
            _spriteRenderer.sprite = deactivatedSprite;
    }

    private void InitializePlatformReference()
    {
        if (_targetPlatform != null) return;

        _targetPlatform = GetComponent<LDtkFields>().GetEntityReference("LaunchingPlatform").GetEntity().GetComponentInChildren<LaunchingPlatform>();
    }

    private void FixedUpdate()
    {
        if (Time.time - _lastCheckTime < _checkInterval) return;

        _lastCheckTime = Time.time;
        bool currentState = _rb.IsTouching(_contactFilter);

        if (currentState != _isPressed)
        {
            _isPressed = currentState;
            UpdatePlatformState();
            UpdateVisualsAndSound();
        }
    }

    private void UpdatePlatformState()
    {
        if (_targetPlatform == null)
        {
            Debug.LogWarning("No platform assigned to pressure plate!", this);
            return;
        }

        _targetPlatform.isActive = !_targetPlatform.isActive;
    }

    private void UpdateVisualsAndSound()
    {
        // Play sound
        var audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.Play(_isPressed ? "buttonOn" : "buttonOff");
        }

        // Change sprite
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = _isPressed && activatedSprite != null ? activatedSprite : deactivatedSprite;
        }
    }

    private void OnDrawGizmos()
    {
        if (_targetPlatform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _targetPlatform.transform.position);
        }
    }
}