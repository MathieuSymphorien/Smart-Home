using UnityEngine;

public class GameSceneLoader : MonoBehaviour
{
    public SaveManager saveManager;
    void Start()
    {
        string saveToLoad = SaveGameHolder.saveNameToLoad;
        if (!string.IsNullOrEmpty(saveToLoad))
        {
            // On accède au SaveManager qui est dans la scène
            SaveManager manager = FindObjectOfType<SaveManager>();
            manager.LoadSaveByName(saveToLoad);

            SaveGameHolder.saveNameToLoad = "";
        }
    }
}