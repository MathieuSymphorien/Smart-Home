using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente une entrée (un "slot") liant un nom et l'objet Canvas/panel correspondant dans la scène.
/// </summary>
[System.Serializable]
public class CanvasEntry
{
    public string canvasName;     // ex: "Mur", "Meuble", "Décor", ...
    public GameObject canvasObject;  // Le GameObject qui contient le GridLayoutGroup, etc.
}

/// <summary>
/// Composant permettant de switcher entre plusieurs "Canvas"/"Panel"
/// (chacun contenant un GridLayoutGroup ou autre).
/// </summary>
public class CanvasSwitcher : MonoBehaviour
{
    [Tooltip("Liste des Canvas disponibles (à renseigner dans l'inspecteur).")]
    public List<CanvasEntry> canvases = new List<CanvasEntry>();

    // Pour mémoriser le Canvas actuellement actif (optionnel si tu veux y accéder)
    private string activeCanvasName;

    void Start()
    {
        // Au démarrage, on peut décider d'activer un Canvas par défaut
        // (ici, le premier de la liste, s'il y en a au moins un).
        if (canvases.Count > 0)
        {
            ShowCanvas(canvases[0].canvasName);
        }
    }

    /// <summary>
    /// Active le Canvas dont le nom correspond à 'canvasName'
    /// et désactive tous les autres.
    /// </summary>
    public void ShowCanvas(string canvasName)
    {
        foreach (CanvasEntry entry in canvases)
        {
            bool shouldEnable = (entry.canvasName == canvasName);
            if (entry.canvasObject != null)
            {
                entry.canvasObject.SetActive(shouldEnable);
            }

            if (shouldEnable)
            {
                activeCanvasName = canvasName;
            }
        }
    }

    public void OnClickShowWalls()
    {
        ShowCanvas("Mur");
    }

    public void OnClickShowFurniture()
    {
        ShowCanvas("Meuble");
    }
}
