using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    private static GameMaster instance;

    public Vector2 savedKnightPosition;
    public Vector2 savedHorsePosition;

    //public Vector2 savedCameraPosition;
    public Vector2[] savedBackgroundPos;

    public Transform playerPositionTesting;
    public Transform levelOfFirstSpawnPoint;
    private Transform firstSpawnPoint;

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

    public void Awake()
    {
        //acces the urrent level gameobject and then from there acces the checkpoint entity
        firstSpawnPoint = FindChildWithTag(levelOfFirstSpawnPoint.Find("Entities"), "Checkpoint");
        //Debug.Log(firstSpawnPoint.position);

        if (playerPositionTesting != null)
        {
            savedKnightPosition = playerPositionTesting.position;
            savedHorsePosition = playerPositionTesting.position;
        }

        else
        {
            savedKnightPosition = firstSpawnPoint.position;
            savedHorsePosition = firstSpawnPoint.position;
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
