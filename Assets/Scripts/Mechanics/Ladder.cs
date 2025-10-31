using UnityEngine;

public class Ladder : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;   
    public PlatformEffector2D platformEffector;

    [SerializeField] private float ladderDetectRadius;
    [SerializeField] private LayerMask whatIsLadder;

    private void Start()
    {
        Collider2D ladderCollider = Physics2D.OverlapCircle(new Vector2(transform.position.x, (transform.position.y +1)), ladderDetectRadius, whatIsLadder);
        if(!ladderCollider)
        {
            boxCollider2D.isTrigger = false;
            boxCollider2D.usedByEffector = true;
            platformEffector.enabled = true;

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(new Vector2(transform.position.x, (transform.position.y + 1)), ladderDetectRadius);
    }   
}
