using System.Collections.Generic;
using System.Linq;
public static class PrefabNameMap
{
    /* clé Resources.Load   →  nom français générique (sans n°) */
    public static readonly Dictionary<string,string> PrefabToLocal = new()
    {
        ["Light"]        = "Lumière",
        ["MotionSensor"] = "Caméra"
    };

    /* l’inverse pour aller plus vite */
    public static readonly Dictionary<string,string> LocalToPrefab =
        PrefabToLocal.ToDictionary(kv => kv.Value, kv => kv.Key);
}