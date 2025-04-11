using UnityEngine;

public class SpawnPrefabByName : MonoBehaviour
{
    [SerializeField] private string prefabName = "CubeTest";  // Nom du prefab (sans extension)
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private Quaternion spawnRotation = Quaternion.identity;

    public void SpawnItem()
    {
        Debug.Log("[SpawnPrefabByName] SpawnItem() called for prefab: " + prefabName);

        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab != null)
        {
            Instantiate(prefab, spawnPosition, spawnRotation);
            Debug.Log("[SpawnPrefabByName] Instantiated prefab: " + prefabName);
        }
        else
        {
            Debug.LogWarning("[SpawnPrefabByName] Prefab '" + prefabName + "' not found in Resources!");
        }
    }
}
