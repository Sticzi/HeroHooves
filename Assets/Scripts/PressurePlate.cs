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

    private Rigidbody2D _rb;
    private bool _isPressed;
    private float _lastCheckTime;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        InitializePlatformReference();
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
        }
    }

    private void UpdatePlatformState()
    {
        if (_targetPlatform == null)
        {
            Debug.LogWarning("No platform assigned to pressure plate!", this);
            return;
        }

        if (_targetPlatform.isActive)
        {
            _targetPlatform.isActive = false;
        }
        else
        {
            _targetPlatform.isActive = true;
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