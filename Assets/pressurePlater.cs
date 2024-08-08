using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class pressurePlater : MonoBehaviour
{

    public ContactFilter2D ContactFilterPlayer;
    public Rigidbody2D rb;
    public bool isPressed => rb.IsTouching(ContactFilterPlayer);
    private MovePlatform movingPlatform;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movingPlatform = GetComponent<LDtkFields>().GetEntityReference("MovingPlatform").GetEntity().gameObject.transform.GetChild(0).GetComponent<MovePlatform>();        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isPressed)
        {
            movingPlatform.moving = true;
        }
        else
        {
            movingPlatform.moving = false;
        }
    }
}
