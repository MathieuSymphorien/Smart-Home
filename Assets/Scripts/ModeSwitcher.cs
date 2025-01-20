using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSwitcher : MonoBehaviour
{
    public GameObject editorMode; // Le mode éditeur (2D)
    public GameObject gameMode;   // Le mode jeu (3D)
    public Button switchButton;   // Bouton pour changer de mode
    public TMP_Text buttonText;       // Texte du bouton

    private bool isEditorMode = true; // Définir le mode initial

    void Start()
    {
        // Assigner la fonction de switch au bouton
        switchButton.onClick.AddListener(SwitchMode);

        // Activer le mode éditeur au démarrage
        SetMode(isEditorMode);
    }

    void SwitchMode()
    {
        // Inverser le mode actuel
        isEditorMode = !isEditorMode;

        // Mettre à jour les modes
        SetMode(isEditorMode);
    }

    void SetMode(bool editorModeActive)
    {
        // Activer/désactiver les GameObjects en fonction du mode
        editorMode.SetActive(editorModeActive);
        gameMode.SetActive(!editorModeActive);

        // Mettre à jour le texte du bouton
        buttonText.text = editorModeActive ? "Passer en mode Jeu" : "Passer en mode Éditeur";
    }
}
