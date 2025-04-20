using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public string saveName;
    public List<ObjectSaveData> objects;

    public SaveData(string name)
    {
        saveName = name;
        objects = new List<ObjectSaveData>();
    }
}

