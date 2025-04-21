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
    private string rootDir;
    private string exportsDir;
    public  static string ImportsDir { get; private set; }   // exposé au reste du code

    /* ----------------- INITIALISATION ----------------- */
    private void Awake()
    {
        /* 1. Définir les chemins **maintenant** (autorisé) */
        rootDir    = Path.Combine(Application.persistentDataPath, "Saves");
        exportsDir = Path.Combine(rootDir, "Exports");
        ImportsDir = Path.Combine(rootDir, "Imports");

        /* 2. Créer les dossiers si besoin */
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(exportsDir);
        Directory.CreateDirectory(ImportsDir);

        /* 3. Charger la base JSON existante */
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

        var tagsToSave = new[] { "Savable", "Wall", "Light", "MotionSensor"};
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
        var tagsToClear = new[] { "Savable", "Wall", "Light", "MotionSensor" };
        foreach (string tag in tagsToClear)
        {
            var objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (var o in objs)
                Destroy(o);
        }
    }

    public bool DeleteSave(string saveName)
    {
        int removed = saveDataList.allSaves.RemoveAll(s => s.saveName == saveName);
        if (removed > 0)
        {
            SaveAllSavesToDisk();      // on ré‑écrit allSaves.json sans l’entrée supprimée
            return true;
        }
        Debug.LogWarning($"Aucune sauvegarde appelée « {saveName} » n’a été trouvée.");
        return false;
    }


    public List<string> GetAllSaveNames() =>
        saveDataList.allSaves.Select(s => s.saveName).ToList();

    
    public void OverwriteSave(string saveName)
{
    var existing = saveDataList.allSaves.FirstOrDefault(s => s.saveName == saveName);
    if (existing == null)
    {
        CreateNewSave(saveName);          // n’existait pas ? on crée
        return;
    }

    existing.objects.Clear();             // on remplit à nouveau
    var tags = new[] { "Savable", "Wall", "Light", "MotionSensor" };
    foreach (string tag in tags)
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            existing.objects.Add(new ObjectSaveData(
                obj.name.Replace("(Clone)", "").Trim(),
                obj.tag,
                obj.transform.position,
                obj.transform.rotation,
                obj.transform.localScale));

    SaveAllSavesToDisk();
}

public string ExportSaveBare(string saveName)
{
    var save = saveDataList.allSaves.FirstOrDefault(s => s.saveName == saveName);
    if (save == null) { Debug.LogWarning($"Pas de sauvegarde « {saveName} »."); return null; }

    string json   = JsonUtility.ToJson(save, true);
    string target = Path.Combine(exportsDir, $"{saveName}.json");

    File.WriteAllText(target, json);
    Debug.Log($"Exporté vers : {target}");

    // Ouvre le dossier contenant le fichier pour que l’utilisateur puisse le récupérer
    Application.OpenURL($"file://{exportsDir}");
    return target;
}

public bool ImportSaveBare(string fileNameWithoutPath)
{
    string path = Path.Combine(ImportsDir, fileNameWithoutPath);
    if (!File.Exists(path)) { Debug.LogWarning($"Fichier absent : {path}"); return false; }

    string json       = File.ReadAllText(path);
    SaveData imported = JsonUtility.FromJson<SaveData>(json);
    if (imported == null || string.IsNullOrEmpty(imported.saveName))
    {
        Debug.LogWarning("Format de sauvegarde invalide.");
        return false;
    }

    int i = saveDataList.allSaves.FindIndex(s => s.saveName == imported.saveName);
    if (i >= 0) saveDataList.allSaves[i] = imported;   // remplace
    else        saveDataList.allSaves.Add(imported);   // ajoute

    SaveAllSavesToDisk();
    Debug.Log($"Importé : {imported.saveName}");
    return true;
}


}