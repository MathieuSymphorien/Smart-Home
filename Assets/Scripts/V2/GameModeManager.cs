using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;      // assigner dans l'inspecteur
    [SerializeField] private Camera editCamera;      // assigner dans l'inspecteur
    [SerializeField] private Canvas editModeCanvas;  // UI du mode édition
    private bool isEditMode = false;

    public void ToggleEditMode()
    {
        isEditMode = !isEditMode;

        // Activer/Désactiver la caméra d'édition
        editCamera.gameObject.SetActive(isEditMode);

        // Activer/Désactiver la caméra de jeu
        gameCamera.gameObject.SetActive(!isEditMode);

        // Activer/Désactiver l'UI du mode édition
        editModeCanvas.gameObject.SetActive(isEditMode);

        // Si nécessaire, mettre le temps en pause ou autre
        // Time.timeScale = isEditMode ? 0f : 1f;
    }
}
