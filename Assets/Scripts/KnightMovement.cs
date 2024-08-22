using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightMovement : MonoBehaviour
{
    private KnightController2D controller;
    private Rigidbody2D rb;

    public GameObject climbedOnLadder;
    
    public bool isKnightControlled;
    public bool isAttacking;


    public string whoIsControlled = "horse";

    public float runSpeed;
    float horizontalMove = 10f;    

    [SerializeField] private float currentSpeed;
    [SerializeField] private LayerMask whatIsHorse;
    [SerializeField] private LayerMask whatIsLadder;
    [SerializeField] private float verticalLadderDetectionOffset;
    [SerializeField] private float opossiteVerticalLadderDetectionOffset;
    [SerializeField] private float ladderDetectRadius;
    [SerializeField] private float callCooldownLadder = 1f;
    [SerializeField] private float climbDistance;

    private float lastCallTime;

    

    public void Awake()
    {
        currentSpeed = runSpeed;
        controller = GetComponent<KnightController2D>();
        rb = GetComponent<Rigidbody2D>();
    }


    public void DetectLadder(float direction)
    {
        Collider2D ladderCollider = Physics2D.OverlapCircle(new Vector2(transform.position.x, (transform.position.y - verticalLadderDetectionOffset)+direction*opossiteVerticalLadderDetectionOffset), ladderDetectRadius, whatIsLadder);
        if(Time.time - lastCallTime >= callCooldownLadder)
        {
            if (ladderCollider != null)
            {
                

                if (climbedOnLadder)
                {
                    climbedOnLadder = ladderCollider.gameObject;
                    controller.ClimbLadder(climbDistance * direction);
                    lastCallTime = Time.time;
                }

                if (!climbedOnLadder)
                {
                    StartClimbingLadder(ladderCollider.gameObject);
                    lastCallTime = Time.time;
                }
            }
            else
            {
                if (climbedOnLadder)
                {
                    StopClimbingLadder(direction);
                    lastCallTime = Time.time;
                }
            }

        }
    }

    public void Update()
    {                    
        if (isKnightControlled&&!isAttacking)
        {
            if(!climbedOnLadder)
            {
                //usuń Raw żeby uzyskać płynny ruch
                horizontalMove = Input.GetAxisRaw("Horizontal") * currentSpeed;
                controller.Move(horizontalMove * Time.fixedDeltaTime);

                if (Input.GetButtonDown("Jump"))
                {
                    controller.Attack();
                }
            }

            float verticalLadderDirection = Input.GetAxisRaw("Vertical");            
            if(verticalLadderDirection != 0)
            {
                DetectLadder(verticalLadderDirection);
            }
                      
        }

        if (Input.GetButtonDown("swap"))
        {
            controller.SwapCharacter(whoIsControlled);
        }

    }

    public void StartClimbingLadder(GameObject ladder)
    {
        climbedOnLadder = ladder;
        controller.anim.SetTrigger("Climb");
        //controller.anim.SetTrigger("StartClimbing");

        rb.bodyType = RigidbodyType2D.Static;

        Collider2D ladderCollider = climbedOnLadder.GetComponent<Collider2D>();

        if (ladderCollider != null)
        {
            // Get the center of the bounds of the composite collider
            Vector3 centerOfCollider2D = ladderCollider.bounds.center;

            // Move the object to the center of the composite collider while keeping the y and z positions the same
            transform.position = new Vector3(centerOfCollider2D.x, transform.position.y, transform.position.z);
        }
    }

    public void StopClimbingLadder(float direction)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        controller.anim.SetTrigger("ClimbOff");

        Collider2D ladderCollider = climbedOnLadder.GetComponent<Collider2D>();

        if (ladderCollider != null)
        {
            if (direction > 0)
            {

                transform.position = new Vector3(ladderCollider.bounds.center.x, ladderCollider.bounds.max.y + 1, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(ladderCollider.bounds.center.x, ladderCollider.bounds.min.y + 1, transform.position.z);
            }
        }

            //CompositeCollider2D compositeCollider = climbedOnLadder.GetComponent<CompositeCollider2D>();

            //if (compositeCollider != null)
            //{            
            //    if(direction > 0)
            //    {

            //        transform.position = new Vector3(compositeCollider.bounds.center.x, compositeCollider.bounds.max.y + 1, transform.position.z);
            //    }
            //    else
            //    {
            //        transform.position = new Vector3(compositeCollider.bounds.center.x, compositeCollider.bounds.min.y+1, transform.position.z);
            //    }


            //}

            climbedOnLadder = null;
    }

    private void OnDrawGizmos()
    {                        
        Gizmos.DrawWireSphere(new Vector2(transform.position.x, (transform.position.y - verticalLadderDetectionOffset)), ladderDetectRadius);
        Gizmos.DrawWireSphere(new Vector2(transform.position.x, (transform.position.y - verticalLadderDetectionOffset)+1*opossiteVerticalLadderDetectionOffset), ladderDetectRadius);
        Gizmos.DrawWireSphere(new Vector2(transform.position.x, (transform.position.y - verticalLadderDetectionOffset)-1*opossiteVerticalLadderDetectionOffset), ladderDetectRadius);
    }
}


