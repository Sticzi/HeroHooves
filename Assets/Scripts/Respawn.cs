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
    private HorseMovement movement;
    
        
    public void Awake()
    {
        horseCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();
        knightCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraKnight").GetComponent<CinemachineConfiner>();
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        horseController = GetComponent<HorseController2D>();
        movement = GetComponent<HorseMovement>();
        knightPrefab = horseController.knightPrefab;


                  
    }

    private Transform FindLevel(int levelNumber)
    {
        
        Transform world = GameObject.FindGameObjectWithTag(gameMaster.savedWorldName).transform;
        foreach (Transform level in world)
        {
            if (level.name == ("Level_" + levelNumber + world.name))
            {
                return level;
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

    public void Spawning()
    {
        if (gameMaster.savedWorldName == "MainMenu")
        {
            movement.CanDoubleJump = false;
            movement.CanDrop = false;
            movement.CanJump = false;
            movement.CanPickUp = false;
            movement.CanSpawnedKnightAttack = false;
            movement.CanSpawnedKnightSwap = false;
        }
        if (gameMaster.horseSavedRoom < 5 && gameMaster.savedWorldName == "World")
        {
            movement.CanDoubleJump = false;
            if(gameMaster.horseSavedRoom< 4)
            {
                movement.CanDrop = false;
                movement.CanSpawnedKnightSwap = false;
                movement.canSpawnedKnightAttack = false;
            }
        }



        if (gameMaster.horseSavedRoom == gameMaster.knightSavedRoom)
        {            
            Transform checkpoint = FindCheckpoint(gameMaster.horseSavedRoom);
            transform.position = checkpoint.position;            
            horseController.KnightPickUp(false);
        }
        else
        {
            Transform checkpoint = FindCheckpoint(gameMaster.horseSavedRoom);
            transform.position = checkpoint.position;

            checkpoint = FindCheckpoint(gameMaster.knightSavedRoom);
            GameObject newKnight = Instantiate(knightPrefab, checkpoint.position, Quaternion.identity);
            newKnight.GetComponent<KnightController2D>().horse = gameObject;
            horseController.spawnedKnight = newKnight;
            horseController.KnightPickedUp = false;
            //if (horseController.spawnedKnight != null)
            //{
            //    Destroy(horseController.spawnedKnight);
            //}
        }
        Collider2D cameraConfiner = FindLevel(gameMaster.horseSavedRoom).Find("CameraBound").GetChild(0).GetComponent<Collider2D>();

        horseCameraConfinerComponent.m_BoundingShape2D = cameraConfiner;
        cameraConfiner = FindLevel(gameMaster.knightSavedRoom).Find("CameraBound").GetChild(0).GetComponent<Collider2D>();
        knightCameraConfinerComponent.m_BoundingShape2D = cameraConfiner;
    }

    public void Start()
    {
        Spawning();
    }    

    public void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
