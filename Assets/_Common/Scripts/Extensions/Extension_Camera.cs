using UnityEngine;

public static class Extension_Camera
{
    public static bool IsPointVisible(this Camera camera, Vector3 point)
    {
        // Convert world point to viewport point
        Vector3 viewportPoint = camera.WorldToViewportPoint(point);

        // Check if the point is within the camera's viewport
        bool inViewport = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                          viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                          viewportPoint.z > 0;

        if (inViewport)
        {
            // Perform raycast to check for obstacles
            RaycastHit hit;
            Vector3 direction = point - camera.transform.position;
            if (Physics.Raycast(camera.transform.position, direction, out hit))
            {
                // Check if the raycast hit the point or something closer
                return hit.distance >= Vector3.Distance(camera.transform.position, point);
            }
            return true; // No obstacles found
        }

        return false; // Point is outside the viewport
    }
}