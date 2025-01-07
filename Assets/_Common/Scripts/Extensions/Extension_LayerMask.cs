using UnityEngine;

public static class Extension_LayerMask
{
    public static bool ContainsLayer(this LayerMask layerMask, int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }
}