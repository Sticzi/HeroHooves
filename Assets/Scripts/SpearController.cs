using UnityEngine;

public class SpearController : MonoBehaviour
{
    [SerializeField] [Range(1f, 20f)] private float knockbackForce = 10f;
    [SerializeField] [Range(1f, 20f)] private float upwardKnockbackForce = 5f;
    public bool isFacingLeft = true;

    private HorseController2D horseController;
    private HorseMovement horseMovement;

    private void Awake()
    {
        horseController = FindObjectOfType<HorseController2D>();
        horseMovement = FindObjectOfType<HorseMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Horse") || collision.CompareTag("Knight"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Knockback(playerRb, collision);
                
            }
        }
    }

    private void Knockback(Rigidbody2D playerRb, Collider2D collision)
    {
        playerRb.velocity = Vector2.zero;
        if (collision.CompareTag("Horse"))
        {
           // collision.GetComponent<HorseController2D>().m_AirControl = false; // the air control is returned when he lands and when he double jumps;
            collision.GetComponent<BetterJump>().isTossed = true;
            collision.GetComponent<HorseController2D>().IsKnockedback = true;
        }

        if (collision.CompareTag("Knight"))
        {
            collision.GetComponent<KnightController2D>().isKnockedback = true;            
        }

        // Calculate knockback direction (upwards)
        //Vector2 knockbackDirection = (collision.transform.position - transform.parent.position).normalized;
        Vector2 totalKnockback = Vector2.left * knockbackForce + Vector2.up * upwardKnockbackForce;
        playerRb.velocity = Vector2.zero;
        if (isFacingLeft)
        {
            collision.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, collision.transform.position.z);
            playerRb.velocity = totalKnockback;
        }
        else
        {
            collision.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, collision.transform.position.z);
            playerRb.velocity = new Vector2(-totalKnockback.x, totalKnockback.y);
        }               
    }    
}


