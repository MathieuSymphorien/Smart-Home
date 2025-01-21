using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSwitcher : MonoBehaviour
{
    public Button switchButton;         // Bouton pour changer de mode
    public TMP_Text buttonText;         // Texte du bouton

    private bool isEditorMode = true;   // Le mode initial est l'éditeur

    // Listes pour stocker les références des éléments
    private List<GameObject> editorElements = new List<GameObject>();
    private List<GameObject> gameElements = new List<GameObject>();

    public GridLayoutGroup grid2D;          // Référence à la grille 2D (GridLayoutGroup)
    public Transform grid3DContainer;      // Conteneur pour les objets 3D
    public GameObject default3DPrefab;     // Prefab par défaut pour représenter les items 3D
    public Vector3 cellSize3D = new Vector3(1, 0, 1); // Taille des cellules dans la grille 3D


    void Start()
    {
        // Associer la fonction SwitchMode au bouton
        switchButton.onClick.AddListener(SwitchMode);

        // Initialiser les listes d'éléments
        InitializeElements();

        // Démarrer en mode éditeur
        SetMode(isEditorMode);
    }

    void InitializeElements()
    {
        // Trouver tous les objets marqués avec les tags correspondants
        editorElements.AddRange(GameObject.FindGameObjectsWithTag("EditorElement"));
        gameElements.AddRange(GameObject.FindGameObjectsWithTag("GameElement"));

        // Debug pour vérifier les objets trouvés
        Debug.Log($"Éléments mode éditeur trouvés : {editorElements.Count}");
        Debug.Log($"Éléments mode jeu trouvés : {gameElements.Count}");
    }

    void SwitchMode()
    {
        // Basculer entre les modes
        isEditorMode = !isEditorMode;

        // Mettre à jour les modes
        SetMode(isEditorMode);
    }

    void SetMode(bool editorModeActive)
    {


        // Activer/Désactiver les éléments du mode éditeur
        ToggleElements(editorElements, editorModeActive);

        // Activer/Désactiver les éléments du mode jeu
        ToggleElements(gameElements, !editorModeActive);
        
        if (!editorModeActive)
        {
            Translate2DTo3D();
        }
    }

    void ToggleElements(List<GameObject> elements, bool active)
    {
        // Activer ou désactiver chaque élément dans la liste
        foreach (GameObject element in elements)
        {
            if (element != null)
            {
                element.SetActive(active);
            }
        }
    }


    void Translate2DTo3D()
{
    // Supprimer les anciens objets 3D
    foreach (Transform child in grid3DContainer)
    {
        Destroy(child.gameObject);
    }

    // Parcourir tous les slots de la grille 2D
    foreach (Transform slot in grid2D.GetComponentInChildren<Transform>())
    {
        // Vérifier si le slot contient un item
        if (slot.childCount > 0)
        {
            // Obtenir l'item dans le slot
            GameObject item2D = slot.GetChild(0).gameObject;

            // Calculer la position 3D correspondante
            Vector3 position3D = new Vector3(
                slot.GetSiblingIndex() % grid2D.constraintCount * cellSize3D.x, // Position X
                0,                                                             // Hauteur Y
                slot.GetSiblingIndex() / grid2D.constraintCount * cellSize3D.z // Position Z
            );

            // Instancier le prefab 3D correspondant
            GameObject item3D = Instantiate(default3DPrefab, position3D, Quaternion.identity, grid3DContainer);

            // Configurer l'item 3D si nécessaire (par exemple, l'échelle ou un matériau)
            item3D.transform.localScale = Vector3.one; // Ajuster l'échelle
        }
    }
}

}
