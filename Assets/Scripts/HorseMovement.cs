using UnityEngine;
using UnityEngine.InputSystem;

public class HorseMovement : MonoBehaviour
{
    [Header("References")]
    public HorseController2D controller;
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Movement Settings")]
    public float runSpeed = 36f;
    public float walkSpeed = 24f;
    [HideInInspector] public float currentSpeed;

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.15f;

    public float jumpBufferCounter;
    public float hangCounter;

    [Header("Detection Settings")]
    public float Radius;
    [SerializeField] private LayerMask whatIsKnight;
    [SerializeField] private Vector2 offset;

    [Header("Other Settings")]
    [SerializeField] private float holdTime = 1f;
    private float timer = 0f;
    public bool isHorseControlled = true;

    // Input System
    public HorseInputActions HorseInput;
    private InputAction move;
    public InputAction jump;
    private InputAction lookDown;
    private InputAction pickDropKnight;

    private void Awake()
    {
        controller = GetComponent<HorseController2D>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = runSpeed;
        HorseInput = new HorseInputActions();
    }

    private void OnEnable()
    {
        // Konfiguracja akcji
        move = HorseInput.Player.Move;
        jump = HorseInput.Player.Jump;
        lookDown = HorseInput.Player.LookDown;
        pickDropKnight = HorseInput.Player.PickDropKnight;

        move.Enable();
        jump.Enable();
        lookDown.Enable();
        pickDropKnight.Enable();

        // Subskrypcje zdarzeñ
        jump.performed += OnJumpPerformed;
        jump.canceled += OnJumpCanceled;
        pickDropKnight.performed += OnPickDropPerformed;
    }

    private void OnDisable()
    {
        move.Disable();
        jump.Disable();
        lookDown.Disable();
        pickDropKnight.Disable();

        // Anulowanie subskrypcji
        jump.performed -= OnJumpPerformed;
        jump.canceled -= OnJumpCanceled;
        pickDropKnight.performed -= OnPickDropPerformed;
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
        jumpBufferCounter = jumpBufferTime;
        controller.Jump(true);
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
        if (!isHorseControlled) return;

        float moveInput = move.ReadValue<float>();
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
        if (!isHorseControlled) return;

        float lookDownInput = lookDown.ReadValue<float>();
        if (lookDownInput < 0)
        {
            timer += Time.deltaTime;
            if (timer >= holdTime && !controller.isLookingDown)
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
        if (controller.KnightPickedUp)
        {
            controller.KnightDropOfF();
        }
        else if (controller.IsInRangeOfKnight)
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
        if (!isHorseControlled && controller.IsGrounded &&
            rb.velocity.x != 0 && !controller.isKnockedback)
        {
            rb.velocity = new Vector2(rb.velocity.x / 10, rb.velocity.y);
        }
    }

    public void OnLanding()
    {
        controller.doubleJumpReady = true;
            controller.canJump = true;
        if (controller.IsGrounded && rb.velocity.y <= 0f)
        {
            controller.isKnockedback = false;
            animator.SetBool("IsJumping", false);
            controller.m_AirControl = true;
        }
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