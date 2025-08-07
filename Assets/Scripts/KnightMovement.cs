using UnityEngine;
using UnityEngine.InputSystem;

public class KnightMovement : MonoBehaviour
{
    private KnightController2D controller;
    private Rigidbody2D rb;

    public GameObject climbedOnLadder;

    public bool isKnightControlled;
    public bool isAttacking;

    [SerializeField] private bool canSwap;
    [SerializeField] private bool canAttack;

    public bool CanSwap // Property używa pola
    {
        get { return canSwap; }
        set { canSwap = value; }
    }
    public bool CanAttack // Property używa pola
    {
        get { return canAttack; }
        set { canAttack = value; }
    }

    public string whoIsControlled = "horse";

    [SerializeField] private LayerMask whatIsHorse;
    [SerializeField] private LayerMask whatIsLadder;
    [SerializeField] private float verticalLadderDetectionOffset;
    [SerializeField] private float opossiteVerticalLadderDetectionOffset;
    [SerializeField] private float ladderDetectRadius;
    [SerializeField] private float callCooldownLadder = 1f;
    [SerializeField] private float climbDistance;

    private float lastCallTime;

    // New Input System
    public PlayerInputActions knightInput;
    private InputAction move;
    private InputAction moveY;
    private InputAction attack;
    private InputAction swap;

    public void Awake()
    {
        controller = GetComponent<KnightController2D>();
        rb = GetComponent<Rigidbody2D>();

        knightInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        move = knightInput.KnightActionMap.Move;
        moveY = knightInput.KnightActionMap.MoveY;
        attack = knightInput.KnightActionMap.Attack;
        swap = knightInput.KnightActionMap.Swap;

        move.Enable();
        moveY.Enable();
        attack.Enable();
        swap.Enable();

        attack.performed += OnAttackPerformed;
        swap.performed += OnSwapPerformed;
    }

    private void OnDisable()
    {
        move.Disable();
        moveY.Disable();
        attack.Disable();
        swap.Disable();

        attack.performed -= OnAttackPerformed;
        swap.performed -= OnSwapPerformed;
    }

    private void Update()
    {
        if (isKnightControlled && !isAttacking)
        {
            HandleMovement();
            HandleLadderClimbing();
        }
        else
        {
            controller.Move(0);
            return;
        }
    }

    #region Input Handlers
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (isKnightControlled && !isAttacking && canAttack)
            controller.Attack();
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        if(canSwap)
        {
            controller.SwapCharacter(whoIsControlled);
        }
        
    }
    #endregion

    #region Movement Logic
    private void HandleMovement()
    {
        float moveInput = move.ReadValue<float>();

        // Force input to either -1, 0, or 1 based on threshold
        // DEADZONE
        if (Mathf.Abs(moveInput) < 0.2f)
        {
            moveInput = 0f;  // No movement if the input is too small
        }
        else
        {
            moveInput = Mathf.Sign(moveInput);  // Use sign to get -1 or 1
        }

        if (climbedOnLadder == null)
        {
            float horizontalMove = moveInput;
            controller.Move(horizontalMove * Time.fixedDeltaTime);
        }           
    }

    private void HandleLadderClimbing()
    {
        float moveInput = moveY.ReadValue<float>();

        if (Mathf.Abs(moveInput) < 0.4f)
        {
            moveInput = 0f;  // No movement if the input is too small
        }
        else
        {
            moveInput = Mathf.Sign(moveInput);  // Use sign to get -1 or 1
        }

        if (moveInput != 0)
        {
            DetectLadder(moveInput);
        }
    }
    #endregion

    #region Ladder System
    public void DetectLadder(float direction)
    {
        Collider2D ladderCollider = Physics2D.OverlapCircle(
            new Vector2
            (
                transform.position.x,
                transform.position.y - verticalLadderDetectionOffset + direction * opossiteVerticalLadderDetectionOffset
            ),
            ladderDetectRadius,
            whatIsLadder
        );

        if (Time.time - lastCallTime >= callCooldownLadder)
        {
            if (ladderCollider != null)
            {
                if (climbedOnLadder)
                {
                    climbedOnLadder = ladderCollider.gameObject;
                    controller.ClimbLadder(climbDistance * direction);
                }
                else
                {
                    StartClimbingLadder(ladderCollider.gameObject);
                }
                lastCallTime = Time.time;
            }
            else if (climbedOnLadder)
            {
                StopClimbingLadder(direction);
                lastCallTime = Time.time;
            }
        }
    }

    public void StartClimbingLadder(GameObject ladder)
    {
        climbedOnLadder = ladder;
        controller.anim.SetTrigger("Climb");
        rb.bodyType = RigidbodyType2D.Static;

        Collider2D ladderCollider = climbedOnLadder.GetComponent<Collider2D>();
        if (ladderCollider != null)
        {
            transform.position = new Vector3(
                ladderCollider.bounds.center.x,
                transform.position.y,
                transform.position.z
            );
        }
    }

    public void StopClimbingLadder(float direction)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        controller.anim.SetTrigger("ClimbOff");

        Collider2D ladderCollider = climbedOnLadder.GetComponent<Collider2D>();
        if (ladderCollider != null)
        {
            float yPosition = direction > 0
                ? ladderCollider.bounds.max.y + 1
                : ladderCollider.bounds.min.y + 1;

            transform.position = new Vector3(
                ladderCollider.bounds.center.x,
                yPosition,
                transform.position.z
            );
        }

        climbedOnLadder = null;
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            new Vector2(
                transform.position.x,
                transform.position.y - verticalLadderDetectionOffset
            ),
            ladderDetectRadius
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            new Vector2(
                transform.position.x,
                transform.position.y - verticalLadderDetectionOffset + opossiteVerticalLadderDetectionOffset
            ),
            ladderDetectRadius
        );

        Gizmos.DrawWireSphere(
            new Vector2(
                transform.position.x,
                transform.position.y - verticalLadderDetectionOffset - opossiteVerticalLadderDetectionOffset
            ),
            ladderDetectRadius
        );
    }
}