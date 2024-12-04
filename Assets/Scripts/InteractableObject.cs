using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{           
    [SerializeField] private ContactFilter2D ContactFilter;
    [SerializeField] private Rigidbody2D rb;
    public bool IsPlayerInsideTriggerCollider => rb.IsTouching(ContactFilter);
    public UnityEvent OnProximityEnable;
    public UnityEvent OnProximityDisable;
    public UnityEvent OnActivate;


    void Update()
    {       
        // If the player is close enough, activate the object
        if (IsPlayerInsideTriggerCollider)
        {
            OnProximityEnable.Invoke();
        }
        else
        {
            OnProximityDisable.Invoke();
        }
    }

    public void Activate()
    {
        OnActivate.Invoke();
    }
}

