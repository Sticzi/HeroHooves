using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlatform : MonoBehaviour
{

    public List<Vector2> waypoints = new List<Vector2>();   
    public float speed;    

    private Vector2 nextPos;
    private int nextWaypointIndex = 0;

    public bool moving = true;
    private Animator anim;

    
    void Start()
    {
        //asign the waypoint positions from the children of the gameobjects
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
        
        anim = GetComponent<Animator>();        
    }    

    

    
    void Update()
    {
        if (transform.position == new Vector3(waypoints[nextWaypointIndex].x, waypoints[nextWaypointIndex].y, transform.position.z))
        {
            if(waypoints.Count > nextWaypointIndex+1)
            {
                nextWaypointIndex++;
            }
            else
            {
                nextWaypointIndex = 0;
            }
        }
        

        //if the moving bool is true, then move
        if (moving)
        {
            anim.enabled = true;
            transform.position = Vector2.MoveTowards(transform.position, waypoints[nextWaypointIndex], speed * Time.deltaTime);
        }
        else
        {
            anim.enabled = false;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Horse") || collision.gameObject.CompareTag("Knight") || collision.gameObject.CompareTag("Box"))
        {
            collision.collider.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Horse") || collision.gameObject.CompareTag("Knight") || collision.gameObject.CompareTag("Box"))
        {
            collision.collider.transform.SetParent(null);
        }
    }
}