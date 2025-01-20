using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject[] placeableObjects; // Liste des objets à placer
    public Tilemap tilemap;               // La Tilemap où placer les objets
    public Camera mainCamera;             // La caméra principale

    private GameObject selectedObject;    // L'objet actuellement sélectionné

    void Update()
    {
        // Sélection de l'objet à placer via les touches numériques
        for (int i = 0; i < placeableObjects.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedObject = placeableObjects[i];
                Debug.Log($"Objet sélectionné : {selectedObject.name}");
            }
        }

        // Placement de l'objet sélectionné
        if (selectedObject != null && Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = tilemap.WorldToCell(mouseWorldPos);

            // Ajouter l'objet à la Tilemap
            PlaceObjectOnGrid(gridPosition);
        }
    }

    void PlaceObjectOnGrid(Vector3Int gridPosition)
    {
        // Place un objet à la position de la grille
        GameObject obj = Instantiate(selectedObject);
        obj.transform.position = tilemap.CellToWorld(gridPosition) + new Vector3(0.5f, 0.5f, 0); // Centré sur la cellule
    }
}

