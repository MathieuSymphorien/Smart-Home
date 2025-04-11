using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    [Header("Prefab à instancier")]
    [SerializeField] private GameObject prefabToSpawn;

    [Header("Paramètres de spawn")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;

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
