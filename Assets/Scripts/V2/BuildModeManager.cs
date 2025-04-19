using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera editCamera;   // La caméra top-down activée en mode édition
    [SerializeField] private LayerMask selectableLayer; // Le layer sur lequel on a nos objets
    [SerializeField] private LayerMask groundLayer;     // Si besoin de détecter un sol/plan

    [SerializeField] private WallSpawnAndScale wallScaleManager;

    private SelectableObject currentlySelectedObject;
    private bool isDragging = false;
    private Vector3 dragOffset;  // offset entre le point cliqué et le pivot de l’objet

    void Update()
    {
        // 1) Détecter un clic gauche enfoncé (down)
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsClickOverUI())
            {
                OnLeftClickDown();
            }
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
        // On sauvegarde l’ancienne position
        Vector3 oldPos = currentlySelectedObject.transform.position;

        Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
        {
            // Nouvelle position
            Vector3 newPos = hit.point + dragOffset;

            // Ajuster la hauteur, si nécessaire
            Collider col = currentlySelectedObject.GetComponent<Collider>();
            if (col != null)
            {
                float halfHeight = col.bounds.extents.y;
                newPos.y = halfHeight;
            }

            // Appliquer temporairement la nouvelle position
            currentlySelectedObject.transform.position = newPos;

            // Vérifier la collision avec un mur "Limit"
            bool isCollidingLimit = IsCollidingWithLimit(currentlySelectedObject.gameObject);
            if (isCollidingLimit)
            {
                // Revenir à l’ancienne position
                currentlySelectedObject.transform.position = oldPos;
                Debug.Log("Déplacement bloqué par un mur Limit : revert position.");
            }
        }
    }


    private void SelectObject(SelectableObject newSelection)
    {
        currentlySelectedObject = newSelection;
        currentlySelectedObject.SetSelected(true);
        // Si l'objet a un "mur" (tag "Wall" ou autre),
        // on avertit WallSpawnAndScale pour activer le slider.
        // A vous de définir comment détecter que c’est un mur. Exemples :

        // 1) Vérifier un tag :
        if (currentlySelectedObject.CompareTag("Wall"))
        {
            wallScaleManager.SelectWall(currentlySelectedObject.gameObject);
        }
        else
        {
            // Ce n’est pas un mur => on désactive le slider
            wallScaleManager.DeselectWall();
        }
    }

    private void DeselectCurrent()
    {
        if (currentlySelectedObject != null)
        {
            currentlySelectedObject.SetSelected(false);
            currentlySelectedObject = null;
        }
        wallScaleManager.DeselectWall();
    }

    /// <summary>
    /// Vérifie si la souris est sur un élément UI (EventSystem).
    /// </summary>
    private bool IsClickOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }


    /// <summary>
    /// Renvoie true si l'objet entre en collision avec un mur "Limit".
    /// </summary>
    private bool IsCollidingWithLimit(GameObject obj)
    {
        Collider objCol = obj.GetComponent<Collider>();
        if (objCol == null) return false; // pas de collider => on ne bloque pas

        // On récupère le centre et les demi-extents de la bounding box
        Vector3 center = objCol.bounds.center;
        Vector3 halfExtents = objCol.bounds.extents;
        Quaternion rotation = obj.transform.rotation;

        // On récupère tous les colliders dans cette OverlapBox
        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation);

        foreach (Collider c in hits)
        {
            // On ignore le propre collider de l'objet
            if (c == objCol) continue;

            // Si c'est un mur limit
            if (c.CompareTag("Limit"))
            {
                return true;
            }
        }
        return false;
    }

    // Autre alternative : on pourrait faire un 
    // if (objCol.bounds.Intersects(c.bounds)) ...
}
