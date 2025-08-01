using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public Animator anim;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Horse") || collision.CompareTag("Knight") && anim.GetCurrentAnimatorStateInfo(0).IsName("goblin"))
        {
            Attack();
        }
    }


    private void Attack()
    {
        anim.SetTrigger("Attack");
    }
}

