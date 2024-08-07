using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    private GameMaster gameMaster;
    public Vector2 playerPos;
    public Vector2 CameraPos;
    public Vector2 BackGroundPos;

    public void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
    }

    //public void OnTriggerEnter2D(Collider2D collision)
    //{        
    //    if (collision.gameObject.layer == 9)
    //    {
    //        if (collision.GetComponent<HorseController2D>().KnightPickedUp)
    //        {
    //            gameMaster.savedHorsePosition = playerPos;
    //            gameMaster.savedKnightPosition = playerPos;
    //            gameMaster.savedCameraPosition = CameraPos;
    //            gameMaster.savedBackgroundPos[1] = BackGroundPos;
    //            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //        }            
    //    }
    //}
}
