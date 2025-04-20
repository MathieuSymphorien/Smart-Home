using UnityEngine;
using UnityEngine.UI;

public class WallSpawnAndScale : MonoBehaviour
{
    [Header("Prefab du mur")]
    [SerializeField] private GameObject wallPrefab;

    [Header("UI")]
    [SerializeField] private Canvas scaleCanvas;   // Canvas/Panel contenant le Slider
    [SerializeField] private Slider scaleSlider;   // Le Slider pour ajuster l'échelle X

    // Position fixe où le mur doit apparaître
    private Vector3 spawnPosition = new Vector3(0, 1, 60);

    // L'objet "mur" actuellement sélectionné (si aucun => null)
    private GameObject selectedWall;

    /*  tout en haut de la classe  */
    private float lastSafeScaleX = 1f;


    private void Start()
    {
        // On masque le Canvas au démarrage
        // if (scaleCanvas != null)
        //     scaleCanvas.gameObject.SetActive(false);

        // Configure le slider (min=0.1, max=50, default=1) pour une plus grande amplitude
        if (scaleSlider != null)
        {
            scaleSlider.minValue = 0.1f;
            scaleSlider.maxValue = 50f;   // <-- plus grand qu'avant
            scaleSlider.value = 1f;

            // Écoute l’événement 'valeur changée' du slider
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
        }
    }

    /// <summary>
    /// Appelé depuis un bouton (UI) pour instancier le mur à la position voulue
    /// </summary>
    public void SpawnWall()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("Aucun prefab de mur assigné !");
            return;
        }

        // Instancier le mur à (0,1,60)
        GameObject newWall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Mur instancié : {newWall.name} à {spawnPosition}");

        // NOTE : À ce stade, on ne “sélectionne” pas forcément ce mur.
        // C'est le BuildModeManager qui gère la sélection quand on clique dessus.
        // Si vous voulez *automatiquement* sélectionner ce nouveau mur,
        // vous pouvez soit l'indiquer à BuildModeManager, soit appeler "SelectWall(newWall)" ici.
    }

    /// <summary>
    /// Appelé quand le slider change de valeur.  
    /// Ajuste uniquement l'échelle X du mur sélectionné.
    /// </summary>
    void OnScaleChanged(float newScale)
    {
        if (!selectedWall) return;

        // 1) on applique toujours la valeur demandée
        Vector3 s = selectedWall.transform.localScale;
        s.x = newScale;
        selectedWall.transform.localScale = s;

        // 2) si le mur a un composant WallSnap, force un snap éventuel
        if (selectedWall.TryGetComponent(out SnapToNeighbor snap))
            snap.TrySnap();
    }


    bool IsScaleColliding(GameObject wall, Vector3 testScale)
    {
        if (!wall.TryGetComponent(out Collider col)) return false;

        /* --- calcule la future bounding‑box --- */
        // centre = même qu’actuel
        Vector3 center = col.bounds.center;

        // demi‑extents : on part de l’extent courant puis
        // on remplace uniquement l’axe X par le futur
        Vector3 half = col.bounds.extents;
        float  ratio = testScale.x / wall.transform.localScale.x;
        half.x *= ratio;

        Quaternion rot = wall.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, half, rot);

        foreach (var h in hits)
        {
            if (h == col) continue;                // on s’ignore soi‑même
            if (h.CompareTag("Limit")) return true;
        }
        return false;
    }



    /// <summary>
    /// Appelé par le BuildModeManager lorsqu’un mur est sélectionné.
    /// Active le slider et met selectedWall = mur cliqué.
    /// </summary>
    public void SelectWall(GameObject wall)
    {
        lastSafeScaleX = wall.transform.localScale.x;
        if (wall == null) return;

        selectedWall = wall;
        
        // On affiche le canvas et on règle la valeur du slider
        if (scaleCanvas != null)
            scaleCanvas.gameObject.SetActive(true);

        // Mettre le slider à la valeur X actuelle du mur
        if (scaleSlider != null)
        {
            float xScale = selectedWall.transform.localScale.x;
            // Clamp si jamais en dehors des bornes
            if (xScale < scaleSlider.minValue) xScale = scaleSlider.minValue;
            if (xScale > scaleSlider.maxValue) xScale = scaleSlider.maxValue;

            scaleSlider.value = xScale;
        }

        Debug.Log("WallSpawnAndScale => Mur sélectionné : " + wall.name);
    }

    /// <summary>
    /// Appelé par le BuildModeManager quand on désélectionne ou qu’on sélectionne autre chose.
    /// </summary>
    public void DeselectWall()
    {
        selectedWall = null;

        // On masque le slider
        // if (scaleCanvas != null)
        //     scaleCanvas.gameObject.SetActive(false);

        // Debug.Log("WallSpawnAndScale => Aucun mur sélectionné");
    }


     /// <summary>
    /// Détecte si l'objet entre en collision avec un mur "Limit"
    /// </summary>
    private bool IsCollidingWithLimit(GameObject obj)
    {
        Collider objCol = obj.GetComponent<Collider>();
        if (objCol == null) return false;

        Vector3 center = objCol.bounds.center;
        Vector3 halfExtents = objCol.bounds.extents;
        Quaternion rotation = obj.transform.rotation;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation);

        foreach (Collider c in hits)
        {
            if (c == objCol) continue; // ignore self
            if (c.CompareTag("Limit"))
            {
                return true;
            }
        }
        return false;
    }
}
