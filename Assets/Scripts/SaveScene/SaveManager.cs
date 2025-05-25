using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

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
        Debug.Log($"Sauvegarde de {json} sauvegardes dans {path}");
        File.WriteAllText(path, json);
    }

    public void CreateEmptySave(string saveName)
    {
        if (saveDataList.allSaves.Any(s => s.saveName == saveName)) return; // slot déjà présent
        saveDataList.allSaves.Add(new SaveData(saveName));
        SaveAllSavesToDisk();
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

        // SaveManager.cs  – dans CreateNewSave() et OverwriteSave()
    foreach (GameObject obj in objectsToSave)
    {
        string tag          = obj.tag;
        string prefabKey    = FindPrefabKey(obj);
        string instanceName = obj.name;              

        string[] links = null;
        if (tag == "MotionSensor")
        {
            var sensor = obj.GetComponent<MotionSensor>();
            links = sensor ? sensor.LinkedLightsNames() : null;   // petite méthode helper
        }

        float r = obj.TryGetComponent(out MotionSensor s) ? s.range : 0f;
        float f = obj.TryGetComponent(out MotionSensor s2) ? s2.fov   : 0f;

        newSaveData.objects.Add(
            new ObjectSaveData(prefabKey, instanceName, tag,
                            obj.transform.position,
                            obj.transform.rotation,
                            obj.transform.localScale,
                            r, f,links)
        );
    }


        saveDataList.allSaves.Add(newSaveData);
        SaveAllSavesToDisk();
    }

    public void LoadSaveByName(string saveName)
    {
        var nameToGO = new Dictionary<string, GameObject>();
        var foundSave = saveDataList.allSaves.Find(s => s.saveName == saveName);
        if (foundSave == null)
        {
            Debug.LogWarning($"Aucune sauvegarde avec le nom {saveName}");
            return;
        }

        ClearCurrentSceneObjects();
        foreach (var d in foundSave.objects)
        {
            GameObject prefab = Resources.Load<GameObject>(d.prefabKey);
            if (!prefab) { Debug.LogWarning($"Prefab {d.prefabKey} introuvable"); continue; }

            var go = Instantiate(prefab,
                                new Vector3(d.posX, d.posY, d.posZ),
                                new Quaternion(d.rotX, d.rotY, d.rotZ, d.rotW));

            go.name = d.instanceName;          // remet le nom FR + numéro
            go.tag = d.originalTag;
            go.transform.localScale = new Vector3(d.scaleX, d.scaleY, d.scaleZ);
            if (d.originalTag == "MotionSensor")
            {
                var sensor = go.GetComponent<MotionSensor>();
                if (sensor != null)
                {
                    sensor.SetRange(d.range);
                    sensor.SetFov(d.fov);
                }
            }
            nameToGO[go.name] = go;
        }

        /* ----- 2ᵉ passage : re-création des liens capteur → lampes ----- */
        foreach (var d in foundSave.objects.Where(o => o.linkedLightNames != null))
        {
            if (!nameToGO.TryGetValue(d.instanceName, out var sensorGO)) continue;
            var sensor = sensorGO.GetComponent<MotionSensor>();
            if (!sensor) continue;

            foreach (string lampName in d.linkedLightNames)
                if (nameToGO.TryGetValue(lampName, out var lampGO))
                    sensor.LinkLight(lampGO.GetComponent<Light>());
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

    public void OverwriteCurrentSave()
    {
        OverwriteSave(SaveGameHolder.saveNameToLoad);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public List<string> GetAllSaveNames() =>
        saveDataList.allSaves.Select(s => s.saveName).ToList();

    
   public void OverwriteSave(string slot)
{
    if (string.IsNullOrEmpty(slot))
    {
        Debug.LogWarning("✖ OverwriteSave : nom de slot vide");
        return;
    }

    /* récupère ou crée l’entrée ---------- */
    var save = saveDataList.allSaves.FirstOrDefault(s => s.saveName == slot);
    if (save == null)
    {
        save = new SaveData(slot);
        saveDataList.allSaves.Add(save);
    }
    save.objects.Clear();

    /* collecte des objets ---------------- */
    string[] tags = { "Savable", "Wall", "Light", "MotionSensor" };
    foreach (string t in tags)
    foreach (GameObject obj in GameObject.FindGameObjectsWithTag(t))
    {
        string prefabKey = FindPrefabKey(obj);

        string[] links = null;
        if (obj.tag == "MotionSensor")
        {
            var sensor = obj.GetComponent<MotionSensor>();
            links = sensor ? sensor.LinkedLightsNames() : null;
        }

        float r = obj.TryGetComponent(out MotionSensor s) ? s.range : 0f;
        float f = obj.TryGetComponent(out MotionSensor s2) ? s2.fov   : 0f;
        
        save.objects.Add(new ObjectSaveData(
            prefabKey,
            obj.name,               // instanceName
            obj.tag,
            obj.transform.position,
            obj.transform.rotation,
            obj.transform.localScale,
            r, f,links));
    }

    Debug.Log($"→ {save.objects.Count} objets enregistrés dans le slot « {slot} »");
    SaveAllSavesToDisk();
}

// helper unique, réutilisé dans CreateNewSave() et OverwriteSave()
static string FindPrefabKey(GameObject obj)
{
    // 1. retire "(Clone)" + espaces superflus
    string raw = obj.name.Replace("(Clone)", "").Trim();

    // 2. découpe avant le premier espace (y compris insécable)
    string shortName = raw.Split(new[] { ' ', '\u00A0' }, 2,
                          System.StringSplitOptions.RemoveEmptyEntries)[0];

    // 3. traduit si c’est une lampe ou un capteur
    return PrefabNameMap.LocalToPrefab.TryGetValue(shortName, out var key)
           ? key              // "Lumière"  →  "Light"
           : shortName;       // sinon on garde le nom réduit : "Wall", "CubeTest", …
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