using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    private static GameMaster instance;

    public GameObject player;
    public int knightSavedRoom;
    public int horseSavedRoom;
    public string savedWorldName;
    public string savedLevelName;

    private string savePath;

    void Awake()
    {
        savePath = Application.persistentDataPath + "/save.dat";

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
            //Load(); // Load existing save when created
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when the player reaches a checkpoint
    public void Save()
    {
        SaveData data = new SaveData()
        {
            knightRoom = knightSavedRoom,
            horseRoom = horseSavedRoom,
            worldName = savedWorldName,
            levelName = savedLevelName
        };

        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(savePath, json);
    }

    public void Load()
    {
        if (System.IO.File.Exists(savePath))
        {
            string json = System.IO.File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            knightSavedRoom = data.knightRoom;
            horseSavedRoom = data.horseRoom;
            savedWorldName = data.worldName;
            savedLevelName = data.levelName;
        }
        else
        {
            // Default starting values (adjust as needed)
            knightSavedRoom = 1;
            horseSavedRoom = 1;
            savedWorldName = "World";
            savedLevelName = "actuall main menu chyba 1";
        }

        SceneManager.LoadSceneAsync(savedLevelName);

    }

    private void OnApplicationQuit()
    {
        Save();
    }

    // Call this when starting a new game
    public void ResetSave()
    {
        knightSavedRoom = 1;
        horseSavedRoom = 1;
        savedWorldName = "World";
        savedLevelName = "actuall main menu chyba 1";
        Save();
    }

    [System.Serializable]
    private class SaveData
    {
        public int knightRoom;
        public int horseRoom;
        public string worldName;
        public string levelName;
    }
}