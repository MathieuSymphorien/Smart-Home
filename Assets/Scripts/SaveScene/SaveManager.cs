using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    // Nom du fichier JSON pour stocker toutes les sauvegardes
    private static string saveFileName = "allSaves.json";

    // Liste en mémoire de toutes les sauvegardes
    private SaveDataList saveDataList = new SaveDataList();

    // Méthode pour récupérer le chemin du fichier JSON
    private static string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    private void Awake()
    {
        // On tente de charger le fichier dès le démarrage
        LoadAllSavesFromDisk();
    }

    private void LoadAllSavesFromDisk()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            saveDataList = JsonUtility.FromJson<SaveDataList>(json);

            if (saveDataList == null)
                saveDataList = new SaveDataList();
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
        // On crée une nouvelle sauvegarde
        SaveData newSaveData = new SaveData(saveName);

        // Récupérer tous les objets (par exemple, tagués "Savable") dans la scène
        GameObject[] savableObjects = GameObject.FindGameObjectsWithTag("Savable");
        if (savableObjects.Length == 0)
        {
            Debug.LogWarning("Aucun objet à sauvegarder trouvé dans la scène.");
            return;
        }
        foreach (GameObject obj in savableObjects)
        {
            // On suppose que vous avez un script/une référence pour connaître le nom du prefab
            // Sinon, on peut stocker un 'prefabName' directement dans un composant attaché.
            // Pour l'exemple, on va simplement dire que 'prefabName' = obj.name
            // ou si vous avez un script "PrefabIdentifier" dessus.

            string prefabName = obj.name.Replace("(Clone)", "").Trim();

            // Prépare les données
            Vector3 pos = obj.transform.position;
            Quaternion rot = obj.transform.rotation;
            ObjectSaveData data = new ObjectSaveData(prefabName, pos, rot);

            newSaveData.objects.Add(data);
        }

        // On ajoute cette sauvegarde à la liste globale
        // Si le saveName existe déjà, on peut éventuellement écraser ou empêcher
        // En l'état, on ajoute juste
        saveDataList.allSaves.Add(newSaveData);

        // On enregistre la liste complète sur disque
        SaveAllSavesToDisk();
    }

    public void LoadSaveByName(string saveName)
    {
        // Retrouver la sauvegarde concernée
        SaveData foundSave = saveDataList.allSaves.Find(s => s.saveName == saveName);
        if (foundSave == null)
        {
            Debug.LogWarning($"Aucune sauvegarde avec le nom {saveName}");
            return;
        }

        // Avant de charger, on supprime peut-être les anciens objets dans la scène
        // (selon votre logique, vous pouvez vider la scène des anciens objets).
        ClearCurrentSceneObjects();

        // Pour chaque ObjectSaveData, recréer l'objet
        foreach (ObjectSaveData objData in foundSave.objects)
        {
            // On récupère un prefab depuis Resources ou un autre gestionnaire
            // Supposez que vous stockez vos prefabs dans un dossier Resources
            // et que le prefabName correspond au nom du fichier prefab
            GameObject prefab = Resources.Load<GameObject>(objData.prefabName);
            Debug.Log($"Chargement du prefab : {objData.prefabName}");
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab introuvable : {objData.prefabName}");
                continue;
            }

            // Instancier le prefab
            Vector3 position = new Vector3(objData.posX, objData.posY, objData.posZ);
            Quaternion rotation = new Quaternion(objData.rotX, objData.rotY, objData.rotZ, objData.rotW);
            GameObject newObj = Instantiate(prefab, position, rotation);

            // Facultatif : le tagguer à nouveau "Savable" pour pouvoir le resauvegarder
            newObj.tag = "Savable";
        }
    }

    private void ClearCurrentSceneObjects()
    {
        // Ici vous pourriez détruire tous les objets tagués "Savable" par exemple :
        GameObject[] savableObjects = GameObject.FindGameObjectsWithTag("Savable");
        foreach (GameObject obj in savableObjects)
        {
            Destroy(obj);
        }
    }

    public List<string> GetAllSaveNames()
    {
        return saveDataList.allSaves.Select(s => s.saveName).ToList();
    }


}
