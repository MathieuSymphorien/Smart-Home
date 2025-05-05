using UnityEngine;

public class SelectionUIManager : MonoBehaviour
{
    [Header("Panneaux spécifiques")]
    [SerializeField] private GameObject wallPanel;
    [SerializeField] private GameObject sensorPanel;

    [Header("Général")]
    [SerializeField] private GameObject genericPanel;

    public void ShowForTag(string tag)
    {
        // Tout cacher d’abord
        wallPanel   ?.SetActive(false);
        sensorPanel ?.SetActive(false);
        genericPanel?.SetActive(false);

        // Afficher celui qui correspond
        switch (tag)
        {
            case "Wall":
                wallPanel   ?.SetActive(true);
                genericPanel?.SetActive(true);
                break;
            case "MotionSensor":
                sensorPanel ?.SetActive(true);
                genericPanel?.SetActive(true);
                break;
            default:
                // autre objet sélectionnable : juste le panneau générique
                genericPanel?.SetActive(true);
                break;
        }
    }

    public void HideAll()
    {
        wallPanel   ?.SetActive(false);
        sensorPanel ?.SetActive(false);
        genericPanel?.SetActive(false);
    }
}
