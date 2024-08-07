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

    public void Awake()
    {
        horseCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraHorse").GetComponent<CinemachineConfiner>();
        knightCameraConfinerComponent = GameObject.FindGameObjectWithTag("VirtualCameraKnight").GetComponent<CinemachineConfiner>();
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        horseController = GetComponent<HorseController2D>();
        knightPrefab = horseController.knightPrefab;        
    }        

    public void Start()
    {
        if(gameMaster.savedHorsePosition == gameMaster.savedKnightPosition)
        {
            transform.position = gameMaster.savedKnightPosition;
            horseController.KnightPickUp();            
        }
        else
        {
            transform.position = gameMaster.savedHorsePosition;
            GameObject newKnight = Instantiate(knightPrefab, gameMaster.savedKnightPosition, Quaternion.identity);
            newKnight.GetComponent<KnightController2D>().horse = gameObject;
            horseController.KnightPickedUp = false;
            horseController.spawnedKnight = newKnight;
        }
    }    

    public void Death()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
