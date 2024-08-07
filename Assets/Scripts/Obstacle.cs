using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerWeaponCollider"))
        {
            //kill/destroy()
            Destroy(this.gameObject);
        }
    }
}
