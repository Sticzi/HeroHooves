using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Collider2D))]
public class LaunchingPlatform : MonoBehaviour
{
    public Transform targetPosition;
    public float launchDuration = 0.2f;
    public float launchBoostForce = 10f;
    public float jumpDetectionWindow = 0.2f;
    public ContactFilter2D filter;
    public InputActionReference jumpAction;

    [Header("Shake Settings")]
    public float shakeDuration = 0.1f;
    public float shakeStrength = 0.2f;
    public int shakeVibrato = 10;

    private bool hasLaunched = false;
    private Collider2D platformCollider;
    private HorseController2D playerController;
    private Vector2 launchDirection;
    private Vector2 originalPosition;
    private bool wasTouching = false;
    private bool release;

    private void Start()
    {
        launchDirection = (targetPosition.position - transform.position).normalized;
        platformCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
    }

    private void FixedUpdate()
    {
        //tu jakieœ sphaghetti ¿eby silnik unity nie myœla³ za szybko ¿e schodzimy z platformy
        if (release && !wasTouching && !platformCollider.IsTouching(filter))
        {
            ReleasePlayer();
            Debug.Log("seiam");
        }

        if (wasTouching && !platformCollider.IsTouching(filter))
        {               
            wasTouching = false;
            release = true;
            Debug.Log("seiam2");
        }       



        Collider2D[] results = new Collider2D[2];
        int count = Physics2D.OverlapCollider(platformCollider, filter, results);

        for (int i = 0; i < count; i++)
        {
            if (platformCollider.IsTouching(results[i]))
            {
                playerController = results[i].GetComponent<HorseController2D>();
                playerController.transform.SetParent(transform);
                wasTouching = true;
                if (hasLaunched) return;
                hasLaunched = true;
                LaunchSequence().Forget();
                break;
            }
        }

        
    }

    private async UniTaskVoid LaunchSequence()
    {
        


        transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);

        // Start movement
        var moveTween = transform.DOMove(targetPosition.position, launchDuration)
            .SetEase(Ease.InQuad);

        var moveTask = moveTween.AsyncWaitForCompletion().AsUniTask();

        // Wait a bit before movement ends to trigger jump window
        await UniTask.Delay(TimeSpan.FromSeconds(launchDuration - 0.2f));


        if (playerController != null && playerController.transform.IsChildOf(transform))
        {
            HandleJumpWindow();
        }

        // Wait for move to fully complete if not already done
        await moveTask;

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        if(this != null)
        {
            await transform.DOMove(originalPosition, launchDuration*3)
            .SetEase(Ease.InSine)
            .AsyncWaitForCompletion()
            .AsUniTask();
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        hasLaunched = false;
    }


    private async UniTask HandleJumpWindow()
    {
        float timer = jumpDetectionWindow;
        while (timer > 0)
        {
            if (jumpAction.action.WasPressedThisFrame())
            {
                ApplyLaunchBoost();
                break;
            }
            timer -= Time.deltaTime;
            await UniTask.Yield();
        }
    }

    private void ReleasePlayer()
    {
        if (playerController == null) return;
        playerController.transform.SetParent(null);
        release = false;
        //ApplyLaunchBoost();
    }

    private void ApplyLaunchBoost()
    {
        if (playerController != null)
        {
            playerController.ApplyExternalVelocity(launchDirection * launchBoostForce);
        }
    }
}