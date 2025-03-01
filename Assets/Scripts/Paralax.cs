using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
        Vector2 dist = new Vector2(camPos.position.x * parallaxEffect, camPos.position.y * parallaxEffectY);
        Vector3 targetPosition = new Vector3(startPos.x + dist.x, startPos.y + dist.y, transform.position.z);

        float temp = cam.transform.position.x * (1 - parallaxEffect);

        // Instantly reposition the background when needed to avoid jumps
        if (temp > startPos.x + length)
        {
            transform.position += new Vector3(length, 0, 0);
            startPos.x += length;
        }
        else if (temp < startPos.x - length)
        {
            transform.position -= new Vector3(length, 0, 0);
            startPos.x -= length;
        }

        // Smooth movement only when no instant repositioning is happening
        transform.DOMove(targetPosition, 0f).SetEase(Ease.Linear);
    }
}
