using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float bounce = 20f;

    private Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
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

            //collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero; tak by³o przedtem i dzia³a³o git
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.gameObject.GetComponent<Rigidbody2D>().velocity.x, 0);

            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounce, ForceMode2D.Impulse);

        }
    }
}
