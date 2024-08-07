using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    private float length;
    private Vector2 startPos;
    private Transform camPos;

    public GameObject cam;
    public float parallaxEffect;
    public float parallaxEffectY;

    public bool cameraLerp;

    void Start()
    {        
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        camPos = cam.transform;
    }    

    void Update()
    {
        if(cameraLerp == true)
        {
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            Vector2 dist = new Vector2(camPos.position.x * parallaxEffect, camPos.position.y * parallaxEffectY);

            transform.position = new Vector3(startPos.x + dist.x, startPos.y + dist.y, transform.position.z);

            if (temp > startPos.x + length)
            {
                startPos.x += length;
            }


            else if (temp < startPos.x - length)
            {
                startPos.x -= length;
            }
        }

        if (cameraLerp == false)
        {
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            Vector2 dist = new Vector2(camPos.position.x * parallaxEffect, camPos.position.y * parallaxEffectY);

            transform.position = new Vector3(startPos.x + dist.x, startPos.y + dist.y, transform.position.z);

            if (temp > startPos.x + length)
            {
                startPos.x += length;
            }


            else if (temp < startPos.x - length)
            {
                startPos.x -= length;
            }
        }

    }

}
