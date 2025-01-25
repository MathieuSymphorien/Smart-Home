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

    [Header("Grille 2D")]
    public GridLayoutGroup grid2D;      // Référence à la grille 2D (GridLayoutGroup)
    
    [Header("Vue 3D")]
    public Transform grid3DContainer;   // Conteneur pour les objets 3D
    public GameObject default3DPrefab;  // Prefab par défaut pour représenter les items 3D
    public GameObject floor3D;          // Référence au sol 3D

    [Header("Player Spawning")]
    public GameObject playerPrefab;      // Le prefab du joueur à instancier
    public Transform playerSpawnPoint;   // Point de spawn (position/rotation) pour le joueur

    // Variable pour stocker le joueur instancié (quand on passe en mode jeu)
    private GameObject currentPlayer;

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

        // S'il est possible, détruire ou instancier le joueur avant d'activer/désactiver les éléments
        if (!isEditorMode)
        {
            // On passe EN mode jeu => on instancie le joueur
            SpawnPlayer();
        }
        else
        {
            // On repasse en mode éditeur => on détruit le joueur actuel
            DestroyPlayer();
        }

        // Mettre à jour l'affichage des éléments
        SetMode(isEditorMode);

        // Mettre à jour le texte du bouton (facultatif)
        if (buttonText != null)
        {
            buttonText.text = isEditorMode 
                ? "Switch to Game Mode" 
                : "Switch to Editor Mode";
        }
    }

    void SetMode(bool editorModeActive)
    {
        // Activer/Désactiver les éléments du mode éditeur
        ToggleElements(editorElements, editorModeActive);

        // Activer/Désactiver les éléments du mode jeu
        ToggleElements(gameElements, !editorModeActive);
        
        // Quand on passe EN mode jeu, on traduit la grille 2D vers la 3D
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

    /// <summary>
    /// Instancie le joueur à l'emplacement défini par playerSpawnPoint
    /// </summary>
    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("playerPrefab is not assigned in the inspector!");
            return;
        }
        if (playerSpawnPoint == null)
        {
            Debug.LogWarning("playerSpawnPoint is not assigned in the inspector!");
            return;
        }

        // Instancier le joueur
        currentPlayer = Instantiate(
            playerPrefab, 
            playerSpawnPoint.position, 
            playerSpawnPoint.rotation
        );

        // (Optionnel) Renommer pour clarté
        currentPlayer.name = "Player_Instantiated";
    }

    /// <summary>
    /// Détruit le joueur si on en a un
    /// </summary>
    void DestroyPlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }
    }

    /// <summary>
    /// Traduit la grille 2D vers le 3D en instanciant des items 3D
    /// </summary>
    void Translate2DTo3D()
    {
        // 1) Vider d'abord les anciens objets 3D (pour éviter les doublons).
        foreach (Transform child in grid3DContainer)
        {
            Destroy(child.gameObject);
        }

        // 2) Récupérer la position et l'échelle du sol
        Vector3 floorPosition = floor3D.transform.position;
        Vector3 floorScale    = floor3D.transform.localScale;

        float floorLength = floorScale.x;  // axe X
        float floorDepth  = floorScale.z;  // axe Z
        float floorMinX   = floorPosition.x - floorLength / 2f;
        float floorMinZ   = floorPosition.z - floorDepth  / 2f;

        // 3) Déterminer combien de slots 2D on a
        int totalSlots = grid2D.transform.childCount;

        int nbColonnes = grid2D.constraintCount;
        int nbLignes   = Mathf.CeilToInt(totalSlots / (float)nbColonnes);

        // 4) Calculer la taille d'une "parcelle"
        float parcelSizeX = floorLength / nbColonnes;
        float parcelSizeZ = floorDepth  / nbLignes;

        // 5) Parcourir tous les slots 2D
        for (int slotIndex = 0; slotIndex < totalSlots; slotIndex++)
        {
            Transform slot2D = grid2D.transform.GetChild(slotIndex);

            // Vérifier si le slot contient un item
            if (slot2D.childCount > 0)
            {
                GameObject item2D = slot2D.GetChild(0).gameObject;

                // 6) Calculer la colonne et la ligne
                int col = slotIndex % nbColonnes;
                int row = slotIndex / nbColonnes;
                row = (nbLignes - 1) - row;
                // 7) Position 3D
                float posX = floorMinX + col * parcelSizeX + parcelSizeX * 0.5f;
                float posZ = floorMinZ + row * parcelSizeZ + parcelSizeZ * 0.5f;
                float posY = floorPosition.y + 1.48f;

                Vector3 item3DPosition = new Vector3(posX, posY, posZ);

                // 8) Instancier l'objet 3D
                GameObject item3D = Instantiate(default3DPrefab, item3DPosition, Quaternion.identity, grid3DContainer);
                // item3D.name = item2D.name + "_3D";
            }
        }
    }
}
