using System.Linq;
using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    [Header("Prefab à instancier")]
    [SerializeField] private GameObject prefabToSpawn;


    [Header("Paramètres de spawn")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;


    /* ---- PREFABS ---- */
    [Header("Prefabs")]
    [SerializeField] private GameObject lightPrefab;        // « Lampe »
    [SerializeField] private GameObject sensorPrefab;       // « Caméra »

    /* ---- PARAMÈTRES GÉNÉRAUX ---- */
    [Header("Réglages généraux")]
    [Tooltip("Hauteur fixe des lampes au‑dessus du sol")]
    [SerializeField] private float ceilingHeight = 3f;


    [Tooltip("Layer contenant TOUS les murs fixes du niveau")]
    [SerializeField] private LayerMask limitLayer;

    [SerializeField] private float sensorHeight = 1.8f;

    /* ---- COMPTEURS ---- */
    static int lightCount  = 0;
    static int sensorCount = 0;

    public void SpawnLight()
    {
        if (!lightPrefab)
        {
            Debug.LogWarning("[SpawnPrefab] LightPrefab manquant !");
            return;
        }

        // 2) rotation 90° sur X (= projecteur vers le bas)
        Quaternion rot = Quaternion.Euler(90f, 0f, 0f);

        // 3) instanciation + renommage
        GameObject go = Instantiate(lightPrefab, spawnPosition, rot);
        go.name = $"Lumière {++lightCount}";

        // 4) (optionnel) lui donner le tag "Light" pour BuildModeManager
        go.tag = "Light";
    }

    public void SpawnMotionSensor()
    {
        if (!sensorPrefab) { Debug.LogWarning("SensorPrefab manquant"); return; }

        
        Quaternion rot = sensorPrefab.transform.rotation;    

        var go = Instantiate(sensorPrefab, spawnPosition, rot);
        go.name = $"Caméra {++sensorCount}";
        go.tag  = "MotionSensor";
    }

    
    public void SpawnItem()
    {

        if (prefabToSpawn != null)
        {
            
            Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        }
        else
        {
            Debug.LogWarning("Le champ prefabToSpawn n’est pas renseigné dans l’Inspector !");
        }
    }
}
