using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightDeath : MonoBehaviour
{
    public Respawn resp;

    private void Start()
    {
        resp = GameObject.FindGameObjectWithTag("Horse").GetComponent<Respawn>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("KillBox"))
        {
            Die();
            //animacja i tak dalej
        }
    }
    public void Die()
    {
        resp.Death();
    }
}
