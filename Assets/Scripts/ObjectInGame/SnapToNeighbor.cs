using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class SnapToNeighbor : MonoBehaviour
{
    [Header("Paramètres snap")]
    public float  snapDistance = 0.2f;
    public string[] targetTags = { "Wall", "MotionSensor", "Savable", "Limit" };

    [Header("Debug")]
    public bool debug = true;                 // ← coche cette case dans l’Inspector

    BoxCollider col;
    Vector3 lastPos, lastScale, lastRot;
    int layerMask;

    void Awake ()
    {
        col       = GetComponent<BoxCollider>();
        layerMask = Physics.DefaultRaycastLayers;
        SaveState();
    }

    void LateUpdate ()
    {
        if (!HasMoved()) return;

        TrySnap();
        SaveState();
    }

    /* ───────────── SNAP ─────────────────────────── */

    public void TrySnap ()
    {
        Vector3 half = Vector3.Scale(col.size * 0.5f, transform.lossyScale) +
                       Vector3.one * snapDistance;
        Vector3 cen  = transform.TransformPoint(col.center);
        Quaternion rot = transform.rotation;

        Collider[] hits = Physics.OverlapBox(
        cen, half, rot, layerMask, QueryTriggerInteraction.Collide);  // ← Collide = inclut les triggers

        // if (debug)
        // {
        //     Debug.Log($"[{name}] OverlapBox centre={cen}  half={half}  rotY={transform.eulerAngles.y}");
        //     if (hits.Length == 0) Debug.Log("   → aucun collider touché !");
        //     foreach (Collider h in hits)
        //         Debug.Log($"   touche {h.name}  tag={h.tag}  trigger={h.isTrigger}");
        // }

        // Collider[] hits = Physics.OverlapBox(cen, half, rot, layerMask,
        //                                      QueryTriggerInteraction.Ignore);

        float   bestSq   = float.PositiveInfinity;
        Vector3 bestDisp = Vector3.zero;
        Collider bestCol = null;

        foreach (Collider other in hits)
        {
            if (other == col) continue;
            if (!targetTags.Contains(other.tag)) continue;

            /* ---------- A) cas chevauchement -------------------- */
            if (Physics.ComputePenetration(
                    col,   transform.position, transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out Vector3 penDir, out float penDist))
            {
                Vector3 disp = penDir * penDist; disp.y = 0f;

                // if (debug)
                //     Debug.Log($"  Overlap avec {other.name}  dist={penDist:F4}  dir={penDir}  disp={disp}");

                float sq = disp.sqrMagnitude;
                if (sq < bestSq && penDist <= snapDistance)
                {
                    bestSq   = sq;
                    bestDisp = disp;
                    bestCol  = other;
                }
            }
            else
            {
                /* ---------- B) cas encore un gap ----------------- */
                Vector3 pA = col.ClosestPoint(other.transform.position);
                Vector3 pB = other.ClosestPoint(pA);
                Vector3 delta = pB - pA; delta.y = 0f;
                float gap = delta.magnitude;

                // if (debug)
                //     Debug.Log($"  Gap avec {other.name}  gap={gap:F4}  delta={delta}");

                if (gap < 1e-4f || gap > snapDistance) continue;

                float sq = gap * gap;
                if (sq < bestSq)
                {
                    bestSq   = sq;
                    bestDisp = delta;
                    bestCol  = other;
                }
            }
        }

        if (bestSq < float.PositiveInfinity)
        {
            transform.position += bestDisp;

            // if (debug)
            //     Debug.Log($"→ Snap sur {bestCol.name}  déplacement appliqué = {bestDisp}\n");
        }
        // else if (debug)
        //     Debug.Log("→ Aucun voisin dans la plage de snap\n");
    }

    /* ─── utilitaires ───────────────────────────── */

    bool HasMoved() =>
        transform.position   != lastPos   ||
        transform.localScale != lastScale ||
        transform.eulerAngles!= lastRot;

    void SaveState()
    {
        lastPos   = transform.position;
        lastScale = transform.localScale;
        lastRot   = transform.eulerAngles;
    }
}
