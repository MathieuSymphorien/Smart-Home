using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Allume / éteint un groupe de Light lorsqu’un objet taggé "Player"
/// entre dans le cône de vision.
/// Le cône est visible pendant le Build‑Mode, masqué en Play‑Mode.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class MotionSensor : MonoBehaviour
{
    /* -------- Réglages vision -------- */
    [Header("Vision")]
    [Range(1, 179)] public float fov = 70f;          // Field‑of‑View en degrés
    public float range = 6f;                         // Portée en mètres
    public LayerMask occluders;                      // Ce qui bouche la vue (murs, meubles)

    /* ---------- LAMPES ---------- */
    [Header("Lights liées")]
    [SerializeField] private List<Light> linkedLights = new();   // 🆕 liste dynamique

    /* -------- Debug -------- */
    [Header("Aperçu du cône")]
    public bool showConeInBuildMode = true;          // visible seulement en Build‑Mode
    [SerializeField] private Material coneMaterial;  // Unlit/Transparent   (couleur ≈ cyan)
    [SerializeField] private int coneSteps = 24;     // résolution de l’arc

    [HideInInspector] public bool highlightLinks;                 // affiché / masqué par l’UI
    public  static  System.Action<MotionSensor> OnSettingsChanged; // event pour prévenir l’UI

    /* -------- Interne -------- */
    SphereCollider trigger;
    readonly HashSet<Transform> playersInRange = new HashSet<Transform>();
    LineRenderer coneLR;

    /* 🆕 — couleur des traits vers les lampes */
    static readonly Color linkColor = new (1f, .4f, 0f, 1f);     // orange

    /* ───── HIGHLIGHT ───── */
    [Header("Debug visuel runtime")]
    [Tooltip("Couleur des traits / cubes quand ce capteur est sélectionné dans l’UI")]
    public Color highlightColor = new(1f, .5f, 0f, 1f);      // orange

    readonly List<LineRenderer> linkLines = new();           // 1 line / light
    readonly Dictionary<Renderer, Color> original = new();   // sauvegarde couleurs
    static readonly Dictionary<Light, LightController> cache = new();
    LightController ControllerFor(Light l)
    {
        if (!l) return null;
        if (!cache.TryGetValue(l, out var c))
            cache[l] = c = l.GetComponent<LightController>() ??
                        l.gameObject.AddComponent<LightController>();
        return c;
    }

    public List<Light> LinkedLights => linkedLights; // expose la liste

    public string[] LinkedLightsNames() =>
    linkedLights.Where(l => l).Select(l => l.name).ToArray();


     public void SetHighlight(bool on)
    {
        if (highlightLinks == on) return;
        highlightLinks = on;

        if (on) EnableHighlight();
        else    DisableHighlight();
    }

    /* --- création des traits + changement de couleur --- */
    void EnableHighlight()
    {
        // 1) Self (cube du sensor)
        TintRenderers(GetComponentsInChildren<Renderer>(), true);

        // 2) Toutes les lampes liées
        foreach (var l in linkedLights)
            if (l) TintRenderers(l.GetComponentsInChildren<Renderer>(), true);

        // 3) Lignes
        BuildLines();
    }

    void DisableHighlight()
    {
        TintRenderers(original.Keys.ToArray(), false);
        original.Clear();

        foreach (var lr in linkLines) if (lr) Destroy(lr.gameObject);
        linkLines.Clear();
    }

    void TintRenderers(IEnumerable<Renderer> rends, bool set)
    {
        foreach (var r in rends)
        {
            if (!r) continue;

            if (set)
            {
                if (!original.ContainsKey(r))
                    original[r] = r.material.color;

                var c = highlightColor;
                c.a = original[r].a;          // on garde l’alpha initial
                r.material.color = c;
            }
            else if (original.TryGetValue(r, out var col))
            {
                r.material.color = col;       // restaure
            }
        }
    }

    void BuildLines()
    {
        foreach (var l in linkedLights)
        {
            if (!l) continue;

            var go = new GameObject($"LinkLine_{l.name}");
            go.transform.SetParent(transform, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace     = true;
            lr.positionCount     = 2;
            lr.widthMultiplier   = 0.025f;
            lr.material          = new Material(Shader.Find("Unlit/Color"))
                                   { color = highlightColor };
            linkLines.Add(lr);
        }
    }

    /* --- mets à jour la position des lignes chaque frame --- */
    void LateUpdate()
{
    if (!highlightLinks) return;

    // Liste désynchronisée ?   (dé‑liaison, objet détruit…)
    if (linkedLights.Count != linkLines.Count ||
        linkedLights.Any(l => l == null))
    {
        RebuildLines();                       // on repart sur une base saine
    }

    for (int i = 0; i < linkLines.Count; ++i)
    {
        var line = linkLines[i];
        var lamp = linkedLights[i];
        if (!line || !lamp) continue;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, lamp.transform.position);
    }
}

    /* ---------- API PUBLIQUE (appelée par l’UI) ---------- */
    LineRenderer CreateLine(Light lamp)
{
    var go = new GameObject($"LinkLine_{lamp.name}");
    go.transform.SetParent(transform, false);

    var lr = go.AddComponent<LineRenderer>();
    lr.useWorldSpace   = true;
    lr.positionCount   = 2;
    lr.widthMultiplier = 0.025f;
    lr.material        = new Material(Shader.Find("Unlit/Color"))
                         { color = highlightColor };
    return lr;
}

void RebuildLines()
{
    // 1. Nettoie
    foreach (var lr in linkLines) if (lr) Destroy(lr.gameObject);
    linkLines.Clear();

    // 2. Reconstruit uniquement pour les lampes encore valides
    linkedLights.RemoveAll(l => l == null);           // retire les null
    foreach (var l in linkedLights)
        linkLines.Add(CreateLine(l));
}

/*  MODIFIE UnlinkLight  ----------------------------------------------- */
public void UnlinkLight(Light l)
{
    if (linkedLights.Remove(l))
    {
        RebuildLines();
        OnSettingsChanged?.Invoke(this);
    }
}

/*  MODIFIE LinkLight  -------------------------------------------------- */
public void LinkLight(Light l)
{
    if (l && !linkedLights.Contains(l))
    {
        linkedLights.Add(l);
        if (highlightLinks) RebuildLines();   // recrée la liste active
        OnSettingsChanged?.Invoke(this);
    }
}


    public void SetRange(float r)
    {
        range = r;  if (trigger) trigger.radius = r;  UpdateCone();
        OnSettingsChanged?.Invoke(this);
    }
    public void SetFov  (float f)
    {
        fov = Mathf.Clamp(f,1,179);               UpdateCone();
        OnSettingsChanged?.Invoke(this);
    }


    /* ---------- INITIALISATION ---------- */
    void Awake()
    {
        trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius    = range;

        if (linkedLights.Count == 0)                                // même fallback qu’avant
            linkedLights = GetComponentsInParent<Light>(true)
                           .Where(l => l.enabled).ToList();

        if (showConeInBuildMode) BuildConeVisual();
    }


    /* ---------- BUILD‑MODE ↔ PLAY‑MODE ---------- */
    void OnEnable()  => BuildModeEvents.OnBuildMode += ToggleCone;
    void OnDisable() => BuildModeEvents.OnBuildMode -= ToggleCone;

    void ToggleCone(bool buildMode)
    {
        if (coneLR) coneLR.enabled = buildMode && showConeInBuildMode;
    }

    /* ---------- DÉTECTION ENTRÉE / SORTIE SPHÈRE ---------- */
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playersInRange.Add(other.transform);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playersInRange.Remove(other.transform);
    }

    /* ---------- BOUCLE DE SURVEILLANCE ---------- */
    void Start() => StartCoroutine(VisionLoop());

    IEnumerator VisionLoop()
    {
        WaitForSeconds wait = new(0.1f);
        bool lastState = false;   // état à la frame précédente
        while (true)
        {
            bool seen = PlayerSeen();
            if (seen != lastState)      
            {
                foreach (var l in linkedLights)
                    ControllerFor(l)?.SensorState(seen);
                lastState = seen;
            }
            yield return wait;
        }

    }

    bool PlayerSeen()
    {
        foreach (var p in playersInRange)
        {
            if (!p) continue; // null si le player a été détruit

            Vector3 dir  = p.position - transform.position;
            float   dist = dir.magnitude;
            float   ang  = Vector3.Angle(transform.forward, dir);

            if (ang > fov * 0.5f) continue;               // hors cône
            if (Physics.Raycast(transform.position, dir.normalized, dist, occluders))
                continue;                                 // mur bloque

            return true;                                  // vu !
        }
        return false;
    }

    /* ---------- VISUEL DU CÔNE (LineRenderer) ---------- */
    /* ---------- BUILD DU VISUEL RUNTIME ---------- */
