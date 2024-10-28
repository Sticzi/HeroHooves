using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoxFall : MonoBehaviour
{

    public bool wasGrounded;
    //bool wasMoved;
    public ContactFilter2D ContactFilter;
    public bool IsGrounded => rb.IsTouching(ContactFilter);
    private Rigidbody2D rb;
    private bool audioEnabled = false;
    public float delay = 1f;       

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wasGrounded = true;
       // wasMoved = false;
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        audioEnabled = true;
        
    }

    void FixedUpdate()
    {

        //if (rb.velocity.x > 0)
        //{
        //    wasMoved = true;
        //}

        if (wasGrounded == false && IsGrounded == true)
        {
            if(audioEnabled)
            FindObjectOfType<AudioManager>().Play("Hit");
                        
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
