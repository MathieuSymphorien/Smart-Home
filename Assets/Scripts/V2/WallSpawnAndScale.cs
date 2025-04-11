using UnityEngine;
using UnityEngine.UI;

public class WallSpawnAndScale : MonoBehaviour
{
    [Header("Prefab du mur")]
    [SerializeField] private GameObject wallPrefab;

    [Header("UI")]
    [SerializeField] private Canvas scaleCanvas;   // Canvas/Panel contenant le Slider
    [SerializeField] private Slider scaleSlider;   // Le Slider pour ajuster l'échelle X

    // Référence au mur fraîchement instancié
    private GameObject currentSpawnedWall;

    // Position fixe où le mur doit apparaître
    private Vector3 spawnPosition = new Vector3(0, 1, 60);

    private void Start()
    {
        // On masque le Canvas au démarrage (si nécessaire)
        if (scaleCanvas != null)
            scaleCanvas.gameObject.SetActive(false);

        // Config du slider (ex. min=0.1, max=5, default=1)
        if (scaleSlider != null)
        {
            scaleSlider.minValue = 0.1f;
            scaleSlider.maxValue = 5f;
            scaleSlider.value = 1f;

            // On écoute l'événement 'valeur changée' du slider
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
        currentSpawnedWall = Instantiate(wallPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"Mur instancié : {currentSpawnedWall.name} à {spawnPosition}");

        // Afficher le slider pour ajuster l'échelle
        if (scaleCanvas != null)
            scaleCanvas.gameObject.SetActive(true);

        // Remet la valeur du slider à 1
        if (scaleSlider != null)
            scaleSlider.value = 1f;
    }

    /// <summary>
    /// Méthode déclenchée quand on bouge le slider.
    /// Ajuste uniquement l'échelle X du mur.
    /// </summary>
    private void OnScaleChanged(float newScaleValue)
    {
        if (currentSpawnedWall != null)
        {
            // Récupère l'échelle actuelle
            Vector3 currentScale = currentSpawnedWall.transform.localScale;
            // On ne modifie que le X, on garde le Y/Z initiaux
            currentScale.x = newScaleValue;
            currentSpawnedWall.transform.localScale = currentScale;
        }
    }
}
