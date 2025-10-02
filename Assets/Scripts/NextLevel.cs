using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private GameMaster gameMaster;
    public Vector2 BackGroundPos;

    public int nextRoom;
    public string nextWorld;
    public string nextLevel;

    public bool isOn = false;

    public void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9 && isOn && collision.GetComponent<HorseController2D>() != null)
        {
            if (collision.GetComponent<HorseController2D>().KnightPickedUp)
            {
                gameMaster.knightSavedRoom = nextRoom;
                gameMaster.horseSavedRoom = nextRoom;
                gameMaster.savedWorldName = nextWorld;
                gameMaster.savedLevelName = nextLevel;
                gameMaster.Save();
                TransitionToNewWorld();
            }
        }
    }

    public void TransitionToNewWorld()
    {
        SceneManager.LoadSceneAsync(nextLevel).completed += _ =>
        {
            // Reinitialize game elements in new scene
            if (gameMaster != null)
            {                
                
                gameMaster.Load();

            }
        };
    }
}
