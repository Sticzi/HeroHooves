using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class Lever : MonoBehaviour
{
    public GameObject movingPlatform;
    void Start()
    {
        movingPlatform = GetComponent<LDtkFields>().GetEntityReference("MovingPlatform").GetEntity().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
