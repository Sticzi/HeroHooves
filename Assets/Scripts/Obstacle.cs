using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;
public class Obstacle : MonoBehaviour
{
    public Animator anim;
    public bool facingRight = false;
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
        facingRight = GetComponent<LDtkFields>().GetBool("facingRight");
        if (facingRight)
        {
            GetComponent<Transform>().localScale = new Vector2(-1, 1);
            transform.GetChild(0).GetComponentInChildren<SpearController>().isFacingLeft = false;
        }
    }
    public void Destroyed()
    {
        anim.SetTrigger("Death");
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;

        Destroy(this.gameObject, 1.5f);
    }
}

