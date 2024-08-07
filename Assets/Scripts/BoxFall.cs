using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoxFall : MonoBehaviour
{

    bool wasGrounded;
    bool wasMoved;
    public ContactFilter2D ContactFilter;
    public bool IsGrounded => rb.IsTouching(ContactFilter);
    private Rigidbody2D rb;

    public AudioSource fallSound;
    public float delay = 1f;

    //[SerializeField] private AudioSource LandSound;    
    [SerializeField] private UnityEvent OnLandingEvent;
    
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (OnLandingEvent == null)
            OnLandingEvent = new UnityEvent();
        wasGrounded = true;
        wasMoved = false;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        fallSound.enabled = true;
    }

    void FixedUpdate()
    {

        //if (rb.velocity.x > 0)
        //{
        //    wasMoved = true;
        //}

        if (wasGrounded == false && IsGrounded == true)
        {
            OnLandingEvent.Invoke();            
        }
        wasGrounded = IsGrounded;

        if (!IsGrounded)
        {
            
            
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }    

    
}
