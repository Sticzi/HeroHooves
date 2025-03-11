using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class MovePlatform : MonoBehaviour
{

    public List<Vector2> waypoints = new List<Vector2>();   
    public float speed;    

    private Vector2 nextPos;
    private int nextWaypointIndex = 0;

    public bool moving = false;

    private Animator anim;

    public float duration;


    public Transform mainCamera; // Reference to the player's transform
    public float maxDistance = 10f; // The distance at which the sound is completely inaudible  

    void Start()
    {
        moving = transform.parent.GetComponent<LDtkFields>().GetBool("Active");

        //asign the waypoint positions from the children of the gameobjects
        Transform parentTransform = transform.parent;
        waypoints.Add(transform.position);

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

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
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

        VolumeDistance();

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

    private void VolumeDistance()
    {
        // Calculate the distance between the player and the sound source
        float distance = Vector2.Distance(transform.position, mainCamera.position);

        // Map the distance to a volume level
        float volume = Mathf.Clamp01(0.55f - (distance / maxDistance));

        // Set the audio source's volume
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