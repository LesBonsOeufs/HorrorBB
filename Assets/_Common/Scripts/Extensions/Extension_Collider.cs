using UnityEngine;

public static class Extension_Collider
{
    public static void ShapeEnglobingTricks(this EdgeCollider2D edge) 
    {
        PolygonCollider2D lPolygon = edge.gameObject.GetComponent<PolygonCollider2D>();
        lPolygon = lPolygon ?? edge.gameObject.AddComponent<PolygonCollider2D>();

        edge.points = lPolygon.points;
        Object.Destroy(lPolygon);
    }


}