using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public ContactFilter2D contactFilter;
    public Animator animator;

    private Rigidbody2D rb;
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
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsFalling = false;
    }

    void Update()
    {
        IsFalling = IsPlayerOnThePlatform;
    }
}
