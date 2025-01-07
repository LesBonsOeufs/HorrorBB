using UnityEngine;


public static class Extension_Vector3
{
	public static Vector3 FlatXY(this Vector3 vector) => new Vector3(vector.x, vector.y, 0);
	public static Vector3 FlatXZ(this Vector3 vector) => new Vector3(vector.x, 0, vector.z);

	public static Vector2 XY(this Vector3 vector) => new Vector2(vector.x, vector.y);
	public static Vector2 XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

    public static Vector3 Divide(this Vector3 selfVector3, Vector3 vector3)
    {
        return new Vector3(
                selfVector3.x / vector3.x,
                selfVector3.y / vector3.y,
                selfVector3.z / vector3.z);
    }
}