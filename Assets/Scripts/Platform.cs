using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private float playerPos;
    [SerializeField] private float SurfacePos;
    public float timeOffset;
    private Animator anim;
    private Collider2D collider2d;


    public bool fallingPlatform;

    public void Start()
    {
        collider2d = GetComponent<Collider2D>();
        if(fallingPlatform)
        {
            anim = GetComponent<Animator>();
        }
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        playerPos = collision.gameObject.GetComponent<Collider2D>().bounds.center.y - collision.gameObject.GetComponent<Collider2D>().bounds.extents.y;
        //SurfacePos = collider2d.bounds.center.y;        

        if (collision.gameObject.layer == 9 && playerPos > SurfacePos)
        {
            gameObject.layer = 8;

            if(fallingPlatform)
            {
                PlatformFall();
            }
        }
    }

    private void PlatformFall()
    {
        anim.SetTrigger("Fall");        
    }    

    private void OnCollisionExit2D(Collision2D collision)
    { 
        if (collision.gameObject.layer == 9)
        {
            gameObject.layer = 14;
        }
    }
}
 