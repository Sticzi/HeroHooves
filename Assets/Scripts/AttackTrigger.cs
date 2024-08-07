using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public Animator anim;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Horse"))
        {
            if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("goblin"))
            {
                anim.SetTrigger("Attack");
            }
        }
        if (collision.gameObject.CompareTag("Knight"))
        {
            if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("goblin"))
            {
                anim.SetTrigger("Attack");
            }
        }
    }
}
