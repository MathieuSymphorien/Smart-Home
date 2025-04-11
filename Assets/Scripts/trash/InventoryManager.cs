using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject[] furniturePrefabs; // Liste des prefabs des meubles
    private GameObject selectedFurniture; // Meuble actuellement sélectionné

    public void SelectFurniture(int index)
    {
        if (index >= 0 && index < furniturePrefabs.Length)
        {
            selectedFurniture = furniturePrefabs[index];
            Debug.Log($"InventoryManager: Selected furniture '{selectedFurniture.name}' at index {index}");
        }
        else
        {
            Debug.LogWarning($"InventoryManager: Invalid index {index}. No furniture selected.");
        }
    }

    public GameObject GetSelectedFurniture()
    {
        if (selectedFurniture != null)
        {
            Debug.Log($"InventoryManager: Returning selected furniture '{selectedFurniture.name}'");
        }
        else
        {
            Debug.LogWarning("InventoryManager: No furniture is currently selected.");
        }
        return selectedFurniture;
    }
}


