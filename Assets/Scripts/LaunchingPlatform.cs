using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using LDtkUnity;

[RequireComponent(typeof(Collider2D))]
public class LaunchingPlatform : MonoBehaviour
{
    
    public float launchDuration = 0.2f;
    public float jumpDetectionWindow = 0.15f;
    public ContactFilter2D filter;
    public InputActionReference jumpAction;
    public LDtkFields ldtkValues;
    
    public bool isActive;

    public float shakeDuration = 0.1f;
    public float shakeStrength = 0.2f;
    public int shakeVibrato = 10;

    private Vector3 targetPosition;
    private float launchBoostForce = 10f;

    private bool hasLaunched = false;
    private Collider2D platformCollider;
    private GameObject launchedObject;
    private Vector2 launchDirection;
    private Vector2 originalPosition;
    private bool wasTouching = false;
    private bool release;

    private void Start()
    {
        ldtkValues = transform.parent.GetComponent<LDtkFields>();

        isActive = ldtkValues.GetBool("active");
        launchBoostForce = ldtkValues.GetFloat("launchBoostForce");
        targetPosition = ldtkValues.GetPoint("targetPosition");

        launchDirection = (targetPosition - transform.position).normalized;
        platformCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if(isActive)
        HandleLaunching();
    }

    private void HandleLaunching()
    {
        //tu jakieœ sphaghetti ¿eby unity nie myœla³o za szybko ¿e schodzimy z platformy
        if (release && !wasTouching && !platformCollider.IsTouching(filter))
        {
            ReleasePlayer();
        }

        if (wasTouching && !platformCollider.IsTouching(filter))
        {
            wasTouching = false;
            release = true;
        }

        Collider2D[] results = new Collider2D[2];
        int count = Physics2D.OverlapCollider(platformCollider, filter, results);

        for (int i = 0; i < count; i++)
        {
            if (platformCollider.IsTouching(results[i]))
            {
                launchedObject = results[i].gameObject;
                launchedObject.transform.SetParent(transform);
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
        var moveTween = transform.DOMove(targetPosition, launchDuration)
            .SetEase(Ease.InQuart);

        var moveTask = moveTween.AsyncWaitForCompletion().AsUniTask();

        // Wait a bit before movement ends to trigger jump window
        await UniTask.Delay(TimeSpan.FromSeconds(launchDuration - 0.2f));


        if (launchedObject != null && launchedObject.transform.IsChildOf(transform))
        {
            
            HandleJumpWindow();
            
        }

        // Wait for move to fully complete if not already done
        await moveTask;

        if (launchedObject.GetComponent<KnightController2D>() != null)
        {
            Debug.Log("knightLaunch");
            launchedObject.GetComponent<KnightController2D>().ApplyExternalVelocity(launchDirection * launchBoostForce);
        }

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
        if (launchedObject == null) return;
        launchedObject.transform.SetParent(null);
        release = false;
        //ApplyLaunchBoost();
    }

    private void ApplyLaunchBoost()
    {
        if (launchedObject.GetComponent<HorseController2D>() != null)
        {
            launchedObject.GetComponent<HorseController2D>().ApplyExternalVelocity(launchDirection * launchBoostForce);
        }
        
    }
}