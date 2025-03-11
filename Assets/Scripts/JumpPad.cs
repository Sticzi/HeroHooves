using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class JumpPad : MonoBehaviour
{
    public float bounce;

    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
        bounce = GetComponent<LDtkFields>().GetFloat("jumpPower");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Horse") || collision.gameObject.CompareTag("Knight"))
        {
            if (collision.gameObject.CompareTag("Horse"))
            {
                collision.GetComponent<HorseMovement>().OnLanding();
            }
            collision.GetComponent<BetterJump>().isTossed = true;

            anim.SetTrigger("jump");

            FindObjectOfType<AudioManager>().Play("SpringJump");

            //collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; tak by�o przedtem i dzia�a�o git
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.gameObject.GetComponent<Rigidbody2D>().velocity.x, 0);

            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounce, ForceMode2D.Impulse);

        }
    }
}
