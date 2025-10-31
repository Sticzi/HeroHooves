using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    private HorseController2D controller;
    private Animator animator;
    private Rigidbody2D rb;    

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.15f;
    [SerializeField] private GameObject LandingCloud;

    [HideInInspector] public float jumpBufferCounter;
    public float hangCounter;

    [Header("Detection Settings")]
    public float Radius;
    [SerializeField] private LayerMask whatIsKnight;
    [SerializeField] private Vector2 offset;

    [Header("Other Settings")]
    [SerializeField] private float lookHoldTime = 1f;
    // replaced single-purpose timer with a look-specific timer and state
    private float lookTimer = 0f;
    private int currentLookDir = 0; // -1 down, 0 neutral, 1 up
    private bool isLooking = false;

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
    public bool IsHorsePaused { get; set; } = false;

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
        playerInput = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Konfiguracja akcji
        move = playerInput.HorseActionMap.Move;
        jump = playerInput.HorseActionMap.Jump;
        lookDown = playerInput.HorseActionMap.Look;
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
        HandleLook();
        HandleSliding();
    }

    #region Input Handlers
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (IsHorseControlled&& canJump && !IsHorsePaused)
        {
            jumpBufferCounter = jumpBufferTime;
            controller.Jump(true);
        }
        
    }

    public void DropSwap()
    {
        if (CanSpawnedKnightSwap && IsHorseControlled && controller.spawnedKnight == null)
        {            
            HandlePickDropKnight();
            controller.spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("horse");
        }
    }

    private void OnSwapPerformed(InputAction.CallbackContext context)
    {
        DropSwap();
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
        if (!IsHorseControlled || IsHorsePaused)
        {
            controller.Move(0);
            return;
        }

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
        float horizontalMove = moveInput;
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

    private void HandleLook()
    {
        // Fast-exit when not controllable or action missing
        if (!IsHorseControlled || lookDown == null) return;

        // Read input once
        float raw = lookDown.ReadValue<float>();

        // Deadzone to avoid accidental slight axis values
        const float deadzone = 0.2f;
        int dir = 0;
        if (raw > deadzone) dir = 1;       // look up
        else if (raw < -deadzone) dir = -1; // look down

        // If direction changed, reset timer and stop any active look
        if (dir != currentLookDir)
        {
            currentLookDir = dir;
            lookTimer = 0f;
            if (isLooking)
            {
                controller.StopLooking();
                controller.IsLookingDown = false;
                isLooking = false;
            }
        }

        if (dir != 0)
        {
            lookTimer += Time.deltaTime;
            if (lookTimer >= lookHoldTime && !isLooking)
            {
                // mark controller state for down-specific logic and call Look with direction
                controller.IsLookingDown = (dir < 0);
                controller.Look(dir);
                isLooking = true;
            }
        }
        else
        {
            // neutral input: ensure we stop looking
            lookTimer = 0f;
            if (isLooking)
            {
                controller.StopLooking();
                controller.IsLookingDown = false;
                isLooking = false;
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
            controller.KnightPickUp(true);
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
            rb.velocity.x != 0 && !controller.IsKnockedback)
        {
            rb.velocity = new Vector2(rb.velocity.x / 10, rb.velocity.y);
        }
    }

    public void OnLanding()
    {
        GameObject dust = Instantiate(LandingCloud, transform.position, transform.rotation);
        dust.transform.localScale = transform.localScale;
        Destroy(dust, 1f);
        controller.DoubleJumpReady = true;
        controller.canJump = true;
        //if (controller.IsGrounded && rb.velocity.y <= 0f)
        //{

            controller.IsKnockedback = false;
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

    public void EnableCharacterControls()
    {
        IsHorseControlled = true;
        if (controller.spawnedKnight != null)
            controller.spawnedKnight.GetComponent<KnightMovement>().enabled = false;

    }

    public void DisableCharacterControls()
    {
        PauseHorseControls(0.1f);
    }
    public async void PauseHorseControls(float duration)
    {
        if (controller.spawnedKnight != null)
            controller.spawnedKnight.GetComponent<KnightMovement>().isKnightControlled = false;
        IsHorseControlled = false;
        await System.Threading.Tasks.Task.Delay((int)(duration * 1000));
        this.enabled = false;
        if (controller.spawnedKnight != null)
        {
            controller.spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("knight");
            controller.spawnedKnight.GetComponent<KnightMovement>().enabled = false;
        }
        
    }
    #endregion
}