void BuildConeVisual()
{
    coneLR = gameObject.AddComponent<LineRenderer>();
    coneLR.useWorldSpace  = false;
    coneLR.loop           = true;          // on referme la ligne
    coneLR.widthMultiplier = 0.025f;
    coneLR.positionCount  = coneSteps + 2; // origine + arc + retour
    coneLR.material       = coneMaterial ??
                            new Material(Shader.Find("Unlit/Color"))
                            { color = Color.cyan };

    UpdateCone();
}

void UpdateCone()
{
    float half = fov * 0.5f * Mathf.Deg2Rad;

    // 0 = origine
    coneLR.SetPosition(0, Vector3.zero);

    // 1..coneSteps = arc
    for (int i = 0; i <= coneSteps; ++i)
    {
        float t = i / (float)coneSteps;
        float ang = -half + fov * Mathf.Deg2Rad * t;

        Vector3 dir = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang));
        coneLR.SetPosition(i + 1, dir * range);
    }

    // dernière position = retour à l’origine pour fermer le cône
    coneLR.SetPosition(coneSteps + 1, Vector3.zero);
}


#if UNITY_EDITOR                   // mise à jour live dans l’Inspector
    void OnValidate()
    {
        if (trigger) trigger.radius = range;
        if (coneLR)  UpdateCone();
    }
        // pour Handles

