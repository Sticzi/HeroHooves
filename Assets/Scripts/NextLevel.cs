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

                //gameMaster.savedBackgroundPos[1] = BackGroundPos;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
}
