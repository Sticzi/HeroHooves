using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LDtkUnity;

public class MovePlatform : MonoBehaviour
{
    public List<Vector3> waypoints = new List<Vector3>();
    public float speed = 2f;

    [SerializeField] private bool startActive;
    private Animator anim;
    private Tween movementTween;

    private void Start()
    {
        InitializeComponents();
        SetupWaypoints();
        CreateMovementTween();
        ToggleMovement(startActive);
    }

    private void InitializeComponents()
    {
        anim = GetComponent<Animator>();
        startActive = transform.parent.GetComponent<LDtkFields>().GetBool("Active");
    }

    private void SetupWaypoints()
    {
        waypoints.Add(transform.position); // Initial position

        Transform parent = transform.parent;
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Position_"))
            {
                waypoints.Add(child.position);
            }
        }
    }

    private void CreateMovementTween()
    {
        if (waypoints.Count < 2) return;

        float pathDuration = CalculatePathDuration();

        movementTween = transform.DOPath(
                waypoints.ToArray(),
                pathDuration,
                PathType.Linear,
                PathMode.Sidescroller2D,
                10,
                Color.green)
            .SetEase(Ease.Linear)
            .SetLoops(-1)
            .SetAutoKill(false)
            .OnPlay(() => anim.speed = 1)
            .OnPause(() => anim.speed = 0);
    }

    private float CalculatePathDuration()
    {
        float totalDistance = 0f;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 current = waypoints[i];
            Vector3 next = waypoints[(i + 1) % waypoints.Count];
            totalDistance += Vector3.Distance(current, next);
        }
        return totalDistance / speed;
    }

    public void ToggleMovement(bool active)
    {
        if (movementTween == null) return;

        if (active) movementTween.Play();
        else movementTween.Pause();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsTransportable(collision.gameObject))
            collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsTransportable(collision.gameObject))
            collision.transform.SetParent(null);
    }

    private bool IsTransportable(GameObject obj) =>
        obj.CompareTag("Horse") || obj.CompareTag("Knight") || obj.CompareTag("Box");

    private void OnDestroy() => movementTween?.Kill();
}