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
    public bool isFrozen { get; set; }
    public bool isLookingDown { get; set; }
    public bool isKnockedback { get; set; }
    public bool doubleJumpReady { get; set; }
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

    [Header("Events")]
    public UnityEvent OnLandEvent;

    private void Awake()
    {
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
    public void Move(float move)
    {
        if ((IsGrounded || m_AirControl) && !isKnockedback)
        {
            HandleFootsteps(move);
            ApplyMovement(move);
            HandleFlipping(move);
        }
    }

    private void ApplyMovement(float move)
    {
        Vector3 targetVelocity = new Vector2(move * 10f, rb.velocity.y);
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
        if (movement.CanDoubleJump && jumpInput && doubleJumpReady && !KnightPickedUp && (movement.hangCounter <= 0 || canJump == false))
            DoubleJump();
    }

    public void ExecuteJump()
    {
        betterJump.jump = true;
        anim.SetBool("IsJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, m_JumpForce);
        movement.hangCounter = 0;
        movement.jumpBufferCounter = 0;
        FindObjectOfType<AudioManager>().Play("Jump");
    }

    public void DoubleJump()
    {
        GameObject cloud = Instantiate(jumpCloud, transform.position + Vector3.down * jumpCloudOffset, Quaternion.identity);
        Destroy(cloud, 1);
        doubleJumpReady = false;
        m_AirControl = true;
        isKnockedback = false;
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

    public void DropAndSwapKnight()
    {
        KnightDropOfF();
        spawnedKnight.GetComponent<KnightController2D>().SwapCharacter("horse");
    }

    public void KnightPickUp()
    {
        KnightPickedUp = true;
        movement.currentSpeed = movement.walkSpeed;
        anim.SetBool("CaryingKnight", true);
        FindObjectOfType<AudioManager>().Play("Equip");

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
        movement.currentSpeed = movement.runSpeed;
        anim.SetBool("CaryingKnight", false);
        spawnedKnight = Instantiate(knightPrefab, transform.position, Quaternion.identity);
        spawnedKnight.GetComponent<KnightController2D>().horse = this.gameObject;
        spawnedKnight.GetComponent<KnightMovement>().CanSwap = movement.canSpawnedKnightSwap;
        spawnedKnight.GetComponent<KnightMovement>().CanAttack = movement.canSpawnedKnightAttack;
        FindObjectOfType<AudioManager>().Play("Equip");
    }
    #endregion

    #region Freeze System
    public void PlayerFreeze()
    {
        if (isFrozen) return;

        //foreach (Transform child in background)
        //    child.GetComponent<Paralax>().cameraLerp = true;

        rb.bodyType = RigidbodyType2D.Static;
        ToggleComponents(false);
        isFrozen = true;
    }

    public void PlayerUnfreeze()
    {
        if (!isFrozen) return;

        //foreach (Transform child in background)
        //    child.GetComponent<Paralax>().cameraLerp = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        ToggleComponents(true);
        isFrozen = false;
    }

    private void ToggleComponents(bool state)
    {
        anim.enabled = state;
        movement.enabled = state;
    }
    #endregion
}