using UnityEngine;
using LDtkUnity;

[RequireComponent(typeof(Rigidbody2D))]
public class PressurePlate : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private ContactFilter2D _contactFilter;
    [SerializeField] private float _checkInterval = 0.1f;

    [Header("Platform Control")]
    [SerializeField] private MovePlatform _targetPlatform;

    private Rigidbody2D _rb;
    private bool _isPressed;
    private float _lastCheckTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        InitializePlatformReference();
    }

    private void InitializePlatformReference()
    {
        if (_targetPlatform != null) return;

        var entityRef = GetComponent<LDtkFields>().GetEntityReference("MovingPlatform");
        if (entityRef != null)
        {
            _targetPlatform = entityRef.GetEntity().GetComponentInChildren<MovePlatform>();
        }
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

        _targetPlatform.ToggleMovement(_isPressed);
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