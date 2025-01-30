using Root;
using System.Collections.Generic;
using UnityEngine;

public static class PathSmoothing
{
    public static List<GraphPoint> Execute(List<GraphPoint> path, E_SmoothingMethod method = E_SmoothingMethod.BASIC, 
        int iterations = 5, float smoothFactor = 0.5f)
    {
        if (path == null || path.Count <= 2)
            return path;

        List<GraphPoint> lSmoothedPath = new List<GraphPoint>(path);

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            for (int i = 1; i < lSmoothedPath.Count - 1; i++)
            {
                switch (method)
                {
                    case E_SmoothingMethod.BASIC:
                        Basic(lSmoothedPath, i, smoothFactor);
                        break;
                    case E_SmoothingMethod.COLLISION_BINARY:
                        Binary(lSmoothedPath, i, smoothFactor);
                        break;
                    case E_SmoothingMethod.COLLISION_ADAPTIVE:
                        Adaptive(lSmoothedPath, i, smoothFactor);
                        break;
                    default:
                        break;
                }
            }
        }

        return lSmoothedPath;
    }

    #region Methods

    private static void Basic(List<GraphPoint> path, int index, float smoothFactor)
    {
        Vector3 lPrevPos = path[index - 1].position;
        Vector3 lCurrentPos = path[index].position;
        Vector3 lNextPos = path[index + 1].position;

        Vector3 lMidPoint = (lPrevPos + lNextPos) / 2f;
        Vector3 lSmoothedPos = Vector3.Lerp(lCurrentPos, lMidPoint, smoothFactor);

        // Project the smoothed position onto the normal plane of the current point
        Plane lNormalPlane = new Plane(path[index].normal, lCurrentPos);
        Vector3 lProjectedPos = lNormalPlane.ClosestPointOnPlane(lSmoothedPos);

        path[index] = new GraphPoint(lProjectedPos, path[index].normal);
    }

    private static void Binary(List<GraphPoint> path, int index, float smoothFactor)
    {
        Vector3 lPrevPos = path[index - 1].position;
        Vector3 lCurrentPos = path[index].position;
        Vector3 lNextPos = path[index + 1].position;

        Vector3 lMidPoint = (lPrevPos + lNextPos) / 2f;
        Vector3 lSmoothedPos = Vector3.Lerp(lCurrentPos, lMidPoint, smoothFactor);

        // Project the smoothed position onto the normal plane of the current point
        Plane lNormalPlane = new Plane(path[index].normal, lCurrentPos);
        Vector3 lProjectedPos = lNormalPlane.ClosestPointOnPlane(lSmoothedPos);

        // Perform collision check for the point and the segments
        if (!Physics.Linecast(lCurrentPos, lProjectedPos) &&
            !Physics.Linecast(path[index - 1].position, lProjectedPos) &&
            !Physics.Linecast(lProjectedPos, path[index + 1].position))
        {
            path[index] = new GraphPoint(lProjectedPos, path[index].normal);
        }
        // If collision detected, keep the original position
    }

    private static void Adaptive(List<GraphPoint> path, int index, float smoothFactor)
    {
        float lCurrentSmoothFactor = smoothFactor;
        Vector3 lCurrentPos;
        Vector3 lProjectedPos;

        do
        {
            Vector3 lPrevPos = path[index - 1].position;
            lCurrentPos = path[index].position;
            Vector3 lNextPos = path[index + 1].position;

            Vector3 lMidPoint = (lPrevPos + lNextPos) / 2f;
            Vector3 lSmoothedPos = Vector3.Lerp(lCurrentPos, lMidPoint, lCurrentSmoothFactor);

            // Project the smoothed position onto the normal plane of the current point
            Plane lNormalPlane = new Plane(path[index].normal, lCurrentPos);
            lProjectedPos = lNormalPlane.ClosestPointOnPlane(lSmoothedPos);
            lCurrentSmoothFactor *= 0.5f;
        } 
        while ((Physics.Linecast(lCurrentPos, lProjectedPos) ||
                Physics.Linecast(path[index - 1].position, lProjectedPos) ||
                Physics.Linecast(lProjectedPos, path[index + 1].position)) && lCurrentSmoothFactor > 0.01f);

        if (lCurrentSmoothFactor > 0.01f)
            path[index] = new GraphPoint(lProjectedPos, path[index].normal);
    }

    #endregion

    /// <summary>
    /// Methods are per-point
    /// </summary>
    public enum E_SmoothingMethod
    {
        /// <summary>
        /// No additional work
        /// </summary>
        BASIC,

        /// <summary>
        /// Collision prevents smoothing
        /// </summary>
        COLLISION_BINARY,

        /// <summary>
        /// Reduces smoothing until no collision
        /// </summary>
        COLLISION_ADAPTIVE,
    }
}