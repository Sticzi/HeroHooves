using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using DG.Tweening;

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
    private Vector2 externalVelocity;
    private Vector2 unCappedExternalVelocity;

    [Header("References")]
    public GameObject jumpCloud;
    public GameObject NormalJumpCloud;
    public float jumpCloudOffset;
    public GameObject knightPrefab;
    public ContactFilter2D groundContactFilter;
    private HorseMovement movement;

    [Header("Camera")]
    [Tooltip("Assign the Cinemachine camera target transform (the object the vcam follows).")]
    public Transform cameraTarget;
    [Tooltip("Local offset applied when looking up (added to starting localPosition).")]
    public Vector3 cameraUpLocalOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("Local offset applied when looking down (added to starting localPosition).")]
    public Vector3 cameraDownLocalOffset = new Vector3(0f, -1.5f, 0f);
    [Tooltip("Tween duration when moving camera target.")]
    public float cameraMoveDuration = 0.25f;
    public Ease cameraEase = Ease.InOutSine;

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
    private CinemachineConfiner horseCameraConfiner;
    private Vector3 m_Velocity = Vector3.zero;
    private float stepTimer = 0f;
    private bool wasGrounded;

    private float _move;

    [Header("Events")]
    public UnityEvent OnLandEvent;

    // camera state
    private Vector3 cameraStartLocalPos;
    private Tween cameraTween;

    private float dropCooldownTimer = 0f;
    private const float DROP_COOLDOWN_DURATION = 0.5f; // Adjust this value as needed
    private bool canDropKnight = true;

    private void Awake()
    {
        currentMovementSpeed = movementSpeed;
        movement = GetComponent<HorseMovement>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        betterJump = GetComponent<BetterJump>();
        horseCameraConfiner = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();

        if (cameraTarget != null)
            cameraStartLocalPos = cameraTarget.localPosition;
    }

    private void Update()
    {
        anim.SetBool("IsGrounded", IsGrounded);

        // Handle drop cooldown
        if (!canDropKnight)
        {
            dropCooldownTimer -= Time.deltaTime;
            if (dropCooldownTimer <= 0)
            {
                canDropKnight = true;
            }
        }
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
        if ((IsGrounded || m_AirControl) && !IsKnockedback && rb.bodyType == RigidbodyType2D.Dynamic)
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
        if (move > 0)
        {
            _move = Mathf.Ceil(move); // zaokrąglenie w górę, np. 2.3 → 3
        }
        else
        {
            _move = Mathf.Floor(move); // zaokrąglenie w dół, np. -2.3 → -3
        }

    }

    private async void SpawnJumpCloudWaveAsync(int count = 2, float spread = 0.1f, float lifetime = 3f, int delayMs = 50, int firstDelayMs = 100)
    {
        await Task.Delay(firstDelayMs);

        // Determine the direction for rotation based on external velocity
        Vector2 dir = externalVelocity.normalized;

        // Fallback if velocity is zero
        if (dir == Vector2.zero) dir = Vector2.up;

        // Calculate rotation angle so cloud is perpendicular to velocity
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // -90 because sprite is already perpendicular to (0,1)

        for (int i = 0; i < count; i++)
        {
            // Slight random scatter
            Vector3 offset = new Vector3(Random.Range(-spread, spread), -jumpCloudOffset, 0f);

            // Instantiate cloud with rotation aligned to velocity
            GameObject cloud = Instantiate(jumpCloud, transform.position + offset, Quaternion.Euler(0f, 0f, angle));
            Destroy(cloud, lifetime);

            await Task.Delay(delayMs); // delay between each cloud
        }
    }

    // New method to handle external velocity
    public void ApplyExternalVelocity(Vector2 velocity)
    {
        GetComponent<BetterJump>().isTossed = true;
        externalVelocity += velocity;

        if (jumpCloud != null)
            SpawnJumpCloudWaveAsync(count: 3, spread: 0, lifetime: 3f, delayMs: 100, firstDelayMs: 75);
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
        GameObject dust = Instantiate(NormalJumpCloud, transform.position, transform.rotation);
        dust.transform.localScale = transform.localScale;
        Destroy(dust, 1);
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
    // direction > 0 => look up, direction < 0 => look down
    public void Look(float direction)
    {
        if (cameraTarget == null) return;

        // map direction to a target Y only (no deadzone)
        float targetLocalY = cameraStartLocalPos.y;
        if (direction > 0f)
        {
            targetLocalY += cameraUpLocalOffset.y;
            IsLookingDown = false;
        }
        else if (direction < 0f)
        {
            targetLocalY += cameraDownLocalOffset.y;
            IsLookingDown = true;
        }
        else
        {
            // direction == 0 -> do nothing; StopLooking() should be called when releasing input
            return;
        }

        MoveCameraToLocalY(targetLocalY);
    }

    public void StopLooking()
    {
        if (cameraTarget == null) return;
        MoveCameraToLocalY(cameraStartLocalPos.y);
        IsLookingDown = false;
    }

    private void MoveCameraToLocalY(float localY)
    {
        if (cameraTarget == null) return;

        if (cameraTween != null && cameraTween.IsActive())
            cameraTween.Kill();

        Vector3 target = cameraStartLocalPos;
        target.y = localY;
        cameraTween = cameraTarget.DOLocalMove(target, cameraMoveDuration).SetEase(cameraEase);
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
        

        //tu gdzieś by wypadałoby tego knighta pierwszego usunąć
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
        if (!canDropKnight) return;

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

    // Add method to start the cooldown - call this from KnightController2D
    public void StartDropCooldown()
    {
        canDropKnight = false;
        dropCooldownTimer = DROP_COOLDOWN_DURATION;
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