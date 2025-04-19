using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotGeneral : MonoBehaviour, IDropHandler
{
    // Référence au prefab de l'item
    public GameObject itemPrefab;

    void Update()
    {
        // Vérifie si le slot est vide et s'il appartient à la toolbar
        if (transform.childCount == 0 && CompareTag("ToolbarSlot"))
        {
            // Crée un nouvel item dans le slot
            Instantiate(itemPrefab, transform);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Vérifie que le slot est vide avant d'accepter l'item
        if (transform.childCount == 0)
        {
            // Déplace l'item dans ce slot
            Item inventoryItem = eventData.pointerDrag.GetComponent<Item>();
            inventoryItem.parentAfterDrag = transform;
        }
    }
}
