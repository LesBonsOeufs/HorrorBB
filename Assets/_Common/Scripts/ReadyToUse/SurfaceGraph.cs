using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Root
{
    //Generates world representation:
    //All points must be on a surface, and evenly spread from each other
    //All neighboring points must not have a collider between them, and be closer to each other than around pointsSpacing * 1.5
    public class SurfaceGraph : Singleton<SurfaceGraph>
    {
        [SerializeField, Tag] private string addVerticesTag = "SurfaceGraph_AddVertices";
        [SerializeField] private float size = 15f;
        [SerializeField] private float pointsSpacing = 0.5f;
        
        private PointOctree<GraphPoint> pointOctree;

        public GraphPoint GetClosestPoint(Vector3 position, float maxDistance)
        {
            GraphPoint[] lNearbyPoints = pointOctree.GetNearby(position, maxDistance);

            if (lNearbyPoints.Length == 0)
                return null;

            return lNearbyPoints.OrderBy(p => Vector3.Distance(p.position, position)).First();
        }

        protected override void Awake()
        {
            base.Awake();
            Refresh();
        }

        [Button]
        private void Refresh()
        {
            Collider[] lColliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            GeneratePointOctreeFromColliders(lColliders);
            SetNeighbors();
        }

        #region Points Generation

        private void GeneratePointOctreeFromColliders(Collider[] colliders)
        {
            pointOctree = new PointOctree<GraphPoint>(size, transform.position, 1f);

            foreach (Collider lCollider in colliders)
            {
                if (!lCollider.enabled)
                    continue;

                List<Vector3> lSurfacePoints = GenerateSurfacePoints(lCollider);

                foreach (Vector3 lPoint in lSurfacePoints)
                    pointOctree.Add(new GraphPoint(lPoint), lPoint);
            }
        }

        List<Vector3> GenerateSurfacePoints(Collider collider)
        {
            if (collider is MeshCollider lMeshCollider)
                return GenerateMeshColliderPoints(lMeshCollider);
            else if (collider is BoxCollider lBoxCollider)
                return GenerateBoxColliderPoints(lBoxCollider);
            else
                return null;
        }

        List<Vector3> GenerateBoxColliderPoints(BoxCollider collider)
        {
            List<Vector3> points = new List<Vector3>();
            Vector3 size = collider.size;
            Vector3 center = collider.center;

            // Calculate the world space corners of the box collider
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(-size.x, -size.y, -size.z) / 2f;
            corners[1] = new Vector3(size.x, -size.y, -size.z) / 2f;
            corners[2] = new Vector3(-size.x, size.y, -size.z) / 2f;
            corners[3] = new Vector3(size.x, size.y, -size.z) / 2f;
            corners[4] = new Vector3(-size.x, -size.y, size.z) / 2f;
            corners[5] = new Vector3(size.x, -size.y, size.z) / 2f;
            corners[6] = new Vector3(-size.x, size.y, size.z) / 2f;
            corners[7] = new Vector3(size.x, size.y, size.z) / 2f;

            for (int i = 0; i < 8; i++)
            {
                corners[i] = collider.transform.TransformPoint(center + corners[i]);
            }

            // Generate points on each face
            GeneratePointsOnFace(points, corners[0], corners[1], corners[2]); // Front
            GeneratePointsOnFace(points, corners[4], corners[5], corners[6]); // Back
            GeneratePointsOnFace(points, corners[0], corners[1], corners[4]); // Bottom
            GeneratePointsOnFace(points, corners[2], corners[3], corners[6]); // Top
            GeneratePointsOnFace(points, corners[0], corners[2], corners[4]); // Left
            GeneratePointsOnFace(points, corners[1], corners[3], corners[5]); // Right

            return points;
        }

        void GeneratePointsOnFace(List<Vector3> points, Vector3 c1, Vector3 c2, Vector3 c3)
        {
            Vector3 edge1 = c2 - c1;
            Vector3 edge2 = c3 - c1;

            int stepsX = Mathf.CeilToInt(edge1.magnitude / pointsSpacing);
            int stepsY = Mathf.CeilToInt(edge2.magnitude / pointsSpacing);

            for (int x = 0; x <= stepsX; x++)
            {
                for (int y = 0; y <= stepsY; y++)
                {
                    float u = x / (float)stepsX;
                    float v = y / (float)stepsY;
                    Vector3 point = c1 + u * edge1 + v * edge2;
                    points.Add(point);
                }
            }
        }

        List<Vector3> GenerateMeshColliderPoints(MeshCollider meshCollider)
        {
            List<Vector3> points = new List<Vector3>();
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Create a 3D grid to store occupied cells
            HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v1 = meshCollider.transform.TransformPoint(vertices[triangles[i]]);
                Vector3 v2 = meshCollider.transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v3 = meshCollider.transform.TransformPoint(vertices[triangles[i + 2]]);

                GeneratePointsOnTriangle(v1, v2, v3, points, occupiedCells, meshCollider.CompareTag(addVerticesTag));
            }

            return points;
        }

        void GeneratePointsOnTriangle(Vector3 v1, Vector3 v2, Vector3 v3, List<Vector3> points, HashSet<Vector3Int> occupiedCells, bool addVertices)
        {
            Vector3 lNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            Vector3 lMin = Vector3.Min(Vector3.Min(v1, v2), v3);
            Vector3 lMax = Vector3.Max(Vector3.Max(v1, v2), v3);

            //Add the vertices as points
            if (addVertices)
            {
                points.Add(v1);
                points.Add(v2);
                points.Add(v3);
            }

            //Add points inside the triangle
            for (float x = lMin.x; x <= lMax.x; x += pointsSpacing)
            {
                for (float y = lMin.y; y <= lMax.y; y += pointsSpacing)
                {
                    for (float z = lMin.z; z <= lMax.z; z += pointsSpacing)
                    {
                        Vector3 point = new Vector3(x, y, z);
                        Vector3Int cell = Vector3Int.FloorToInt(point / pointsSpacing);

                        if (!occupiedCells.Contains(cell) && IsPointInTriangle(point, v1, v2, v3, lNormal))
                        {
                            points.Add(point);
                            occupiedCells.Add(cell);
                        }
                    }
                }
            }
        }

        bool IsPointInTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
        {
            float d = Vector3.Dot(normal, v1);
            if (Mathf.Abs(Vector3.Dot(normal, point) - d) > 0.01f)
                return false;

            Vector3 edge1 = v2 - v1;
            Vector3 edge2 = v3 - v2;
            Vector3 edge3 = v1 - v3;

            Vector3 c1 = Vector3.Cross(edge1, point - v1);
            Vector3 c2 = Vector3.Cross(edge2, point - v2);
            Vector3 c3 = Vector3.Cross(edge3, point - v3);

            return Vector3.Dot(c1, normal) >= 0 && Vector3.Dot(c2, normal) >= 0 && Vector3.Dot(c3, normal) >= 0;
        }

        #endregion

        private void SetNeighbors()
        {
            List<GraphPoint> lNearbyPoints = new();
            ICollection<GraphPoint> lGraph = pointOctree.GetAll();

            foreach (GraphPoint lPoint in lGraph)
            {
                pointOctree.GetNearbyNonAlloc(lPoint.position, pointsSpacing * 1.5f, lNearbyPoints);

                for (int i = lNearbyPoints.Count - 1; i >= 0; i--)
                {
                    Vector3 lPointToNearby = lNearbyPoints[i].position - lPoint.position;
                    //Don't start ray at point.position for preventing starting inside the collider
                    Vector3 lRaycastOrigin = lPoint.position - lPointToNearby.normalized * 0.01f;

                    //If there is a collider between the points, remove the point from the neighbors
                    if (Physics.Raycast(lRaycastOrigin, lPointToNearby, lPointToNearby.magnitude))
                        lNearbyPoints.RemoveAt(i);
                }

                lPoint.neighbors = new List<GraphPoint>(lNearbyPoints);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (pointOctree == null)
                return;

            pointOctree.DrawAllBounds(); // Draw node boundaries
            pointOctree.DrawAllObjects(); // Mark object positions

            //Draw neighbors
            foreach (GraphPoint lPoint in pointOctree.GetAll())
            {
                foreach (GraphPoint lNeighbor in lPoint.neighbors)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(lPoint.position, lNeighbor.position);
                }
            }
        }
    }

    public class GraphPoint : IEquatable<GraphPoint>
    {
        public Vector3 position;
        public List<GraphPoint> neighbors;

        public GraphPoint(Vector3 position)
        {
            this.position = position;
            neighbors = new List<GraphPoint>();
        }

        public bool Equals(GraphPoint other)
        {
            return position == other.position;
        }
    }
}