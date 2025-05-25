using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera editCamera;   // La caméra top-down activée en mode édition
    [SerializeField] private LayerMask selectableLayer; // Le layer sur lequel on a nos objets
    [SerializeField] private LayerMask groundLayer;     // Si besoin de détecter un sol/plan

    [SerializeField] private SelectionUIManager uiManager;
    [SerializeField] private WallSpawnAndScale wallScaleManager;

    [SerializeField] private LayerMask wallLayer;   // couche des murs (tags Wall | Limit)
    // [SerializeField] private float ceilingHeight = 3f;


    public float lightHeight  = 3.0f;
    public float sensorHeight = 1.8f;

    private Vector3 lastSafePos;      // dernière position ne touchant pas un mur
private Vector3 lastMouseOnGround; // pour ajuster l’offset


    private SelectableObject currentlySelectedObject;
    private bool isDragging = false;
    private Vector3 dragOffset;  // offset entre le point cliqué et le pivot de l’objet
    private bool isInBuildMode;


    void Start()
    {
        DeselectCurrent();
    }

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

        // ctrl C pour dupliquer
        if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            DuplicateCurrentSelection();
        // ctrl X pour supprimer
        if (Input.GetKeyDown(KeyCode.X) && Input.GetKey(KeyCode.LeftControl))
            DeleteCurrentSelection();

    }

    private void OnLeftClickDown()
    {
        // Lancer un raycast vers la scène
        Ray ray = editCamera.ScreenPointToRay(Input.mousePosition);

        // Vérifier si on touche un objet sélectionnable
        if (Physics.Raycast(ray, out RaycastHit hit,
                    500f, selectableLayer,
                    QueryTriggerInteraction.Ignore))
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

    // hauteur = moitié de la taille Y du collider principal
    Collider physCol = MainCollider(currentlySelectedObject.gameObject);
    if (physCol)
    {
        newPos.y = physCol.bounds.extents.y;
    }

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
    {
        snap.TrySnap();
        newPos = currentlySelectedObject.transform.position; 
    }
    lastSafePos      = newPos;
    lastMouseOnGround = hit.point;
}

// -----------------------------------------------------------------
// renvoie le 1ᵉʳ Collider non‑trigger trouvé (Box, Capsule, Mesh…)
// -----------------------------------------------------------------
private static Collider MainCollider(GameObject go)
{
    foreach (var c in go.GetComponents<Collider>())
        if (!c.isTrigger) return c;          // ← on ignore SphereCollider trigger

    return go.GetComponent<Collider>();      // fallback (au cas où)
}


private bool IsCollidingWithLimitAt(Vector3 pos)
{
    Collider objCol = MainCollider(currentlySelectedObject.gameObject);
    if (!objCol) return false;

    Vector3 half = objCol.bounds.extents;
    Quaternion rot = currentlySelectedObject.transform.rotation;

    // OverlapBoxes SANS TRIGGER
    Collider[] hits = Physics.OverlapBox(pos, half, rot,
                                        ~0,
                                        QueryTriggerInteraction.Ignore);

    foreach (var c in hits)
    {
        if (c == objCol) continue;
        if (c.CompareTag("Limit") || c.CompareTag("Wall"))
            return true;
    }
    return false;

}

public void DeleteCurrentSelection()
{
    if (currentlySelectedObject == null) return;

    GameObject toDestroy = currentlySelectedObject.gameObject;
    DeselectCurrent();
    Destroy(toDestroy);
}


public void DuplicateCurrentSelection()
{
    if (currentlySelectedObject == null) return;

    GameObject original = currentlySelectedObject.gameObject;

    // On instancie le clone juste à côté pour qu’il ne se superpose pas
    Vector3 offset = Vector3.right * 0.5f;      // décale de 0,5 m à droite
    GameObject clone = Instantiate(
        original,
        original.transform.position + offset,
        original.transform.rotation,
        original.transform.parent);              // garde la même hiérarchie

    // Nouveau nom unique
    clone.name = GenerateUniqueName(original.name);

    // Sélection du clone
    DeselectCurrent();
    SelectObject(clone.GetComponent<SelectableObject>());
}


private string GenerateUniqueName(string originalName)
{
    // Séparation du texte et du suffixe numérique éventuel
    Match m = Regex.Match(originalName, @"^(.*?)(\d+)?$");
    string basePart = m.Groups[1].Value;
    int    number   = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) + 1 : 1;

    string candidate;
    do
    {
        candidate = $"{basePart}{number}";
        number++;
    }
    // Tant qu'un objet du même nom existe dans la scène, on incrémente
    while (GameObject.Find(candidate) != null);

    return candidate;
}



     private void SelectObject(SelectableObject newSelection)
    {
        currentlySelectedObject = newSelection;
        currentlySelectedObject.SetSelected(true);

        // ---- Mur ? ----
        if (currentlySelectedObject.CompareTag("Wall"))
        {
            wallScaleManager.SelectWall(currentlySelectedObject.gameObject);
            uiManager.ShowForTag("Wall");
        }
        // ---- MotionSensor ? ----
        else if (currentlySelectedObject.CompareTag("MotionSensor"))
        {
            wallScaleManager.DeselectWall();
            uiManager.ShowForTag("MotionSensor");
        }
        else            // autre type
        {
            wallScaleManager.DeselectWall();
            uiManager.ShowForTag("Other");
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
        uiManager.HideAll();
    }

    /// <summary>
    /// Vérifie si la souris est sur un élément UI (EventSystem).
    /// </summary>
    private bool IsClickOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

}
