using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera editCamera;   // La caméra top-down activée en mode édition
    [SerializeField] private LayerMask selectableLayer; // Le layer sur lequel on a nos objets
    [SerializeField] private LayerMask groundLayer;     // Si besoin de détecter un sol/plan

    [SerializeField] private WallSpawnAndScale wallScaleManager;

    [SerializeField] private LayerMask wallLayer;   // couche des murs (tags Wall | Limit)
    [SerializeField] private float ceilingHeight = 3f;


    [SerializeField] private float lightHeight  = 3.0f;
[SerializeField] private float sensorHeight = 1.8f;

    private Vector3 lastSafePos;      // dernière position ne touchant pas un mur
private Vector3 lastMouseOnGround; // pour ajuster l’offset


    private SelectableObject currentlySelectedObject;
    private bool isDragging = false;
    private Vector3 dragOffset;  // offset entre le point cliqué et le pivot de l’objet
    private bool isInBuildMode;

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

        // quand on appuie sur TAB, par exemple
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool build = !isInBuildMode;
            isInBuildMode = build;

            BuildModeEvents.OnBuildMode?.Invoke(build);
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
                lastSafePos      = so.transform.position;
                lastMouseOnGround = hit.point; 
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
void DragSelectedObject()
{
    Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out var hit, 500f, groundLayer)) return;

    /* 1) Nouveau centre candidat ------------------------------------ */
    Vector3 newPos = hit.point + dragOffset;

    /* --------- Hauteurs fixes --------- */
    if (currentlySelectedObject.CompareTag("Light"))
        newPos.y = lightHeight;
    else if (currentlySelectedObject.CompareTag("MotionSensor"))
        newPos.y = sensorHeight;
    else if (currentlySelectedObject.TryGetComponent(out Collider col))
        newPos.y = col.bounds.extents.y;

    /* 2) Collision ? -------------------------------------------------- */
    if (IsCollidingWithLimitAt(newPos))
    {
        // on se replace PROPREMENT à la dernière position valide
        currentlySelectedObject.transform.position = lastSafePos;

        // et on recalcule l’offset pour suivre correctement la souris
        dragOffset = lastSafePos - lastMouseOnGround;
        return;
    }

    /* 3) Tout va bien  →  on valide et on mémorise */
    currentlySelectedObject.transform.position = newPos;
    if (currentlySelectedObject.TryGetComponent(out SnapToNeighbor snap))
        snap.TrySnap();
    lastSafePos      = newPos;
    lastMouseOnGround = hit.point;
}

private bool IsCollidingWithLimitAt(Vector3 pos)
{
    Collider objCol = currentlySelectedObject.GetComponent<Collider>();
    if (!objCol) return false;

    Vector3 half = objCol.bounds.extents;
    Quaternion rot = currentlySelectedObject.transform.rotation;

    Collider[] hits = Physics.OverlapBox(pos, half, rot);
    foreach (var c in hits)
    {
        if (c == objCol) continue;
        if (c.CompareTag("Limit") || c.CompareTag("Wall"))
            return true;
    }
    return false;
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

}
