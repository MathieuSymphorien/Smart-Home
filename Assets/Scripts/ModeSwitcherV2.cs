using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSwitcherV2 : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton;    // Bouton qui déclenche le switch
    [SerializeField] private TMP_Text buttonText;    // Texte sur le bouton
    [SerializeField] private Canvas editorCanvas;    // Le canvas/UI utilisé en mode édition

    [Header("Cameras")]
    [SerializeField] private Camera editCamera;      // Caméra (ou GameObject) pour le mode édition

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;     // Le prefab du joueur
    [SerializeField] private Transform playerSpawnPoint;  // Point de spawn pour le joueur

    // Référence du joueur instancié (s’il existe)
    private GameObject currentPlayer;

    // Pour savoir dans quel mode on est (true => édition ; false => jeu)
    private bool isEditorMode = true;

    private void Start()
    {
        // On rattache la fonction SwitchMode au bouton
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(SwitchMode);
        }

        // On démarre en mode Édition
        SetMode(isEditorMode);
    }

    /// <summary>
    /// Appelé quand on clique sur le bouton.
    /// Bascule le booléen et applique le mode correspondant.
    /// </summary>
    private void SwitchMode()
    {
        // Inverser l’état
        isEditorMode = !isEditorMode;

        // Appliquer le mode
        SetMode(isEditorMode);

        // Mettre à jour le texte du bouton (facultatif)
        if (buttonText != null)
        {
            buttonText.text = isEditorMode 
                ? "Passer en mode Jeu" 
                : "Revenir en mode Édition";
        }
    }

    /// <summary>
    /// Active/désactive la caméra, le canvas, et le joueur
    /// en fonction du mode choisi.
    /// </summary>
    private void SetMode(bool editorModeActive)
    {
        if (editorModeActive)
        {
            // MODE ÉDITION
            // 1) Détruire le joueur s’il existe
            DestroyPlayer();

            // 2) Activer la caméra d’édition
            if (editCamera != null)
                editCamera.gameObject.SetActive(true);

            // 3) Afficher le canvas
            if (editorCanvas != null)
                editorCanvas.gameObject.SetActive(true);
        }
        else
        {
            // MODE JEU
            // 1) Instancier le joueur
            SpawnPlayer();

            // 2) Désactiver la caméra d’édition
            if (editCamera != null)
                editCamera.gameObject.SetActive(false);

            // 3) Masquer l’UI d’édition
            if (editorCanvas != null)
                editorCanvas.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Instancie le joueur s'il n'y en a pas déjà
    /// </summary>
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("PlayerPrefab non assigné dans l’inspector !");
            return;
        }
        if (playerSpawnPoint == null)
        {
            Debug.LogWarning("PlayerSpawnPoint non assigné dans l’inspector !");
            return;
        }
        if (currentPlayer == null)
        {
            currentPlayer = Instantiate(playerPrefab, 
                                        playerSpawnPoint.position, 
                                        playerSpawnPoint.rotation);
            currentPlayer.name = "Player_Instantiated";
        }
    }

    /// <summary>
    /// Détruit le joueur instancié s’il existe
    /// </summary>
    private void DestroyPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
    }
}
