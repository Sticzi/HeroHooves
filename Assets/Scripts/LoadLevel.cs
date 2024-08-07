using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    public int levelIndex;
    public void Load()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + levelIndex);
    }    
}
