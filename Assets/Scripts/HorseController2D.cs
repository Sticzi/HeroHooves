using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class HorseController2D : MonoBehaviour
{
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the HorseMovement
    public bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    
    //[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private AudioSource JumpSound;
    [SerializeField] public float minPitch, maxPitch, minPitchDouble, maxPitchDouble;

    const float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
    private Rigidbody2D rb;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;


    public float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    
    public float jumpCloudOffset;
    public GameObject jumpCloud;

    public float coyoteTime;
    private float hangCounter;

    public bool KnightPickedUp;

    private bool isJumping;
    public bool doubleJumpReady;

    private Animator anim;
    private HorseMovement movement;
    private BetterJump betterJump;
    private Transform background;

    public GameObject knightPrefab;
    public GameObject spawnedKnight;
    public GameObject virtualCameraKnight;
    public GameObject knight;

    private CinemachineConfiner horseDownCameraConfinerComponent;
    private CinemachineConfiner horseCameraConfinerComponent;

    public float maxFallSpeed;
    public bool IsInRangeOfKnight;
    public bool isFrozen;
    public bool isLookingDown;
    public bool isKnockedback;

    private Vector2 horseVelocityContainer;
    private Vector2 knightVelocityContainer;

    public AudioClip[] footstepSounds; // Tablica dŸwiêków kroków
    public float stepInterval = 0.5f; // Interwa³ pomiêdzy krokami
    private float stepTimer = 0f;

    public AudioSource footstepSource;
    public AudioSource equipKnight;

    bool wasGrounded;

    public ContactFilter2D groundContactFilter;    
    public bool IsGrounded => rb.IsTouching(groundContactFilter);  


    [Header("Events")]    
    [Space]

    public UnityEvent OnLandEvent;    


    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }      
    
    private void Awake()
    {
        horseDownCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorseDown").GetComponent<CinemachineConfiner>();
        horseCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();

        background = GameObject.FindGameObjectWithTag("Background").GetComponent<Transform>();
        virtualCameraKnight = GameObject.FindGameObjectWithTag("VirtualCameraKnight");
        movement = GetComponent<HorseMovement>();
        betterJump = GetComponent<BetterJump>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (OnLandEvent == null)
        {
            OnLandEvent = new UnityEvent();      
        }
    }
    private void Update()
    {
        if (IsGrounded)
        {
            anim.SetBool("IsGrounded", true);
        }
        else
        {
            anim.SetBool("IsGrounded", false);
        }
    }

   
    
    private void FixedUpdate()
    {
        if (rb.velocity.y < maxFallSpeed) //maksymalna prêdkoœæ spadania, wszo dzia³a znaczek wiêkszoœci
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }

        //GroundedCheck();        

        if (wasGrounded == false && IsGrounded == true)
        {
            OnLandEvent.Invoke();
        }
        wasGrounded = IsGrounded;

        if (IsGrounded)
        {
            hangCounter = coyoteTime;
        }
        else
        {
            if (isJumping)
            {
                hangCounter = 0f;
            }

            hangCounter -= Time.deltaTime;
        }

        if (isLookingDown)
        {
            LookDown();            
        }

        if(isJumping)
        {
            //przy puszczeniu skoku skok siê koñczy
            if (!Input.GetButton("Jump"))
            {
                isJumping = false;
            }
        }        
    }   

    public void LookDown()
    {
        horseDownCameraConfinerComponent.m_BoundingShape2D = horseCameraConfinerComponent.m_BoundingShape2D;
        horseDownCameraConfinerComponent.GetComponent<CinemachineVirtualCamera>().Priority = 15;
    }

    public void StopLookingDown()
    {
        horseDownCameraConfinerComponent.GetComponent<CinemachineVirtualCamera>().Priority = 0;
    }

    public void Move(float move)
    {

        

        //only control the player if grounded or airControl is turned on
        if ((IsGrounded || m_AirControl) && !isKnockedback)
        {
            if(move != 0 && IsGrounded)
            {                
                stepTimer += Time.deltaTime;

                // Jeœli up³yn¹³ odpowiedni interwa³
                if (stepTimer >= stepInterval)
                {
                    // Odtwórz losowy dŸwiêk kroku
                    PlayFootstepSound();

                    // Zresetuj timer kroku
                    stepTimer = 0f;
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, rb.velocity.y);
            // And then smoothing it out and applying it to the character
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }        
    }

    void PlayFootstepSound()
    {      
        AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
        footstepSource.PlayOneShot(footstepSound);
    }

    public void Jump(bool jump)
    {
        // If the player should jump...
        if (movement.jumpBufferCounter >= 0 && hangCounter > 0f && !isJumping)
        {
            // Add a vertical force to the player.
            JumpSound.pitch = Random.Range(minPitch, maxPitch);
            JumpSound.Play();                      
            isJumping = true;
            betterJump.jump = true;
            betterJump.isTossed = false;
            rb.velocity = new Vector2(rb.velocity.x, m_JumpForce);
            
        }

        //Double Jump
        if (jump && !IsGrounded && !isJumping && doubleJumpReady && !KnightPickedUp)
        {
            DoubleJump();
        }
    }

    public void DoubleJump()
    {
        //stwórz, zapisz, usuñ
        GameObject cloud = Instantiate(jumpCloud, new Vector2(transform.position.x, transform.position.y - jumpCloudOffset), transform.rotation);
        Destroy(cloud, 2f);

        JumpSound.pitch = Random.Range(minPitchDouble, maxPitchDouble);
        JumpSound.Play();
        doubleJumpReady = false;
        m_AirControl = true;
        isKnockedback = false;
        isJumping = true;
        betterJump.jump = true;
        betterJump.isTossed = false;
        rb.velocity = new Vector2(rb.velocity.x, m_JumpForce);
    }

    public void KnightDropOfF()
    {
        anim.SetBool("CaryingKnight", false);
        KnightPickedUp = false;
        movement.currentSpeed = movement.runSpeed;
        spawnedKnight = Instantiate(knightPrefab, transform.position, Quaternion.identity);
        spawnedKnight.transform.GetChild(0).GetComponent<KnightDeath>().resp = GetComponent<Respawn>();
        spawnedKnight.GetComponent<KnightController2D>().horse = gameObject;        
    }

    public void KnightPickUp()
    {
        virtualCameraKnight.GetComponent<CinemachineVirtualCamera>().Priority = 0;

        anim.SetBool("CaryingKnight", true);
        KnightPickedUp = true;
        movement.currentSpeed = movement.walkSpeed;
        movement.isHorseControlled = true;
        equipKnight.Play();
        Destroy(spawnedKnight);
    }

    public void InRangeOfKnight()
    {
        IsInRangeOfKnight = true;
    }

    public void OutOfRangeOfKnight()
    {
        IsInRangeOfKnight = false;
    }


    private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

        transform.Rotate(0f, 180f, 0f);
	}

    public void PlayerFreeze()
    {
        for (int i = 0; i < 5; i++)
        {
            background.GetChild(i).GetComponent<Paralax>().cameraLerp = true;
        }
        if (isFrozen == false)
        {            
            horseVelocityContainer = GetComponent<Rigidbody2D>().velocity;
            GetComponent<Animator>().enabled = false;
            GetComponent<HorseMovement>().enabled = false;
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            knight = GameObject.FindGameObjectWithTag("Knight");

            if (knight != null)
            {
                knightVelocityContainer = GetComponent<Rigidbody2D>().velocity;
                knight.GetComponent<KnightMovement>().enabled = false;
                knight.GetComponent<Rigidbody2D>().isKinematic = true;
                knight.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                knight.GetComponent<Animator>().enabled = false;
            }
            isFrozen = true;
        }
    }

    public void KnightAndHorseFreeze()
    {
        for (int i = 0; i < 5; i++)
        {
            background.GetChild(i).GetComponent<Paralax>().cameraLerp = false;
        }

        if (isFrozen == true)
        {            
            
            GetComponent<HorseMovement>().enabled = true;
            GetComponent<Rigidbody2D>().isKinematic = false;
            GetComponent<Rigidbody2D>().velocity = horseVelocityContainer;
            GetComponent<Animator>().enabled = true;

            knight = GameObject.FindGameObjectWithTag("Knight");
            if (knight != null)
            {
                knight.GetComponent<KnightMovement>().enabled = true;
                knight.GetComponent<Rigidbody2D>().isKinematic = false;
                knight.GetComponent<Rigidbody2D>().velocity = knightVelocityContainer;
                knight.GetComponent<Animator>().enabled = true;
            }
            isFrozen = false;
        }
    }
    private void OnDrawGizmosSelected()
    {       
        Gizmos.DrawWireSphere(new Vector2(transform.position.x, transform.position.y - jumpCloudOffset), 0.2f);
    }
}



