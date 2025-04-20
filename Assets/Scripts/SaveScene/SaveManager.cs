using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    private static string saveFileName = "allSaves.json";
    private SaveDataList saveDataList = new SaveDataList();

    private static string GetSaveFilePath() =>
        Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        LoadAllSavesFromDisk();
    }

    private void LoadAllSavesFromDisk()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            saveDataList = JsonUtility.FromJson<SaveDataList>(json) ?? new SaveDataList();
        }
        else
        {
            saveDataList = new SaveDataList();
        }
    }

    private void SaveAllSavesToDisk()
    {
        string path = GetSaveFilePath();
        string json = JsonUtility.ToJson(saveDataList, true);
        File.WriteAllText(path, json);
    }

    public void CreateNewSave(string saveName)
    {
        var newSaveData = new SaveData(saveName);

        var tagsToSave = new[] { "Savable", "Wall" };
        var objectsToSave = new List<GameObject>();
        foreach (string tag in tagsToSave)
            objectsToSave.AddRange(GameObject.FindGameObjectsWithTag(tag));

        if (objectsToSave.Count == 0)
        {
            Debug.LogWarning("Aucun objet à sauvegarder trouvé (Savable ou wall).");
            return;
        }

        foreach (GameObject obj in objectsToSave)
        {
            string prefabName   = obj.name.Replace("(Clone)", "").Trim();
            string originalTag  = obj.tag;
            Vector3 pos         = obj.transform.position;
            Quaternion rot      = obj.transform.rotation;
            Vector3 scale       = obj.transform.localScale;

            var data = new ObjectSaveData(
                prefabName,
                originalTag,
                pos, rot,
                scale
            );
            newSaveData.objects.Add(data);
        }

        saveDataList.allSaves.Add(newSaveData);
        SaveAllSavesToDisk();
    }

    public void LoadSaveByName(string saveName)
    {
        var foundSave = saveDataList.allSaves.Find(s => s.saveName == saveName);
        if (foundSave == null)
        {
            Debug.LogWarning($"Aucune sauvegarde avec le nom {saveName}");
            return;
        }

        ClearCurrentSceneObjects();

        foreach (var objData in foundSave.objects)
        {
            GameObject prefab = Resources.Load<GameObject>(objData.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab introuvable : {objData.prefabName}");
                continue;
            }

            Vector3 position = new Vector3(objData.posX, objData.posY, objData.posZ);
            Quaternion rotation = new Quaternion(objData.rotX, objData.rotY, objData.rotZ, objData.rotW);
            Vector3 scale = new Vector3(objData.scaleX, objData.scaleY, objData.scaleZ);

            GameObject newObj = Instantiate(prefab, position, rotation);
            newObj.tag = objData.originalTag;
            newObj.transform.localScale = scale;
        }
    }

    private void ClearCurrentSceneObjects()
    {
        var tagsToClear = new[] { "Savable", "Wall" };
        foreach (string tag in tagsToClear)
        {
            var objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var o in objs)
                Destroy(o);
        }
    }

    public List<string> GetAllSaveNames() =>
        saveDataList.allSaves.Select(s => s.saveName).ToList();
}