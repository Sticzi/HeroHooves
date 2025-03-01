using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    private HorseController2D controller;
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Movement Settings")]
    public float runSpeed = 36f;
    public float walkSpeed = 24f;
    [HideInInspector] public float currentSpeed;

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.15f;

    [HideInInspector] public float jumpBufferCounter;
    [HideInInspector] public float hangCounter;

    [Header("Detection Settings")]
    public float Radius;
    [SerializeField] private LayerMask whatIsKnight;
    [SerializeField] private Vector2 offset;

    [Header("Other Settings")]
    [SerializeField] private float lookDownHoldTime = 1f;
    private float timer = 0f;

    [SerializeField] private bool canDrop = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canDoubleJump = true;
    [SerializeField] private bool canPickUp = true;
    public bool canSpawnedKnightSwap = true;
    public bool canSpawnedKnightAttack = true;

    public bool CanSpawnedKnightSwap
    {
        get { return canSpawnedKnightSwap; }
        set { canSpawnedKnightSwap = value;  }
    }

    public bool CanSpawnedKnightAttack
    {
        get { return canSpawnedKnightAttack; }
        set { canSpawnedKnightAttack = value; }
    }

    public bool CanDrop 
    {
        get { return canDrop; }
        set { canDrop = value; } 
    }
    public bool CanJump
    {
        get { return canJump; }
        set { canJump = value; }
    }
    public bool CanDoubleJump
    {
        get { return canDoubleJump; }
        set { canDoubleJump = value; }
    }
    public bool CanPickUp
    {
        get { return canPickUp; }
        set { canPickUp = value; }
    }
    public bool IsHorseControlled { get; set; } = true;

    // Input System
    public PlayerInputActions playerInput; 
    private InputAction move;
    [HideInInspector] public InputAction jump;
    private InputAction lookDown;
    private InputAction pickDropKnight;
    private InputAction swap;

    private void Awake()
    {
        controller = GetComponent<HorseController2D>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed;
        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Konfiguracja akcji
        move = playerInput.HorseActionMap.Move;
        jump = playerInput.HorseActionMap.Jump;
        lookDown = playerInput.HorseActionMap.LookDown;
        pickDropKnight = playerInput.HorseActionMap.PickDropKnight;
        swap = playerInput.KnightActionMap.Swap; 

        move.Enable();
        jump.Enable();
        lookDown.Enable();
        pickDropKnight.Enable();
        swap.Enable();

        // Subskrypcje zdarzeñ
        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        pickDropKnight.performed += OnPickDropPerformed;
        swap.performed += OnSwapPerformed;
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        lookDown.Disable();
        pickDropKnight.Disable();
        swap.Disable();

        // Anulowanie subskrypcji
        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        pickDropKnight.performed -= OnPickDropPerformed;
        swap.performed -= OnSwapPerformed;
    }

    private void Update()
    {
        HandleKnightDetection();
        UpdateAnimations();

        HandleMovement();
        HandleJumpBuffer();

        HandleCoyoteTime();
        HandleLookDown();
        HandleSliding();
    }

    #region Input Handlers
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (IsHorseControlled&& canJump)
        {
            jumpBufferCounter = jumpBufferTime;
            controller.Jump(true);
        }
        
    }

    public void DropSwap()
    {
        HandlePickDropKnight();
        controller.spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("horse");
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        if (IsHorseControlled&&controller.spawnedKnight == null)
        {
            HandlePickDropKnight();
            controller.spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("horse");
        }
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        controller.isJumpButtonHeld = false;
    }

    private void OnPickDropPerformed(InputAction.CallbackContext context)
    {
        HandlePickDropKnight();
    }
    #endregion

    #region Core Logic
    private void HandleMovement()
    {
        if (!IsHorseControlled) return;

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
        float horizontalMove = moveInput * currentSpeed;
        controller.Move(horizontalMove * Time.fixedDeltaTime);
    }

    private void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleCoyoteTime()
    {
        if (controller.IsGrounded)
        {
            hangCounter = coyoteTime;
        }
        else
        {
            hangCounter -= Time.deltaTime;
        }
    }
    #endregion

    #region Additional Systems
    private void HandleKnightDetection()
    {
        Collider2D collider = Physics2D.OverlapCircle(
            new Vector2(transform.position.x - offset.x, transform.position.y - offset.y),
            Radius,
            whatIsKnight
        );

        if (collider != null) controller.InRangeOfKnight();
        else controller.OutOfRangeOfKnight();
    }

    private void HandleLookDown()
    {
        if (!IsHorseControlled) return;

        float lookDownInput = lookDown.ReadValue<float>();
        if (lookDownInput < 0)
        {
            timer += Time.deltaTime;
            if (timer >= lookDownHoldTime && !controller.isLookingDown)
            {
                controller.isLookingDown = true;
            }
        }
        else
        {
            timer = 0f;
            if (controller.isLookingDown)
            {
                controller.isLookingDown = false;
                controller.StopLookingDown();
            }
        }
    }

    private void HandlePickDropKnight()
    {
        if (controller.KnightPickedUp&&CanDrop)
        {
            controller.KnightDropOfF();
        }
        else if (controller.IsInRangeOfKnight&&CanPickUp)
        {
            controller.KnightPickUp();
        }
    }
    #endregion

    #region Utilities
    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("Velocity.y", rb.velocity.y);
    }

    private void HandleSliding()
    {
        if (!IsHorseControlled && controller.IsGrounded &&
            rb.velocity.x != 0 && !controller.isKnockedback)
        {
            rb.velocity = new Vector2(rb.velocity.x / 10, rb.velocity.y);
        }
    }

    public void OnLanding()
    {
        controller.doubleJumpReady = true;
        controller.canJump = true;
        //if (controller.IsGrounded && rb.velocity.y <= 0f)
        //{
            
            controller.isKnockedback = false;
        GetComponent<BetterJump>().isTossed = false;
            animator.SetBool("IsJumping", false);
            controller.m_AirControl = true;
        //}
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            new Vector2(transform.position.x - offset.x, transform.position.y - offset.y),
            Radius
        );
    }
    #endregion
}