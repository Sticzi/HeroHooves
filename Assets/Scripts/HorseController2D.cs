using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class HorseController2D : MonoBehaviour
{
    public GameObject horseControlIndicator;
    public GameObject spawnedKnight;

    [Header("Movement Settings")]
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;
    public bool m_AirControl = false;
    public float m_JumpForce = 400f;
    public float maxFallSpeed = -20f;
    public float movementSpeed;
    public float movementSpeedWithKnight;
    private float currentMovementSpeed;

    [Header("Launch Settings")]
    public float airResistance = 0.9f;
    public Vector2 externalVelocity;
    private Vector2 unCappedExternalVelocity;

    [Header("References")]
    public GameObject jumpCloud;
    public float jumpCloudOffset;
    public GameObject knightPrefab;
    public ContactFilter2D groundContactFilter;
    private HorseMovement movement;

    [Header("Audio")]
    public float stepInterval = 0.5f;

    // State
    [HideInInspector] public bool isJumpButtonHeld;
    public bool IsGrounded => rb.IsTouching(groundContactFilter);
    public bool KnightPickedUp { get; set; }
    public bool IsInRangeOfKnight { get; set; }
    public bool IsFrozen { get; set; }
    public bool IsLookingDown { get; set; }
    public bool IsKnockedback { get; set; }
    public bool DoubleJumpReady { get; set; }
    //public bool isJumping { get; set; }
    public bool canJump = false;

    public void InRangeOfKnight() => IsInRangeOfKnight = true;
    public void OutOfRangeOfKnight() => IsInRangeOfKnight = false;

    // Internal
    private Rigidbody2D rb;
    private Animator anim;
    private BetterJump betterJump;
    private Transform background;
    private CinemachineConfiner horseCameraConfiner;
    private CinemachineConfiner horseDownCameraConfiner;
    private Vector3 m_Velocity = Vector3.zero;
    private float stepTimer = 0f;
    private bool wasGrounded;

    private float _move;

    [Header("Events")]
    public UnityEvent OnLandEvent;

    private void Awake()
    {
        currentMovementSpeed = movementSpeed;
        movement = GetComponent<HorseMovement>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        betterJump = GetComponent<BetterJump>();
        background = GameObject.FindGameObjectWithTag("Background").transform;

        horseCameraConfiner = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();
        if(GameObject.FindGameObjectWithTag("VirtualCameraHorseDown") != null)
        {
            horseDownCameraConfiner = GameObject.FindGameObjectWithTag("VirtualCameraHorseDown").GetComponent<CinemachineConfiner>();
        }
        
    }

    private void Update()
    {
        anim.SetBool("IsGrounded", IsGrounded);
    }

    private void FixedUpdate()
    {
        HandleFallSpeed();
        HandleGroundCheck();
        HandleJumping();
        HandleMovement();
    }

    #region Core Mechanics
    private void HandleFallSpeed()
    {
        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
    }

    private void HandleGroundCheck()
    {
        if (!wasGrounded && IsGrounded)
            OnLandEvent.Invoke();

        wasGrounded = IsGrounded;
    }
    #endregion

    #region Movement

    public void HandleMovement()
    {
        if ((IsGrounded || m_AirControl) && !IsKnockedback)
        {
            HandleFootsteps(_move);
            ApplyMovement(_move);
            HandleFlipping(_move);
        }

        // Apply air resistance to external velocity or lerp to 0 velocity if grounded
        if (!IsGrounded) externalVelocity *= airResistance;
        //unCappedExternalVelocity += airResistance;
        else externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, 0.1f);
    }
    public void Move(float move)
    {
        _move = move > 0 ? Mathf.Ceil(move) : Mathf.Floor(move);
    }

    // New method to handle external velocity
    public void ApplyExternalVelocity(Vector2 velocity)
    {
        GetComponent<BetterJump>().isTossed = true;
        externalVelocity += velocity;
        //externalVelocity = Vector2.ClampMagnitude(externalVelocity, 50f);
    }

    // Modified ApplyMovement
    private void ApplyMovement(float move)
    {
        Vector3 targetVelocity = new Vector2(move * currentMovementSpeed + externalVelocity.x, rb.velocity.y);
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }

    private void HandleFootsteps(float move)
    {
        if (Mathf.Abs(move) > 0.1f && IsGrounded)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
    }

    private void PlayFootstepSound()
    {
        FindObjectOfType<AudioManager>().Play("horseStep");
    }
    #endregion

    #region Jump Logic

    private void HandleJumping()
    {
        if (movement.jumpBufferCounter > 0 && movement.hangCounter > 0 && canJump)
        {
            ExecuteJump();
            canJump = false;
        }        
    }

    public void Jump(bool jumpInput)
    {       
        if (movement.CanDoubleJump && jumpInput && DoubleJumpReady && !KnightPickedUp && (movement.hangCounter <= 0 || canJump == false))
            DoubleJump();
    }

    public void ExecuteJump()
    {
        betterJump.jump = true;
        anim.SetBool("IsJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, m_JumpForce + externalVelocity.y);
        movement.hangCounter = 0;
        movement.jumpBufferCounter = 0;
        FindObjectOfType<AudioManager>().Play("Jump");
    }

    public void DoubleJump()
    {
        GameObject cloud = Instantiate(jumpCloud, transform.position + Vector3.down * jumpCloudOffset, Quaternion.identity);
        Destroy(cloud, 1);
        DoubleJumpReady = false;
        m_AirControl = true;
        IsKnockedback = false;
        betterJump.jump = true;
        rb.velocity = new Vector2(rb.velocity.x, m_JumpForce);
        FindObjectOfType<AudioManager>().Play("DoubleJump");
    }
    #endregion

    #region Flipping
    private void HandleFlipping(float moveDirection)
    {
        if ((moveDirection > 0 && !m_FacingRight) || (moveDirection < 0 && m_FacingRight))
            Flip();
    }

    private bool m_FacingRight = true;
    private void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Camera Controls
    public void LookDown()
    {
        horseDownCameraConfiner.m_BoundingShape2D = horseCameraConfiner.m_BoundingShape2D;
        horseDownCameraConfiner.GetComponent<CinemachineVirtualCamera>().Priority = 15;
    }

    public void StopLookingDown()
    {
        horseDownCameraConfiner.GetComponent<CinemachineVirtualCamera>().Priority = 0;
    }
    #endregion

    #region Knight Interaction

    //public void DropAndSwapKnight()
    //{
    //    KnightDropOfF();
    //    spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("horse");
    //}

    public void KnightPickUp(bool playSound)
    {
        KnightPickedUp = true;
        currentMovementSpeed = movementSpeedWithKnight;
        anim.SetBool("CaryingKnight", true);
        if(playSound)
        {
            FindObjectOfType<AudioManager>().Play("Equip");
        }
        

        //tu gdzieœ by wypada³oby tego knighta pierwszego usun¹æ
        if (spawnedKnight != null)
        {
            if(!movement.IsHorseControlled)
            {
                spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("knight");
            }            

            Destroy(spawnedKnight);
        }
    }

    public void KnightDropOfF()
    {
        KnightPickedUp = false;
        currentMovementSpeed = movementSpeed;
        anim.SetBool("CaryingKnight", false);
        spawnedKnight = Instantiate(knightPrefab, transform.position, Quaternion.identity);
        spawnedKnight.GetComponent<KnightController2D>().horse = this.gameObject;
        spawnedKnight.GetComponent<KnightMovement>().CanSwap = movement.canSpawnedKnightSwap;
        spawnedKnight.GetComponent<KnightMovement>().CanAttack = movement.canSpawnedKnightAttack;        
        if (transform.localScale.x == -1)
        {
            spawnedKnight.GetComponent<KnightController2D>().Flip();
        }

        FindObjectOfType<AudioManager>().Play("Equip");
    }
    #endregion

    #region Freeze System
    public void PlayerFreeze()
    {
        if (IsFrozen) return;

        //foreach (Transform child in background)
        //    child.GetComponent<Paralax>().cameraLerp = true;

        rb.bodyType = RigidbodyType2D.Static;
        ToggleComponents(false);
        IsFrozen = true;
    }

    public void PlayerUnfreeze()
    {
        if (!IsFrozen) return;

        //foreach (Transform child in background)
        //    child.GetComponent<Paralax>().cameraLerp = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        ToggleComponents(true);
        IsFrozen = false;
    }

    private void ToggleComponents(bool state)
    {
        anim.enabled = state;
        movement.enabled = state;
    }
    #endregion
}