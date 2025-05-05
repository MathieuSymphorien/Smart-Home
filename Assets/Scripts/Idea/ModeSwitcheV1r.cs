using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSwitcherV1 : MonoBehaviour
{
    public Button switchButton;         // Bouton pour changer de mode
    public TMP_Text buttonText;         // Texte du bouton

    private bool isEditorMode = true;   // Le mode initial est l'éditeur

    // Listes pour stocker les références des éléments
    private List<GameObject> editorElements = new List<GameObject>();
    private List<GameObject> gameElements = new List<GameObject>();

    [Header("Grille 2D pour les meubles")]
    public GridLayoutGroup grid2D;      // Référence à la grille 2D (GridLayoutGroup)

    [Header("Grille 2D pour les Murs")]
    public GridLayoutGroup wallsGrid2D;
    
    [Header("Vue 3D")]
    public Transform grid3DContainer;   // Conteneur pour les objets 3D
    public GameObject default3DPrefab;  // Prefab par défaut pour représenter les items 3D
    public GameObject wall3DPrefab;    // Prefab pour les murs 3D
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
    // (1) Vider les anciens objets 3D
    foreach (Transform child in grid3DContainer)
    {
        Destroy(child.gameObject);
    }

    // *** PARTIE MEUBLES ***
    InstancierObjets2DSurLeSol(grid2D);

    // *** PARTIE MURS ***
    InstancierObjets2DSurLeSol(wallsGrid2D);
}

// Nouvelle méthode "générique" qui prend en paramètre la grille à scanner
void InstancierObjets2DSurLeSol(GridLayoutGroup gridToScan)
{
    // 1) Récupérer position/échelle du sol
    Vector3 floorPos   = floor3D.transform.position;
    Vector3 floorScale = floor3D.transform.localScale;

    float floorLength = floorScale.x;  // axe X
    float floorDepth  = floorScale.z;  // axe Z
    float floorMinX   = floorPos.x - floorLength / 2f;
    float floorMinZ   = floorPos.z - floorDepth  / 2f;

    // 2) Nombre de slots dans la grille
    int totalSlots   = gridToScan.transform.childCount;
    int nbColonnes   = gridToScan.constraintCount;
    int nbLignes     = Mathf.CeilToInt(totalSlots / (float)nbColonnes);

    // 3) Calculer la taille d'une "parcelle"
    float parcelSizeX = floorLength / nbColonnes;
    float parcelSizeZ = floorDepth  / nbLignes;

    // 4) Parcourir tous les slots 2D
    for (int slotIndex = 0; slotIndex < totalSlots; slotIndex++)
    {
        Transform slot2D = gridToScan.transform.GetChild(slotIndex);

        if (slot2D.childCount > 0)
        {
            // Récupérer l'item 2D
            GameObject item2D = slot2D.GetChild(0).gameObject;

            // Calculer la colonne/ligne
            int col = slotIndex % nbColonnes;
            int row = slotIndex / nbColonnes;
            row = (nbLignes - 1) - row; 

            // Position 3D
            float posX = floorMinX + col * parcelSizeX + parcelSizeX * 0.5f;
            float posZ = floorMinZ + row * parcelSizeZ + parcelSizeZ * 0.5f;
            float posY = floorPos.y + 1.48f; // À adapter selon la hauteur souhaitée

            Vector3 item3DPosition = new Vector3(posX, posY, posZ);

            // Instancier le bon prefab 3D
            GameObject item3D;
            if (item2D.CompareTag("Wall"))
            {
                // Si c’est un mur, on utilise le prefab de mur
                item3D = Instantiate(wall3DPrefab, item3DPosition, Quaternion.identity, grid3DContainer);
                AdjustWallScale(item3D, col, row, parcelSizeX, parcelSizeZ, nbColonnes, nbLignes, slotIndex, gridToScan);
            }
            else
            {
                // Sinon on instancie le prefab par défaut
                item3D = Instantiate(default3DPrefab, item3DPosition, Quaternion.identity, grid3DContainer);
            }

            // Optionnel : renommer l’objet pour debug
            // item3D.name = item2D.name + "_3D";
        }
    }
}



     /// <summary>
    /// Ajuste l'échelle des murs pour qu'ils soient collés les uns aux autres
    /// </summary>
   void AdjustWallScale(
    GameObject wall3D,
    int col, int row,
    float parcelSizeX, float parcelSizeZ,
    int nbColonnes, int nbLignes,
    int slotIndex,
    GridLayoutGroup gridToScan
)
{
    // Vérif des voisins dans gridToScan
    Transform gridTransform = gridToScan.transform;

    bool hasLeftWall = false;
    bool hasRightWall = false;
    bool hasTopWall = false;
    bool hasBottomWall = false;

    // Gauche
    if (col > 0)
    {
        Transform leftSlot = gridTransform.GetChild(slotIndex - 1);
        if (leftSlot.childCount > 0 && leftSlot.GetChild(0).CompareTag("Wall"))
        {
            hasLeftWall = true;
        }
    }
    // Droite
    // ... pareil pour top/bas

    // Ajustement final
    Vector3 wallScale = wall3D.transform.localScale;
    if (hasLeftWall || hasRightWall)
    {
        wallScale.x = parcelSizeX * 2;
    }
    if (hasTopWall || hasBottomWall)
    {
        wallScale.z = parcelSizeZ * 2;
    }
    wall3D.transform.localScale = wallScale;
}

}
