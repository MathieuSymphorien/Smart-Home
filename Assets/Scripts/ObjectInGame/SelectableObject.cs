using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SelectableObject : MonoBehaviour
{
    private Color originalColor;
    private bool isSelected;

    private Renderer[] renderers;

    private void Awake()
    {
        // Récupère tous les Renderer pour changer la couleur
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            originalColor = renderers[0].material.color;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateColor();
    }

    private void UpdateColor()
    {
        Color newColor = isSelected ? Color.yellow : originalColor;
        foreach (Renderer rend in renderers)
        {
            rend.material.color = newColor;
        }
    }
}
