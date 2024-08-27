using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public ContactFilter2D contactFilter;
    public Animator animator;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isFalling;

    // Public property to check if the player is on the platform
    public bool IsPlayerOnThePlatform => rb.IsTouching(contactFilter);

    // Public property to get and set the IsFalling state
    public bool IsFalling
    {
        get => isFalling;
        set
        {
            if (isFalling != value)
            {
                isFalling = value;
                animator.SetBool("IsFalling", isFalling);

                // Play the audio only when the platform starts falling
                if (isFalling)
                {
                    audioSource.Play();
                }
            }
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        IsFalling = false;
    }

    void Update()
    {
        IsFalling = IsPlayerOnThePlatform;
    }
}
