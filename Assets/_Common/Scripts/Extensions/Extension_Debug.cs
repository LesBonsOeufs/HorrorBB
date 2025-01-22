using UnityEngine;

public static class Extension_Debug
{
    public static void DrawCross(Vector3 position, float size = 1f, Color color = default, float duration = 0.1f)
    {
        Debug.DrawLine(position + Vector3.left * size, position + Vector3.right * size, color, duration);
        Debug.DrawLine(position + Vector3.down * size, position + Vector3.up * size, color, duration);
        Debug.DrawLine(position + Vector3.back * size, position + Vector3.forward * size, color, duration);
    }
}