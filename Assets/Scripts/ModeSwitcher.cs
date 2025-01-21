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

        // Mettre à jour le texte du bouton
        buttonText.text = editorModeActive ? "Passer en mode Jeu" : "Passer en mode Éditeur";

        // Logs pour déboguer
        Debug.Log(editorModeActive ? "Mode éditeur activé" : "Mode jeu activé");
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
}
