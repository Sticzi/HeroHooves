using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LDtkUnity;


public class BoxFall : MonoBehaviour
{

    public bool wasGrounded;
    //bool wasMoved;
    public ContactFilter2D contactFilter;
    [SerializeField] private ContactFilter2D knightHitContactFilter;
    [SerializeField] private ContactFilter2D horseHitContactFilter;
    public bool isGrounded => rb.IsTouching(contactFilter);
    public bool isOnKnight => rb.IsTouching(knightHitContactFilter);
    public bool isOnHorse => rb.IsTouching(horseHitContactFilter);
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
    private void OnCollisionEnter2D(Collision2D collision)
    {        
        if (isOnKnight)
        {
            collision.transform.GetComponentInChildren<KnightDeath>().Die();
            Debug.Log("TYLE MA NA GŁOWIE OHOHOHOHO");
        }
        if (isOnHorse)
        {
            collision.transform.GetComponentInChildren<HorseDeath>().Die();
            Debug.Log("TYLE MA NA GŁOWIE OHOHOHOHO");
        }
    }


    //GetComponent<LDtkFields>().GetBool("Active");
    void FixedUpdate()
    {

        //if (rb.velocity.x > 0)
        //{
        //    wasMoved = true;
        //}

        if (wasGrounded == false && isGrounded == true)
        {
            if(audioEnabled)
            FindObjectOfType<AudioManager>().Play("Hit");
                        
        }
        wasGrounded = isGrounded;

        //if (!isGrounded && !GetComponent<LDtkFields>().GetBool("canMoveInAir"))
        if (!isGrounded)
        {


            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }    

    
}
