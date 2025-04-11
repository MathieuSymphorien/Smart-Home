using UnityEngine;

public class BuildModeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera editCamera;   // La caméra top-down activée en mode édition
    [SerializeField] private LayerMask selectableLayer; // Le layer sur lequel on a nos objets
    [SerializeField] private LayerMask groundLayer;     // Si besoin de détecter un sol/plan

    private SelectableObject currentlySelectedObject;
    private bool isDragging = false;
    private Vector3 dragOffset;  // offset entre le point cliqué et le pivot de l’objet

    void Update()
    {
        // 1) Détecter un clic gauche enfoncé (down)
        if (Input.GetMouseButtonDown(0))
        {
            OnLeftClickDown();
        }

        // 2) Si on est en dragging, on déplace l’objet
        if (isDragging && currentlySelectedObject != null)
        {
            DragSelectedObject();
        }

        // 3) Relâche clic gauche
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
            }
        }

        // 4) Gérer la rotation via une touche (ex. R pour faire pivoter)
        if (currentlySelectedObject != null)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Pivote de 45° sur l'axe Y
                currentlySelectedObject.transform.Rotate(Vector3.up, 45f);
            }
            // Ou Q/E pour pivoter à gauche/droite
            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentlySelectedObject.transform.Rotate(Vector3.up, -15f);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentlySelectedObject.transform.Rotate(Vector3.up, 15f);
            }
        }
    }

    private void OnLeftClickDown()
    {
        // Lancer un raycast vers la scène
        Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);

        // Vérifier si on touche un objet sélectionnable
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, selectableLayer))
        {
            // Récupérer le script SelectableObject
            SelectableObject so = hit.collider.GetComponentInParent<SelectableObject>();
            if (so != null)
            {
                // Si c'est un autre objet que celui qu'on avait déjà sélectionné,
                // désélectionner l'ancien, sélectionner le nouveau
                if (so != currentlySelectedObject)
                {
                    DeselectCurrent();
                    SelectObject(so);
                }
                // Passer en mode drag
                isDragging = true;

                // Calculer un offset pour éviter que l'objet saute de position
                // ex. offset = posObjet - pointRay
                Vector3 objectPos = so.transform.position;
                dragOffset = objectPos - hit.point;
            }
        }
        else
        {
            // Si on clique dans le vide (pas d'objet), on désélectionne
            DeselectCurrent();
        }
    }

    private void DragSelectedObject()
{
    Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
    {
        Vector3 newPos = hit.point + dragOffset;

        // Récupère la hauteur de l'objet :
        Collider col = currentlySelectedObject.GetComponent<Collider>();
        if (col != null)
        {
            float halfHeight = col.bounds.extents.y;
            newPos.y = halfHeight;
        }

        currentlySelectedObject.transform.position = newPos;
    }
}


    private void SelectObject(SelectableObject newSelection)
    {
        currentlySelectedObject = newSelection;
        currentlySelectedObject.SetSelected(true);
    }

    private void DeselectCurrent()
    {
        if (currentlySelectedObject != null)
        {
            currentlySelectedObject.SetSelected(false);
            currentlySelectedObject = null;
        }
    }
}