void OnDrawGizmos()
{
    if (Application.isPlaying) return;   // En jeu : c’est le LineRenderer qui s’en charge

    // Choisis ta couleur (ici cyan translucide)
    Color fill  = new Color(0, 1, 1, 0.15f);
    Color border = Color.cyan;

    // Prépare la matrice locale → monde
    Gizmos.matrix = transform.localToWorldMatrix;

    // 1. Dessin plein (optionnel)
    Gizmos.color = fill;
    DrawVisionWedge(fov, range, true);

    // 2. Contour
    Gizmos.color = border;
    DrawVisionWedge(fov, range, false);
}

/// <summary>
/// Dessine un wedge (cône 2D vertical) soit plein, soit filaire.
/// </summary>
void DrawVisionWedge(float fovDeg, float dist, bool solid)
{
    int steps = 32;
    Vector3 origin = Vector3.zero;
    float half = fovDeg * 0.5f * Mathf.Deg2Rad;

    // points périphériques
    Vector3[] pts = new Vector3[steps + 1];
    for (int i = 0; i <= steps; ++i)
    {
        float t = i / (float)steps;
        float ang = -half + fovDeg * Mathf.Deg2Rad * t;
        pts[i] = new Vector3(Mathf.Sin(ang) * dist, 0, Mathf.Cos(ang) * dist);
    }

    if (solid)
    {
        for (int i = 0; i < steps; ++i)
        {
            Gizmos.DrawLine(origin, pts[i]);
            Gizmos.DrawLine(pts[i], pts[i + 1]);
            Gizmos.DrawLine(pts[i + 1], origin);
        }
    }
    else
    {
        // contour avant
        for (int i = 0; i < steps; ++i)
            Gizmos.DrawLine(pts[i], pts[i + 1]);

        // rayons
        Gizmos.DrawLine(origin, pts[0]);
        Gizmos.DrawLine(origin, pts[steps]);
    }
}

#endif
}



