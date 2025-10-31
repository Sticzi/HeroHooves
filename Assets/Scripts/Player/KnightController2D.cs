using Cinemachine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class KnightController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = 0.05f;
    public bool m_AirControl = false;
    public float movementSpeed;
    private float _move;

    // Copied from HorseController2D: terminal fall speed (negative)
    public float maxFallSpeed = -20f;

    [Header("References")]
    public GameObject jumpCloud;
    public GameObject horse;
    public GameObject virtualCameraKnight;
    public GameObject controlIndicator;
    public ContactFilter2D groundContactFilter;

    [Header("Launch Settings")]
    private Vector2 externalVelocity;
    public float airResistance = 0.99f;

    [Header("Audio")]    
    public float stepInterval = 0.5f;


    // Component references
    private Rigidbody2D rb;
    public Animator anim;
    private KnightMovement movement;
    private CinemachineImpulseSource impulseSource;

    // State variables
    [HideInInspector] public bool IsGrounded => rb.IsTouching(groundContactFilter);
    public bool IsInRangeOfHorse { get; set; }
    public bool isKnockedback { get; set; }
    public bool isClimbing { get; set; }

    private bool m_FacingRight = true;
    private Vector3 m_Velocity = Vector3.zero;
    private float stepTimer = 0f;
    private bool wasGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movement = GetComponent<KnightMovement>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        virtualCameraKnight = GameObject.FindGameObjectWithTag("VirtualCameraKnight");       
    }

    private void FixedUpdate()
    {
        if (!isClimbing)
        {
            HandleFallSpeed();
            HandleGroundCheck();
            UpdateAnimator();
            HandleSliding();
            HandleMovement();
        }
        
    }

    #region Movement

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

    private void HandleMovement()
    {
        if ((IsGrounded || m_AirControl) && !isKnockedback)
        {
            HandleFootsteps(_move);
            ApplyMovement(_move);
            HandleFlipping(_move); // Naprawione: metoda jest teraz zdefiniowana
        }
        // Apply air resistance to external velocity or lerp to 0 velocity if grounded
        if (!IsGrounded) externalVelocity *= airResistance;
        //unCappedExternalVelocity += airResistance;
        else externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, 0.2f);
    }

    public void ApplyExternalVelocity(Vector2 velocity)
    {
        GetComponent<BetterJump>().isTossed = true;
        externalVelocity += velocity;
        rb.velocity = new Vector2(rb.velocity.x, externalVelocity.y);
        if (jumpCloud != null)
            SpawnJumpCloudWaveAsync(count: 3, spread: 0, lifetime: 3f, delayMs: 100, firstDelayMs: 75);
        //externalVelocity = Vector2.ClampMagnitude(externalVelocity, 50f);
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

            // Instantiate cloud with rotation aligned to velocity
            GameObject cloud = Instantiate(jumpCloud, transform.position, Quaternion.Euler(0f, 0f, angle));
            Destroy(cloud, lifetime);

            await Task.Delay(delayMs); // delay between each cloud
        }
    }

    private void ApplyMovement(float move)
    {
        Vector3 targetVelocity = new Vector2(move * movementSpeed + externalVelocity.x, rb.velocity.y);//tu sie akumuluje y velocity na kazdej klatce osobno to trzeba

        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
    }
    #endregion

    #region Animations and Sounds
    private void UpdateAnimator()
    {
        anim.SetBool("IsGrounded", IsGrounded);
        anim.SetFloat("velocity.y", rb.velocity.y);
        anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
    }

    private void HandleFootsteps(float move)
    {
        if (move != 0 && IsGrounded)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
    }
    private void HandleSliding()
    {
        if ((!movement.isKnightControlled || movement.isAttacking) && IsGrounded && rb.velocity.x != 0 && !isKnockedback)
        {
            rb.velocity = new Vector2(rb.velocity.x / 10, rb.velocity.y);
        }
    }

    private void PlayFootstepSound()
    {
        FindObjectOfType<AudioManager>().Play("knightStep");
    }
    #endregion

    #region Character Control
    public async void SwapCharacter(string whoIsControlled)
    {
        CinemachineVirtualCamera knightCamera = virtualCameraKnight.GetComponent<CinemachineVirtualCamera>();
        CinemachineVirtualCamera horseCamera = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineVirtualCamera>();

        switch (whoIsControlled)
        {
            case "horse":
                knightCamera.Follow = this.gameObject.transform;
                SetControlState(true, 20, knightCamera);
                horse.GetComponent<HorseMovement>().IsHorseControlled = false;
                GetComponent<SpriteRenderer>().sortingOrder = 2;
                ToggleControlIndicator(true);                
                break;

            case "knight":
                SetControlState(false, 0, knightCamera);
                horse.GetComponent<HorseMovement>().IsHorseControlled = true;
                GetComponent<SpriteRenderer>().sortingOrder = 0;
                ToggleControlIndicator(false);
                break;
        }

        await HandleCameraTransition(horseCamera, knightCamera);
    }

    private void SetControlState(bool isKnightControlled, int priority, CinemachineVirtualCamera camera)
    {
        movement.isKnightControlled = isKnightControlled;
        movement.whoIsControlled = isKnightControlled ? "knight" : "horse";
        camera.Priority = priority;
        anim.SetBool("isControlled", isKnightControlled);
    }

    private async Task HandleCameraTransition(CinemachineVirtualCamera horseCam, CinemachineVirtualCamera knightCam)
    {
        // Naprawione: Pobieramy komponenty CinemachineConfiner
        var horseConfiner = horseCam.GetComponent<CinemachineConfiner>();
        var knightConfiner = knightCam.GetComponent<CinemachineConfiner>();

        if (horseConfiner?.m_BoundingShape2D != knightConfiner?.m_BoundingShape2D)
        {
            horse.GetComponent<HorseController2D>().PlayerFreeze();
            await Task.Delay(750);
            horse.GetComponent<HorseController2D>().PlayerUnfreeze();
        }
    }
    #endregion

    #region Combat
    public void Attack()
    {
        anim.SetTrigger("attack");
        rb.velocity = new Vector2(0, rb.velocity.y);
        DOVirtual.DelayedCall(0.4f, () => HammerSlam());
    }

    private void HammerSlam()
    {
        FindObjectOfType<AudioManager>().Play("Attack");
        StartCoroutine(VibrateController(0.5f, 0.2f, 0.2f));
        impulseSource.GenerateImpulse();
    }

    private IEnumerator VibrateController(float lowFreq, float highFreq, float duration)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);
            yield return new WaitForSeconds(duration);
            Gamepad.current.SetMotorSpeeds(0f, 0f); // stop vibration
        }
        
    }
    #endregion

    #region Utilities
    private void HandleGroundCheck()
    {
        if (!wasGrounded && IsGrounded)
        {
            isKnockedback = false;
            if (GetComponent<BetterJump>() != null)
            {
                GetComponent<BetterJump>().isTossed = false;
            }
            
        }
        wasGrounded = IsGrounded;
    }

    // Copied behavior from HorseController2D: clamp downward velocity to a maximum (terminal) fall speed
    private void HandleFallSpeed()
    {
        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
    }

    private void ToggleControlIndicator(bool showKnightControl)
    {
        if (controlIndicator != null)
        {
            controlIndicator.GetComponent<Animator>().SetTrigger(showKnightControl ? "ControlSwap" : "ControlOff");
            horse.GetComponent<HorseController2D>().horseControlIndicator.GetComponent<Animator>()
                .SetTrigger(showKnightControl ? "ControlOff" : "ControlSwap");
        }
    }

    // Naprawione: Nowa implementacja metody HandleFlipping
    private void HandleFlipping(float moveDirection)
    {
        if ((moveDirection > 0 && !m_FacingRight) || (moveDirection < 0 && m_FacingRight))
        {
            Flip();
        }
    }

    // Naprawione: Lepsza implementacja odwracania postaci
    public void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    #endregion

    #region Ladder System
    public void ClimbLadder(float distance)
    {
        if (!isClimbing)
        {
            isClimbing = true;
            anim.SetTrigger("Climb");
        }
        FindObjectOfType<AudioManager>().Play("LadderStep");
        transform.Translate(Vector3.up * distance);
        anim.SetTrigger("Climb");
    }
    #endregion

    #region Collision pickup - falling onto horse
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null) return;

        if (!collider.gameObject.TryGetComponent<HorseController2D>(out var otherHorse)) return;

        // only trigger pickup when knight is falling fast enough and roughly above the horse
        if (rb == null) return;
        if (rb.velocity.y >= -10f) return; // not falling fast enough

        // ensure knight is above the horse at collision moment
        if (transform.position.y <= otherHorse.transform.position.y) return;

        // hand-off to the horse to accept the falling knight
        otherHorse.StartDropCooldown();
        otherHorse.KnightPickUp(playSound: true);
        
    }
    #endregion
}