using Cinemachine;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

public class KnightController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = 0.05f;
    public bool m_AirControl = false;

    [Header("References")]
    public GameObject horse;
    public GameObject virtualCameraKnight;
    public GameObject controlIndicator;
    public ContactFilter2D groundContactFilter;

    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;

    // Component references
    private Rigidbody2D rb;
    public Animator anim;
    private KnightMovement movement;

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

        virtualCameraKnight = GameObject.FindGameObjectWithTag("VirtualCameraKnight");
        if (controlIndicator != null) controlIndicator.SetActive(false);
    }

    private void FixedUpdate()
    {
        HandleGroundCheck();
        UpdateAnimator();
    }

    #region Movement
    public void Move(float move)
    {
        if ((IsGrounded || m_AirControl) && !isKnockedback)
        {
            HandleFootsteps(move);
            ApplyMovement(move);
            HandleFlipping(move); // Naprawione: metoda jest teraz zdefiniowana
        }
    }

    private void ApplyMovement(float move)
    {
        Vector3 targetVelocity = new Vector2(move * 10f, rb.velocity.y);
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

    private void PlayFootstepSound()
    {
        if (footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            footstepSource.PlayOneShot(clip);
        }
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
                SetControlState(true, 20, knightCamera);
                horse.GetComponent<HorseMovement>().isHorseControlled = false;
                ToggleControlIndicator(true);
                break;

            case "knight":
                SetControlState(false, 0, knightCamera);
                horse.GetComponent<HorseMovement>().isHorseControlled = true;
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
        DOVirtual.DelayedCall(0.35f, () => FindObjectOfType<AudioManager>().Play("Attack"));
    }
    #endregion

    #region Utilities
    private void HandleGroundCheck()
    {
        if (!wasGrounded && IsGrounded)
        {
            isKnockedback = false;
        }
        wasGrounded = IsGrounded;
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
    private void Flip()
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
        transform.Translate(Vector3.up * distance);
    }
    #endregion
}