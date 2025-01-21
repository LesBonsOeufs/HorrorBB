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
            List<GraphPoint> lNearbyPoints = pointOctree.GetNearby(position, maxDistance).ToList();

            if (lNearbyPoints.Count == 0)
                return null;

            RemoveNonDirectPoints(position, lNearbyPoints);
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

                List<GraphPoint> lSurfacePoints = GenerateSurfacePoints(lCollider);

                foreach (GraphPoint lPoint in lSurfacePoints)
                    pointOctree.Add(lPoint, lPoint.position);
            }
        }

        List<GraphPoint> GenerateSurfacePoints(Collider collider)
        {
            if (collider is MeshCollider lMeshCollider)
                return GenerateMeshColliderPoints(lMeshCollider);
            else if (collider is BoxCollider lBoxCollider)
                return GenerateBoxColliderPoints(lBoxCollider);
            else
                return null;
        }

        List<GraphPoint> GenerateBoxColliderPoints(BoxCollider collider)
        {
            List<GraphPoint> points = new();
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
            GeneratePointsOnFace(points, corners[0], corners[1], corners[2], collider.transform.forward * -1); // Front
            GeneratePointsOnFace(points, corners[4], corners[5], corners[6], collider.transform.forward); // Back
            GeneratePointsOnFace(points, corners[0], corners[1], corners[4], collider.transform.up * -1); // Bottom
            GeneratePointsOnFace(points, corners[2], corners[3], corners[6], collider.transform.up); // Top
            GeneratePointsOnFace(points, corners[0], corners[2], corners[4], collider.transform.right * -1); // Left
            GeneratePointsOnFace(points, corners[1], corners[3], corners[5], collider.transform.right); // Right

            return points;
        }

        void GeneratePointsOnFace(List<GraphPoint> points, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 normal)
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
                    points.Add(new GraphPoint(point, normal));
                }
            }
        }

        List<GraphPoint> GenerateMeshColliderPoints(MeshCollider meshCollider)
        {
            List<GraphPoint> points = new();
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
                GeneratePointsOnTriangle(points, v1, v2, v3, occupiedCells, meshCollider.CompareTag(addVerticesTag));
            }

            return points;
        }

        void GeneratePointsOnTriangle(List<GraphPoint> points, Vector3 v1, Vector3 v2, Vector3 v3, HashSet<Vector3Int> occupiedCells, bool addVertices)
        {
            Vector3 lNormal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            Vector3 lMin = Vector3.Min(Vector3.Min(v1, v2), v3);
            Vector3 lMax = Vector3.Max(Vector3.Max(v1, v2), v3);

            //Add the vertices as points
            if (addVertices)
            {
                points.Add(new GraphPoint(v1, lNormal));
                points.Add(new GraphPoint(v2, lNormal));
                points.Add(new GraphPoint(v3, lNormal));
            }

            //Add points inside the triangle
            for (float x = lMin.x; x <= lMax.x; x += pointsSpacing)
            {
                for (float y = lMin.y; y <= lMax.y; y += pointsSpacing)
                {
                    for (float z = lMin.z; z <= lMax.z; z += pointsSpacing)
                    {
                        Vector3 lPoint = new Vector3(x, y, z);
                        Vector3Int lCell = Vector3Int.FloorToInt(lPoint / pointsSpacing);

                        if (!occupiedCells.Contains(lCell) && IsPointInTriangle(lPoint, v1, v2, v3, lNormal))
                        {
                            points.Add(new GraphPoint(lPoint, lNormal));
                            occupiedCells.Add(lCell);
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
                RemoveNonDirectPoints(lPoint.position, lNearbyPoints);
                lPoint.neighbors = new List<GraphPoint>(lNearbyPoints);
            }
        }

        /// <summary>
        /// Removes all points that are separated from position by a collider
        /// </summary>
        private void RemoveNonDirectPoints(Vector3 position, List<GraphPoint> testedPoints)
        {
            for (int i = testedPoints.Count - 1; i >= 0; i--)
            {
                Vector3 lPointToNearby = testedPoints[i].position - position;
                //Don't start ray at point.position for preventing starting inside the collider
                Vector3 lRaycastOrigin = position - lPointToNearby.normalized * 0.01f;

                //If there is a collider between the points, remove the point from the neighbors
                if (Physics.Raycast(lRaycastOrigin, lPointToNearby, lPointToNearby.magnitude))
                    testedPoints.RemoveAt(i);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (pointOctree == null)
                return;

            pointOctree.DrawAllBounds(); // Draw node boundaries
            pointOctree.DrawAllObjects(); // Mark object positions
            ICollection<GraphPoint> lPoints = pointOctree.GetAll();

            //Draw neighbors
            foreach (GraphPoint lPoint in lPoints)
            {
                foreach (GraphPoint lNeighbor in lPoint.neighbors)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(lPoint.position, lNeighbor.position);
                }
            }

            //Draw normals
            foreach (GraphPoint lPoint in lPoints)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(lPoint.position, lPoint.position + lPoint.normal * 0.1f);
            }
        }
    }

    public class GraphPoint : IEquatable<GraphPoint>
    {
        public Vector3 position;
        public Vector3 normal;
        public List<GraphPoint> neighbors;

        public GraphPoint(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
            neighbors = new List<GraphPoint>();
        }

        public bool Equals(GraphPoint other)
        {
            return position == other.position;
        }
    }
}