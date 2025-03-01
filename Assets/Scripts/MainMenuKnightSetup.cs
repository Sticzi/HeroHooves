using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuKnightSetup : MonoBehaviour
{
    public GameObject spawnedKnight;
    public bool isDisabled;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDisabled)
        {
            isDisabled = true;
            spawnedKnight = GetComponent<HorseController2D>().spawnedKnight;
            spawnedKnight.GetComponent<KnightMovement>().enabled = false;
        }
    }
}
