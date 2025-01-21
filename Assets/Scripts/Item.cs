using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine. EventSystems;
using UnityEngine.UI;


public class Item : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [Header("UI")]
    public Image image;

    [HideInInspector] public Transform parentAfterDrag;
    // Drag and drop
    public void OnBeginDrag(PointerEventData eventData) {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData) {
    transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        // Vérifie si l'item a été relâché sur un slot ou sur la poubelle
        GameObject dropTarget = eventData.pointerEnter; // Objet sous la souris
        if (dropTarget != null && dropTarget.CompareTag("Trash"))
        {
            // Détruire l'item si déposé sur la poubelle
            Destroy(gameObject);
        }
        else
        {
            // Replace l'item dans son parent d'origine si pas sur la poubelle
            transform.SetParent(parentAfterDrag);
        }
    }
}