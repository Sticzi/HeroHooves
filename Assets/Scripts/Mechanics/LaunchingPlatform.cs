using System.Collections.Generic;
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
    private Vector2 launchDirection;
    private Vector2 originalPosition;
    private float speed = 1f;


    private HashSet<GameObject> objectsOnPlatform = new HashSet<GameObject>();
    private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>();
    public bool IsPlayerOn => objectsOnPlatform.Count > 0;


    private Animator animator;

    // New: track how many consecutive frames an object was not detected on the platform
    [SerializeField] private int releaseFrameThreshold = 2;
    private readonly Dictionary<GameObject, int> missedFrames = new Dictionary<GameObject, int>();

    private void Start()
    {
        
        ldtkValues = transform.parent.GetComponent<LDtkFields>();

        isActive = ldtkValues.GetBool("active");
        speed = ldtkValues.GetFloat("speed");
        launchBoostForce = ldtkValues.GetFloat("launchBoostForce");
        targetPosition = ldtkValues.GetPoint("targetPosition");

        launchDirection = (targetPosition - transform.position).normalized;
        platformCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        originalPosition = transform.position;
    }

    private void Update()
    {
        HandleLaunching();
    }

    private void HandleLaunching()
    {
        // Update objects on platform
        UpdateObjectsOnPlatform();

        if (hasLaunched) return;

        if (isActive)
        {
            hasLaunched = true;
            LaunchSequence().Forget();
        }
    }

    private void UpdateObjectsOnPlatform()
    {
        Collider2D[] results = new Collider2D[5];
        int count = Physics2D.OverlapCollider(platformCollider, filter, results);

        HashSet<GameObject> currentObjects = new HashSet<GameObject>();

        // Add new objects and parent them. Reset missedFrames for detected objects.
        for (int i = 0; i < count; i++)
        {
            GameObject obj = results[i].gameObject;
            currentObjects.Add(obj);

            if (!objectsOnPlatform.Contains(obj))
            {
                // New object on platform
                objectsOnPlatform.Add(obj);
                originalParents[obj] = obj.transform.parent;
                obj.transform.SetParent(transform);
                // ensure missedFrames reset
                missedFrames[obj] = 0;
            }
            else
            {
                // object already on platform, ensure missedFrames cleared
                if (missedFrames.ContainsKey(obj))
                    missedFrames[obj] = 0;
            }
        }

        // For objects we think are on the platform but are not currently detected,
        // increment their missed frame counter and only release when threshold reached.
        List<GameObject> objectsToRelease = new List<GameObject>();
        foreach (GameObject obj in new List<GameObject>(objectsOnPlatform))
        {
            if (!currentObjects.Contains(obj))
            {
                int missed = 0;
                if (!missedFrames.TryGetValue(obj, out missed))
                    missed = 0;
                missed++;
                missedFrames[obj] = missed;

                if (missed >= releaseFrameThreshold)
                {
                    objectsToRelease.Add(obj);
                }
            }
        }

        // Release objects that exceeded missed frame threshold
        foreach (GameObject obj in objectsToRelease)
        {
            ReleaseObject(obj);
            // cleanup missedFrames entry if any
            if (missedFrames.ContainsKey(obj))
                missedFrames.Remove(obj);
        }
    }

    private void ReleaseObject(GameObject obj)
    {
        if (obj != null && originalParents.ContainsKey(obj))
        {
            obj.transform.SetParent(originalParents[obj]);
            originalParents.Remove(obj);
        }
        objectsOnPlatform.Remove(obj);
        if (missedFrames.ContainsKey(obj))
            missedFrames.Remove(obj);
    }

    public async UniTaskVoid LaunchSequence()
    {
        var shakeTween = transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
        await shakeTween.AsyncWaitForCompletion().AsUniTask();

        float duration = launchDuration;
        Ease easeType = Ease.InSine;
        Vector3 startPos = transform.position;
        Vector3 endPos = targetPosition;

        var moveTween = transform.DOMove(endPos, duration*speed).SetEase(easeType);

        Vector3 lastPos = startPos;
        animator.Play("MovingPlatform");
        if (animator != null)
        {
            moveTween.OnUpdate(() =>
            {
                float moveSpeed = (transform.position - lastPos).magnitude / Time.deltaTime;
                float baseSpeed = 7f;
                animator.speed = moveSpeed > 0.001f ? moveSpeed / baseSpeed : 0f;
                lastPos = transform.position;
            });
        }

        moveTween.onComplete += () =>
        {
            shakeTween = transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
            if (animator != null)
                animator.speed = 0f;
            foreach (GameObject obj in new List<GameObject>(objectsOnPlatform))
            {
                if (obj != null)
                {
                    if (obj.GetComponent<KnightController2D>() != null)
                    {
                        obj.GetComponent<KnightController2D>().ApplyExternalVelocity(launchDirection * launchBoostForce*2.35f);
                    }
                }
            }
        };

        var launchSoundOffset = 0.28f;
        await UniTask.Delay(TimeSpan.FromSeconds(launchSoundOffset));
        FindObjectOfType<AudioManager>().Play("launch");

        await UniTask.Delay(TimeSpan.FromSeconds(launchDuration - launchSoundOffset - jumpDetectionWindow/1.2f));
        // Handle jump window for all objects
        foreach (GameObject obj in new List<GameObject>(objectsOnPlatform))
        {
            if (obj != null && obj.transform.IsChildOf(transform))
            {
                await HandleJumpWindow(obj);
            }
        }


               

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        while (isActive)
        {
            await UniTask.Yield();
        }

        if (this != null)
        {
            var returnTween = transform.DOMove(originalPosition, launchDuration * 3)
                .SetEase(Ease.InSine);
            FindObjectOfType<AudioManager>().Play("reset");
            if (animator != null)
            {
                animator.Play("ReverseMovingPlatform");
                animator.speed = 0.8f;
            }

            await returnTween.AsyncWaitForCompletion().AsUniTask();

            if (animator != null)
            {
                animator.speed = 0f;
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        hasLaunched = false;
    }

    private async UniTask HandleJumpWindow(GameObject targetObject)
    {
        float timer = jumpDetectionWindow;
        while (timer > 0 && targetObject != null && objectsOnPlatform.Contains(targetObject))
        {
            if (jumpAction.action.WasPressedThisFrame())
            {
                ApplyLaunchBoost(targetObject);
                break;
            }
            timer -= Time.deltaTime;
            await UniTask.Yield();
        }
    }

    private void ApplyLaunchBoost(GameObject targetObject)
    {
        if (targetObject == null) return;

        if (targetObject.GetComponent<HorseController2D>() != null)
        {
            targetObject.GetComponent<HorseController2D>().ApplyExternalVelocity(launchDirection * launchBoostForce);
        }
        else if (targetObject.GetComponent<KnightController2D>() != null)
        {
            targetObject.GetComponent<KnightController2D>().ApplyExternalVelocity(launchDirection * launchBoostForce);
        }
    }

}