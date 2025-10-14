using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public Button continueButton; // Przypisz w Inspectorze

    private void Start()
    {
        //DeleteSave(); // Usuń plik zapisu przy starcie (do testów)
        string savePath = Application.persistentDataPath + "/save.dat";
        bool hasSave = File.Exists(savePath);
        if (!hasSave)
        {
            DisableContinue();
        }
    }
    private void DisableContinue()
    {
        continueButton.interactable = false;
        SetContinueButtonTextOpacity(120);
    }

    public void DeleteSave()
    {
        string savePath = Application.persistentDataPath + "/save.dat";
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted.");
        }
    }

    public void Continue()
    {
        // Find the GameMaster instance in the scene
        GameMaster gameMaster = FindObjectOfType<GameMaster>();
        if (gameMaster != null)
        {
            gameMaster.Load();
            // Load the saved level
            FindObjectOfType<AudioManager>().Stop("MainMenuMusic");
            FindObjectOfType<AudioManager>().Play("LevelMusic2");
            SceneManager.LoadScene(gameMaster.savedLevelName);
        }
        else
        {
            Debug.LogWarning("GameMaster not found. Cannot continue.");
        }
    }


    private void SetContinueButtonTextOpacity(byte alpha)
    {
        // Pobierz wszystkie komponenty Text (UI) pod przyciskiem
        var texts = continueButton.GetComponentsInChildren<Text>(true);
        foreach (var text in texts)
        {
            var color = text.color;
            color.a = alpha / 255f;
            text.color = color;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level0"); // Load your first level scene
        
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
    }
}
