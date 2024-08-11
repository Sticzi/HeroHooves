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

    public AudioSource audioSource; // Reference to the AudioSource component
    public float minPitch = 1.8f;   // Minimum pitch value
    public float maxPitch = 2.7f;   // Maximum pitch value
    private Animator anim;

    public float duration;

    
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
            if (!audioSource.isPlaying)
            {
                // Randomize the pitch
                audioSource.pitch = Random.Range(minPitch, maxPitch);

                // Play the sound
                audioSource.Play();
            }
            else if (audioSource.time >= audioSource.clip.length*duration)
            {
                // Check if the sound has finished playing, then randomize pitch and play again
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.Play();
            }

            anim.enabled = true;
            transform.position = Vector2.MoveTowards(transform.position, waypoints[nextWaypointIndex], speed * Time.deltaTime);
        }
        else
        {
            if (audioSource.isPlaying)
            {
                // Stop playing the sound immediately if not moving
                audioSource.Stop();
            }

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