using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovePlatform : MonoBehaviour
{
    public List<Vector2> waypoints = new List<Vector2>();
    public float movePlatformSpeed = 3f;
    public float waitTimeAtWaypoint = 1f; // Time to wait at each waypoint

    public bool moving = true;

    private int currentWaypointIndex = 0;
    private Animator anim;
    private Vector2 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        startPosition = transform.position;

        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            foreach (Transform child in parentTransform)
            {
                if (child.name.StartsWith("Position_"))
                {
                    waypoints.Add(child.position);
                }
            }
        }

        if (waypoints.Count > 0)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (!moving || waypoints.Count == 0)
            return;

        Vector2 targetWaypoint = waypoints[currentWaypointIndex];
        anim.enabled = true;

        transform.DOMove(targetWaypoint, movePlatformSpeed).SetSpeedBased(true).OnComplete(() =>
        {
            anim.enabled = false;
            StartCoroutine(WaitAndMoveToNext());
        });
    }

    private IEnumerator WaitAndMoveToNext()
    {
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Count)
        {
            // If the platform has reached the last waypoint, move back to the starting position
            currentWaypointIndex = 0;
            MoveToStartPosition();
        }
        else
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToStartPosition()
    {
        anim.enabled = true;

        transform.DOMove(startPosition, movePlatformSpeed).SetSpeedBased(true).OnComplete(() =>
        {
            anim.enabled = false;
            StartCoroutine(WaitAndMoveToFirstWaypoint());
        });
    }

    private IEnumerator WaitAndMoveToFirstWaypoint()
    {
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        MoveToNextWaypoint();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SetParent(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        SetParentNull(collision);
    }

    private void SetParent(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Horse") || collision.gameObject.CompareTag("Knight") || collision.gameObject.CompareTag("Box"))
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    private void SetParentNull(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Horse") || collision.gameObject.CompareTag("Knight") || collision.gameObject.CompareTag("Box"))
        {
            collision.collider.transform.SetParent(null);
        }
    }
}
