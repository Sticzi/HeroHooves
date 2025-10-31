using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Secret : MonoBehaviour
{
    public Animator anim;
    public bool facingRight = false;
    public UnityEvent onReveal;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerWeaponCollider"))
        {
            //kill/destroy()
            Destroyed();

        }
    }

    public void Start()
    {
        //facingRight = GetComponent<LDtkFields>().GetBool("facingRight");
        if (facingRight)
        {
            GetComponent<Transform>().localScale = new Vector2(-1, 1);
        }
    }
    public void Destroyed()
    {
        onReveal.Invoke();
        anim.SetTrigger("destroy");
        GetComponent<BoxCollider2D>().enabled = false;
        FindObjectOfType<AudioManager>().Play("FallingPlatform");
        Destroy(this.gameObject, 2);
    }
}

