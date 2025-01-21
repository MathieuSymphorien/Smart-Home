using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public GridManager gridManager; // Référence au système de grille
    public InventoryManager inventoryManager; // Référence à l'inventaire

    void Update()
    {
        // Vérifier le clic gauche
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("PlacementManager: Mouse click detected. Attempting to place furniture.");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"PlacementManager: Raycast hit {hit.collider.gameObject.name}");

                // Vérifier si l'on clique sur une cellule
                GameObject cell = hit.collider.gameObject;
                if (cell.name.StartsWith("Cell"))
                {
                    Debug.Log($"PlacementManager: Valid cell '{cell.name}' clicked. Proceeding to place furniture.");
                    PlaceFurniture(cell);
                }
                else
                {
                    Debug.LogWarning($"PlacementManager: Clicked object '{cell.name}' is not a valid cell.");
                }
            }
            else
            {
                Debug.LogWarning("PlacementManager: Raycast did not hit any object.");
            }
        }
    }

    void PlaceFurniture(GameObject cell)
    {
        // Obtenir le meuble sélectionné
        GameObject furniture = inventoryManager.GetSelectedFurniture();
        if (furniture != null)
        {
            if (cell.transform.childCount == 0)
            {
                Debug.Log($"PlacementManager: Placing furniture '{furniture.name}' on cell '{cell.name}'");

                // Placer le meuble au centre de la cellule
                GameObject placedFurniture = Instantiate(furniture, cell.transform);
                placedFurniture.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning($"PlacementManager: Cell '{cell.name}' is already occupied. Cannot place furniture.");
            }
        }
        else
        {
            Debug.LogWarning("PlacementManager: No furniture selected. Cannot place anything.");
        }
    }
}
