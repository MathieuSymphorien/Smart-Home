using UnityEngine;

public class GameSceneLoader : MonoBehaviour
{
    public SaveManager saveManager;
    // GameSceneLoader.cs
    void Start()
    {
        string saveToLoad = SaveGameHolder.saveNameToLoad;
        // Debug.Log($"GameSceneLoader: {saveToLoad}");
        if (!string.IsNullOrEmpty(saveToLoad))
        {
            // Debug.Log("dans le if");
            SaveManager manager = FindObjectOfType<SaveManager>();
            manager.LoadSaveByName(saveToLoad);
        }
    }

}