using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSwitcher : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button switchButton;
    [SerializeField] private TMP_Text buttonText; 
    [SerializeField] private Canvas editorCanvas;  

    [Header("Cameras")]
    [SerializeField] private Camera editCamera; 

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;    
    [SerializeField] private Transform playerSpawnPoint; 
    private GameObject currentPlayer;
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
        isEditorMode = !isEditorMode;
        SetMode(isEditorMode);

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
