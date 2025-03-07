using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterJump : MonoBehaviour
{

    public float fallMultiplier = 3f;
    public float lowJumpMultiplier = 2.5f;

    private float minimalJumpCounter = 0f;
    public float minimalJumpTime;
    public bool jump;
    public bool isTossed;
    
    Rigidbody2D rb;
    //public float velocityCut;
    private HorseMovement horseMovement; // Dodaj referencję do HorseMovement

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        horseMovement = GetComponent<HorseMovement>(); // Pobierz komponent
    }

    void FixedUpdate()
    {        
        if(jump)
        {
            minimalJumpCounter = minimalJumpTime;
            jump = false;
        }

        else if(minimalJumpCounter > 0)
        {
            minimalJumpCounter -= Time.deltaTime;
        }


        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;            
        }

        if(horseMovement == null && rb.velocity.y > 0 && !isTossed)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        else if(horseMovement!= null && rb.velocity.y > 0 && !horseMovement.jump.IsPressed() && minimalJumpCounter <= 0 && !isTossed)
        {            
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //if(Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        //{
        //    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * velocityCut);
        //}
    }
}