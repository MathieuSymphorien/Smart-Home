using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public string saveName;  // Nom unique ou label pour distinguer la sauvegarde
    public List<ObjectSaveData> objects;

    public SaveData(string name)
    {
        saveName = name;
        objects = new List<ObjectSaveData>();
    }
}
