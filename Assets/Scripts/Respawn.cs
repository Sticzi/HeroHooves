using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour
{        
    public GameObject knightPrefab;
    private HorseController2D horseController;      
    public GameMaster gameMaster;
    private CinemachineConfiner horseCameraConfinerComponent;
    private CinemachineConfiner knightCameraConfinerComponent;
    private Transform world;
        
    public void Awake()
    {
        horseCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();
        knightCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraKnight").GetComponent<CinemachineConfiner>();
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        horseController = GetComponent<HorseController2D>();
        knightPrefab = horseController.knightPrefab;

        world = GameObject.FindGameObjectWithTag("World").transform;             
    }

    private Transform FindLevel(int levelNumber)
    {
        foreach (Transform child in world)
        {
            if (child.name == ("Level_" + levelNumber))
            {
                return child;
            }
        }
        return null;
    }

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
    private Transform FindCheckpoint(int levelNumber)
    {        
        return FindChildWithTag(FindLevel(levelNumber).Find("Entities"), "Checkpoint");
    }

    public void spawning()
    {
        if (gameMaster.horseSavedRoom == gameMaster.knightSavedRoom)
        {            
            Transform checkpoint = FindCheckpoint(gameMaster.horseSavedRoom);
            transform.position = checkpoint.position;            
            horseController.KnightPickUp();
        }
        else
        {
            Transform checkpoint = FindCheckpoint(gameMaster.horseSavedRoom);
            transform.position = checkpoint.position;

            checkpoint = FindCheckpoint(gameMaster.knightSavedRoom);
            GameObject newKnight = Instantiate(knightPrefab, checkpoint.position, Quaternion.identity);
            newKnight.GetComponent<KnightController2D>().horse = gameObject;
            horseController.KnightPickedUp = false;
            horseController.spawnedKnight = newKnight;
        }
        Collider2D cameraConfiner = (FindLevel(gameMaster.horseSavedRoom).Find("CameraBound").GetChild(0)).GetComponent<Collider2D>();

        horseCameraConfinerComponent.m_BoundingShape2D = cameraConfiner;
        cameraConfiner = (FindLevel(gameMaster.knightSavedRoom).Find("CameraBound").GetChild(0)).GetComponent<Collider2D>();
        knightCameraConfinerComponent.m_BoundingShape2D = cameraConfiner;
    }

    public void Start()
    {
        spawning();
    }    

    public void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
