using UnityEngine;
using UnityEngine.UI;

public class WallSpawnAndScale : MonoBehaviour
{
    [Header("Prefab du mur")]
    [SerializeField] private GameObject wallPrefab;

    [Header("UI")]
    [SerializeField] private Canvas scaleCanvas;
    [SerializeField] private Slider scaleSlider;

    private Vector3 spawnPosition = new Vector3(0, 1, 60);

    private GameObject selectedWall;

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
            scaleSlider.maxValue = 50f;  
            scaleSlider.value = 1f;

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
        GameObject newWall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);
       }

    /// <summary>
    /// Appelé quand le slider change de valeur.  
    /// Ajuste uniquement l'échelle X du mur sélectionné.
    /// </summary>
    void OnScaleChanged(float newScale)
    {
        if (!selectedWall) return;

        Vector3 s = selectedWall.transform.localScale;
        s.z = newScale;
        selectedWall.transform.localScale = s;

        if (selectedWall.TryGetComponent(out SnapToNeighbor snap))
            snap.TrySnap();
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

            // scaleSlider.value = xScale;
            scaleSlider.SetValueWithoutNotify(xScale);
        }
    }

    /// <summary>
    /// Appelé par le BuildModeManager quand on désélectionne ou qu’on sélectionne autre chose.
    /// </summary>
    public void DeselectWall()
    {
        selectedWall = null;
    }
}
