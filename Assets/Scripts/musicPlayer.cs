using UnityEngine;
using UnityEngine.SceneManagement;

public class musicPlayer : MonoBehaviour
{
    //public int targetSceneIndex; // The scene index that triggers the music    


    //void OnEnable()
    //{        
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}
    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    StartMusic();
        
    //}

    //public void StartMusic()
    //{
    //    targetSceneIndex = SceneManager.GetActiveScene().buildIndex;
    //    Debug.Log(targetSceneIndex);
    //    if (targetSceneIndex == 0)
    //    {
    //        Debug.Log("sieeema");
    //        FindObjectOfType<AudioManager>().Play("MainMenuMusic");            
    //    }
    //    if (targetSceneIndex == 1)
    //    {
    //        Debug.Log("haloo");
    //        FindObjectOfType<AudioManager>().Play("LevelMusic");            
    //    }
    //}
}
