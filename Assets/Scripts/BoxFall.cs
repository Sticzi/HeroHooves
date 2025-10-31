using Cinemachine;
using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // ✅ for Gamepad vibration support

public class BoxFall : MonoBehaviour
{
    public bool wasGrounded;
    public ContactFilter2D contactFilter;
    [SerializeField] private ContactFilter2D knightHitContactFilter;
    [SerializeField] private ContactFilter2D horseHitContactFilter;

    public bool isGrounded => rb.IsTouching(contactFilter);
    public bool isOnKnight => rb.IsTouching(knightHitContactFilter);
    public bool isOnHorse => rb.IsTouching(horseHitContactFilter);

    private Rigidbody2D rb;
    private bool audioEnabled = false;
    public float delay = 1f;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wasGrounded = true;

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        audioEnabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isOnKnight)
        {
            collision.transform.GetComponentInChildren<KnightDeath>().Die();
            Debug.Log("Knight crushed!");
        }

        if (isOnHorse)
        {
            collision.transform.GetComponentInChildren<HorseDeath>().Die();
            Debug.Log("Horse crushed!");
        }
    }

    void FixedUpdate()
    {
        // Detect when the box hits the ground
        if (wasGrounded == false && isGrounded == true)
        {
            if (audioEnabled)
            {
                FindObjectOfType<AudioManager>().Play("Hit");
                impulseSource.GenerateImpulse();

                // ✅ Trigger controller vibration when the box lands
                StartCoroutine(VibrateController(0.7f, 0.3f, 0.3f));
            }
                

           

        }

        wasGrounded = isGrounded;

        // Lock horizontal position mid-air
        if (!isGrounded)
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            Vector2 newPosition = rb.position;
            newPosition.x = Mathf.Round(newPosition.x);
            rb.position = newPosition;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
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
}
