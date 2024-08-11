using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class Lever : MonoBehaviour
{    
    
    public SpriteRenderer leverRenderer;
    public Sprite leverLeftSprite;
    public Sprite leverRightSprite;
    public bool isLeverRight;
    

    private MovePlatform movingPlatform;
    void Start()
    {        
        movingPlatform = GetComponent<LDtkFields>().GetEntityReference("MovingPlatform").GetEntity().gameObject.transform.GetChild(0).GetComponent<MovePlatform>();
    }

    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerWeaponCollider"))
        {
            PushLever();
        }
    }

    public void PushLever()
    {
        if(isLeverRight)
        {
            movingPlatform.moving = false;
            leverRenderer.sprite = leverLeftSprite;
            isLeverRight = false;
        }
        else
        {
            isLeverRight = true;
            movingPlatform.moving = true;
            leverRenderer.sprite = leverRightSprite;
        }
        

    }    
}
