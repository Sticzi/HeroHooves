using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject objectToActivate; // Reference to the object to activate        
    [SerializeField] private ContactFilter2D ContactFilter;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator cloudTextAnimator;
    public bool IsPlayerInsideTriggerCollider => rb.IsTouching(ContactFilter);

    void Update()
    {       
        // If the player is close enough, activate the object
        if (IsPlayerInsideTriggerCollider)
        {
            Appear();
        }
        else
        {
            Disappear();
        }
    }

    public void Appear()
    {
        cloudTextAnimator.SetBool("IsHidden", false);
    }

    public void Disappear()
    {
        cloudTextAnimator.SetBool("IsHidden", true);
    }

}

