using UnityEngine;

public class Ladder : MonoBehaviour
{
    public ContactFilter2D ContactFilter;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool IsTouchingPlayer => rb.IsTouching(ContactFilter);
}
