using UnityEngine;
using System.Linq;

/// <summary>
/// Colle automatiquement cet objet au voisin le plus proche si la
/// distance entre leurs bords X ou Z est &lt; snapDistance.
/// Tags ciblés : "Wall" et "Furniture" (modifiable dans l’Inspector).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SnapToNeighbor : MonoBehaviour
{
    [Tooltip("Seuil d'accroche (mètres)")]
    public float snapDistance = 0.2f;

    [Tooltip("Tags d’objets auxquels on peut se coller")]
    public string[] targetTags = { "Wall", "MotionSensor", "Savable", "Limit" };

    BoxCollider col;
    Vector3 lastPos, lastScale, lastRot;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        SaveState();
    }

    void LateUpdate()
    {
        if (!HasMoved()) return;      // inutile de tester chaque frame

        TrySnap();
        SaveState();
    }

    /* ───────── SNAP CORE ───────── */

    public void TrySnap()
    {
        var dirs = new[]              // four directions principales
        {
            transform.right,   -transform.right,
            transform.forward, -transform.forward
        };

        foreach (var dir in dirs)
            SnapAlong(dir);
    }

    void SnapAlong(Vector3 dir)
    {
        Vector3 center = col.bounds.center;
        Vector3 half   = col.bounds.extents;

        // Point d’origine = bord dans la direction 'dir'
        Vector3 origin = center + Vector3.Scale(half, dir.normalized);

        if (!Physics.Raycast(origin, dir, out RaycastHit hit, snapDistance))
            return;

        if (hit.collider == col) return;                     // ignore self
        if (!targetTags.Contains(hit.collider.tag)) return;  // mauvais tag

        transform.position += dir * hit.distance;            // colle pile
    }

    /* ───────── UTILS ───────── */

    bool HasMoved() =>
        transform.position != lastPos ||
        transform.localScale != lastScale ||
        transform.eulerAngles != lastRot;

    void SaveState()
    {
        lastPos   = transform.position;
        lastScale = transform.localScale;
        lastRot   = transform.eulerAngles;
    }
}
