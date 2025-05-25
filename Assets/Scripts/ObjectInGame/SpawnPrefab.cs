using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class SpawnPrefab : MonoBehaviour
{
    [Header("Prefab à instancier")]
    [SerializeField] private GameObject prefabToSpawn;


    [Header("Paramètres de spawn")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private Vector3 spawnLight = Vector3.zero;
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;


    /* ---- PREFABS ---- */
    [Header("Prefabs")]
    [SerializeField] private GameObject lightPrefab;        // « Lampe »
    [SerializeField] private GameObject sensorPrefab;       // « Caméra »

    /* ---- PARAMÈTRES GÉNÉRAUX ---- */
    [Header("Réglages généraux")]
    // [Tooltip("Hauteur fixe des lampes au‑dessus du sol")]
    // [SerializeField] private float ceilingHeight = 3f;


    [Tooltip("Layer contenant TOUS les murs fixes du niveau")]
    [SerializeField] private LayerMask limitLayer;


    /* ───── HELPERS PRIVÉS ───── */
    /// <summary>
    /// Retourne le prochain index disponible pour un tag et un motif donnés.
    /// Exemple : si la scène contient « Caméra 3 », renvoie 4.
    /// </summary>
    static int NextIndex(string tag, string regex)
    {
        var rx    = new Regex(regex, RegexOptions.IgnoreCase);
        var scene = SceneManager.GetActiveScene();

        var indices =
            Object.FindObjectsOfType<Transform>(true)          // tous les objets
                  .Where(t => t.CompareTag(tag)                 // …ayant ce tag
                           && t.gameObject.scene == scene)      // …dans cette scène
                  .Select(t => rx.Match(t.name))
                  .Where(m => m.Success &&
                              int.TryParse(m.Groups[1].Value, out _))
                  .Select(m => int.Parse(m.Groups[1].Value));
        Debug.Log(FindObjectsOfType<Transform>(true));
        return indices.DefaultIfEmpty(0).Max() + 1;             // suivant
    }

    /* ───── SPAWN LAMPES ───── */
    public void SpawnLight()
    {
        if (!lightPrefab)
        {
            Debug.LogWarning("[SpawnPrefab] LightPrefab manquant !");
            return;
        }

        Quaternion rot = Quaternion.Euler(90f, 0f, 0f);          // projecteur vers le bas
        GameObject go = Instantiate(lightPrefab, spawnLight, rot);
        // Debug.Log(NextIndex());
        int n = NextIndex("Light",  @"Lumière[\u00A0 ]*(\d+)");
        go.name = $"Lumière {n}";
        go.tag  = "Light";
    }

    /* ───── SPAWN CAPTEURS ───── */
    public void SpawnMotionSensor()
    {
        if (!sensorPrefab)
        {
            Debug.LogWarning("[SpawnPrefab] SensorPrefab manquant !");
            return;
        }

        Quaternion rot = sensorPrefab.transform.rotation;
        GameObject go = Instantiate(sensorPrefab, spawnPosition, rot);

        int n = NextIndex("MotionSensor",  @"Caméra[\u00A0 ]*(\d+)");
        go.name = $"Caméra {n}";
        go.tag  = "MotionSensor";
    }

    /* ---- Spawn générique (inchangé) ---- */
    public void SpawnItem()
    {
        if (prefabToSpawn)
            Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        else
            Debug.LogWarning("Le champ prefabToSpawn n’est pas renseigné !");
    }
        
}

