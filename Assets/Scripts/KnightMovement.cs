using UnityEngine;
using UnityEngine.InputSystem;

public class KnightMovement : MonoBehaviour
{
    private KnightController2D controller;
    private Rigidbody2D rb;

    public GameObject climbedOnLadder;

    public bool isKnightControlled;
    public bool isAttacking;
    public bool CanSwap;

    public string whoIsControlled = "horse";
    public float runSpeed;

    [SerializeField] private float currentSpeed;
    [SerializeField] private LayerMask whatIsHorse;
    [SerializeField] private LayerMask whatIsLadder;
    [SerializeField] private float verticalLadderDetectionOffset;
    [SerializeField] private float opossiteVerticalLadderDetectionOffset;
    [SerializeField] private float ladderDetectRadius;
    [SerializeField] private float callCooldownLadder = 1f;
    [SerializeField] private float climbDistance;

    private float lastCallTime;
    private Vector2 moveInput;

    // New Input System
    public KnightInputActions knightInput;
    private InputAction move;
    private InputAction attack;
    private InputAction swap;

    public void Awake()
    {
        currentSpeed = runSpeed;
        controller = GetComponent<KnightController2D>();
        rb = GetComponent<Rigidbody2D>();

        knightInput = new KnightInputActions();
    }

    private void OnEnable()
    {
        move = knightInput.Player.Move;
        attack = knightInput.Player.Attack;
        swap = knightInput.Player.Swap;

        move.Enable();
        attack.Enable();
        swap.Enable();

        attack.performed += OnAttackPerformed;
        swap.performed += OnSwapPerformed;
    }

    private void OnDisable()
    {
        move.Disable();
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
    }

    #region Input Handlers
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (isKnightControlled && !isAttacking)
            controller.Attack();
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        controller.SwapCharacter(whoIsControlled);
    }
    #endregion

    #region Movement Logic
    private void HandleMovement()
    {
        moveInput = move.ReadValue<Vector2>();
        float horizontalMove = moveInput.x * currentSpeed;

        if (!climbedOnLadder)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime);
        }
    }

    private void HandleLadderClimbing()
    {
        float verticalDirection = moveInput.y;
        if (verticalDirection != 0)
        {
            DetectLadder(verticalDirection);
        }
    }
    #endregion

    #region Ladder System
    public void DetectLadder(float direction)
    {
        Collider2D ladderCollider = Physics2D.OverlapCircle(
            new Vector2(
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