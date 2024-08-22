using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class pressurePlater : MonoBehaviour
{

    public ContactFilter2D ContactFilterPlayer;
    public Rigidbody2D rb;
    public bool IsPressed => rb.IsTouching(ContactFilterPlayer);
    private MovePlatform movingPlatform;
    public SpriteRenderer spriteRenderer;
    public Sprite clickedPlate;
    public Sprite defaultPlate;
    public AudioSource clickSound;
    public AudioSource clickOffSound;

    private bool wasPressed = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movingPlatform = GetComponent<LDtkFields>().GetEntityReference("MovingPlatform").GetEntity().gameObject.transform.GetChild(0).GetComponent<MovePlatform>();        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(IsPressed)
        {
            Click();
        }
        else 
        {
            if (wasPressed)
                ClickOff();
        }
    }

    public void Click()
    {
        movingPlatform.moving = true;
        spriteRenderer.sprite = clickedPlate;
        if(wasPressed == false)
        {
            clickSound.Play();
        }        
        wasPressed = true;

    }

    public void ClickOff()
    {
        movingPlatform.moving = false;
        spriteRenderer.sprite = defaultPlate;
        if(wasPressed == true)
        {
            clickOffSound.Play();
        }        
        wasPressed = false;
    }
}
