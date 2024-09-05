using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Animator anim;
    public AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerWeaponCollider"))
        {
            //kill/destroy()
            Destroyed();
            
        }
    }

    public void Destroyed()
    {
        audioSource.time = 0.4f;
        anim.SetTrigger("Death");

        audioSource.pitch = Random.Range(1, 1.3f);       
        audioSource.Play();
        
        Destroy(this.gameObject,1.5f);
    }
}
