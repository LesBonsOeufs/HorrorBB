using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Root
{
    //Generates world representation:
    //All points must be on a surface, and evenly spread from each other
    //All neighboring points must not have a collider between them, and be closer to each other than around pointsSpacing * 1.5
    public class SurfaceGraph : Singleton<SurfaceGraph>
    {
        [SerializeField] private bool refreshOnAwake = true;
        [SerializeField, Tag] private string addVerticesTag = "SurfaceGraph_AddVertices";
        [SerializeField] private float size = 15f;
        [SerializeField] private float pointsSpacing = 1f;
        [SerializeField] private float pointsNormalShift = 0.4f;
        [SerializeField] private float neighborMaxDistance = 1.5f;

        [SerializeField] private bool keepOnlyReachableFrom = false;
        [ShowIf(nameof(keepOnlyReachableFrom)), SerializeField] private Vector3 reachablePoint = Vector3.zero;

        [Foldout("Advanced"), Tooltip("Does not influence the final position of the points"), SerializeField] 
        private float neighborAssignmentPurposeNormalShift = 0.01f;
        [Foldout("Advanced"), SerializeField] private float safeRaycastOffset = 0.1f;
        [Foldout("Advanced"), SerializeField] private float sphereCastRadius = 0.009f;

        private PointOctree<GraphPoint> pointOctree;

        public GraphPoint GetClosestPoint(Vector3 position, float maxDistance)
        {
            return GetSortedClosePoints(position, maxDistance)?.First();
        }

        /// <summary>
        /// Gets all direct points under maxDistance from the position, sorted by proximity
        /// </summary>
        public IEnumerable<GraphPoint> GetSortedClosePoints(Vector3 position, float maxDistance)
        {
            List<GraphPoint> lNearbyPoints = pointOctree.GetNearby(position, maxDistance).ToList();
            //Here, maybe should not use safeRaycasts
            RemoveNonDirectPoints(position, lNearbyPoints);

            if (lNearbyPoints.Count == 0)
                return null;

            return lNearbyPoints.OrderBy(p => Vector3.Distance(p.position, position));
        }

        protected override void Awake()
        {
            base.Awake();

            if (refreshOnAwake)
                Refresh();
        }

        [Button]
        public void Refresh()
        {
            Collider[] lColliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            List<GraphPoint> lPoints = GeneratePointsFromColliders(lColliders);

            //Fill point octree (for neighbors calculation)
            pointOctree = new PointOctree<GraphPoint>(size, transform.position, 1f);
            foreach (GraphPoint lPoint in lPoints)
                pointOctree.Add(lPoint, lPoint.position);

            NormalShift(lPoints, neighborAssignmentPurposeNormalShift);

            SetNeighbors();

            if (keepOnlyReachableFrom)
                SelectReachables(lPoints);

            if (pointsNormalShift != 0f)
                NormalShift(lPoints, pointsNormalShift - neighborAssignmentPurposeNormalShift);
        }

        #region Points Generation

        private List<GraphPoint> GeneratePointsFromColliders(Collider[] colliders)
        {
            List<GraphPoint> lPoints = new();

            foreach (Collider lCollider in colliders)
            {
                if (!lCollider.enabled || lCollider.isTrigger)
                    continue;

                lPoints.AddRange(GenerateSurfacePoints(lCollider));
            }

            return lPoints;
        }

        List<GraphPoint> GenerateSurfacePoints(Collider collider)
        {
            if (collider is MeshCollider lMeshCollider)
                return GenerateMeshColliderPoints(lMeshCollider);
            else if (collider is BoxCollider lBoxCollider)
                return GenerateBoxColliderPoints(lBoxCollider);
            else
                return new List<GraphPoint>();
        }

        List<GraphPoint> GenerateBoxColliderPoints(BoxCollider collider)
        {
            List<GraphPoint> lPoints = new();
            Vector3 lSize = collider.size;
            Vector3 lCenter = collider.center;

            // Calculate the world space corners of the box collider
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(-lSize.x, -lSize.y, -lSize.z) / 2f;
            corners[1] = new Vector3(lSize.x, -lSize.y, -lSize.z) / 2f;
            corners[2] = new Vector3(-lSize.x, lSize.y, -lSize.z) / 2f;
            corners[3] = new Vector3(lSize.x, lSize.y, -lSize.z) / 2f;
            corners[4] = new Vector3(-lSize.x, -lSize.y, lSize.z) / 2f;
            corners[5] = new Vector3(lSize.x, -lSize.y, lSize.z) / 2f;
            corners[6] = new Vector3(-lSize.x, lSize.y, lSize.z) / 2f;
            corners[7] = new Vector3(lSize.x, lSize.y, lSize.z) / 2f;

            for (int i = 0; i < 8; i++)
            {
                corners[i] = collider.transform.TransformPoint(lCenter + corners[i]);
            }

            // Generate points on each face
            GeneratePointsOnFace(lPoints, corners[0], corners[1], corners[2], collider.transform.forward * -1); // Front
            GeneratePointsOnFace(lPoints, corners[4], corners[5], corners[6], collider.transform.forward); // Back
            GeneratePointsOnFace(lPoints, corners[0], corners[1], corners[4], collider.transform.up * -1); // Bottom
            GeneratePointsOnFace(lPoints, corners[2], corners[3], corners[6], collider.transform.up); // Top
            GeneratePointsOnFace(lPoints, corners[0], corners[2], corners[4], collider.transform.right * -1); // Left
            GeneratePointsOnFace(lPoints, corners[1], corners[3], corners[5], collider.transform.right); // Right

            return lPoints;
        }

        void GeneratePointsOnFace(List<GraphPoint> points, Vector3 c1, Vector3 c2, Vector3 c3, Vector3 normal)
        {
            Vector3 lEdge1 = c2 - c1;
            Vector3 lEdge2 = c3 - c1;

            int lStepsX = Mathf.CeilToInt(lEdge1.magnitude / pointsSpacing);
            int lStepsY = Mathf.CeilToInt(lEdge2.magnitude / pointsSpacing);

            for (int x = 0; x <= lStepsX; x++)
            {
                for (int y = 0; y <= lStepsY; y++)
                {
                    float u = x / (float)lStepsX;
                    float v = y / (float)lStepsY;
                    Vector3 point = c1 + u * lEdge1 + v * lEdge2;
                    points.Add(new GraphPoint(point, normal));
                }
            }
        }

        List<GraphPoint> GenerateMeshColliderPoints(MeshCollider meshCollider)
        {
            List<GraphPoint> lPoints = new();
            Mesh lMesh = meshCollider.sharedMesh;
            Vector3[] lVertices = lMesh.vertices;
            int[] lTriangles = lMesh.triangles;

            // Create a 3D grid to store occupied cells
            HashSet<Vector3Int> lOccupiedCells = new HashSet<Vector3Int>();

            for (int i = 0; i < lTriangles.Length; i += 3)
            {
                Vector3 v1 = meshCollider.transform.TransformPoint(lVertices[lTriangles[i]]);
                Vector3 v2 = meshCollider.transform.TransformPoint(lVertices[lTriangles[i + 1]]);
                Vector3 v3 = meshCollider.transform.TransformPoint(lVertices[lTriangles[i + 2]]);
                GeneratePointsOnTriangle(lPoints, v1, v2, v3, lOccupiedCells, meshCollider.CompareTag(addVerticesTag));
            }

            return lPoints;
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
                        Vector3 point = new Vector3(x, y, z);
                        Vector3Int cell = Vector3Int.FloorToInt(point / pointsSpacing);

                        if (!occupiedCells.Contains(cell) && IsPointInTriangle(point, v1, v2, v3, lNormal))
                        {
                            points.Add(new GraphPoint(point, lNormal));
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

        #region Set Neighbors

        private void SetNeighbors()
        {
            List<GraphPoint> lNearbyPoints = new();
            ICollection<GraphPoint> lGraph = pointOctree.GetAll();

            foreach (GraphPoint lPoint in lGraph)
            {
                pointOctree.GetNearbyNonAlloc(lPoint.position, neighborMaxDistance, lNearbyPoints);

                foreach (GraphPoint lNearby in lNearbyPoints)
                {
                    if (lPoint == lNearby || lPoint.neighbors.Contains(lNearby))
                        continue;

                    //If there is a collider between the points, not valid
                    //Check both ways
                    if (SafeSpherecast(lPoint.position, lNearby.position) || SafeSpherecast(lNearby.position, lPoint.position))
                        continue;

                    lPoint.neighbors.Add(lNearby);
                    lNearby.neighbors.Add(lPoint);
                }
            }
        }

        /// <summary>
        /// Removes all points that are separated from position by a collider
        /// </summary>
        private void RemoveNonDirectPoints(Vector3 position, List<GraphPoint> testedPoints)
        {
            Vector3 lPositionToTested;

            for (int i = testedPoints.Count - 1; i >= 0; i--)
            {
                lPositionToTested = testedPoints[i].position - position;

                if (Physics.Raycast(position, lPositionToTested, lPositionToTested.magnitude))
                    testedPoints.RemoveAt(i);
            }
        }

        #endregion

        #region Optional

        private void SelectReachables(List<GraphPoint> points)
        {
            GraphPoint lStartPoint = GetClosestPoint(reachablePoint, size);

            if (lStartPoint == null)
            {
                Debug.LogError("Initial reachable point on graph could not be found");
                return;
            }

            HashSet<GraphPoint> lReachablePoints = new() { lStartPoint };
            Queue<GraphPoint> lQueue = new();
            lQueue.Enqueue(lStartPoint);

            while (lQueue.Count > 0)
            {
                GraphPoint current = lQueue.Dequeue();
                foreach (GraphPoint neighbor in current.neighbors)
                {
                    if (!lReachablePoints.Contains(neighbor))
                    {
                        lReachablePoints.Add(neighbor);
                        lQueue.Enqueue(neighbor);
                    }
                }
            }

            // Update the points list with only reachable points
            points.Clear();
            points.AddRange(lReachablePoints);
        }

        private void NormalShift(List<GraphPoint> points, float shifting)
        {
            //After neighbors are set, shift points based on normals
            foreach (GraphPoint lPoint in points)
            {
                Vector3 lShift = lPoint.normal * shifting;

                //If shifting will traverse a collider, don't shift
                if (!SafeSpherecast(lPoint.position, lPoint.position + lShift))
                    lPoint.position += lShift;
            }

            //Update point octree
            pointOctree = new PointOctree<GraphPoint>(size, transform.position, 1f);
            foreach (GraphPoint lPoint in points)
                pointOctree.Add(lPoint, lPoint.position);
        }

        private bool SafeSpherecast(Vector3 origin, Vector3 target)
        {
            //Don't start ray at point.position for preventing starting inside a collider
            Vector3 lRaycastOrigin = origin - (target - origin).normalized * safeRaycastOffset;
            Vector3 lOriginToTarget = target - lRaycastOrigin;
            return Physics.SphereCast(new Ray(lRaycastOrigin, lOriginToTarget), sphereCastRadius, lOriginToTarget.magnitude);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (keepOnlyReachableFrom)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(reachablePoint, Vector3.one * 0.5f);
            }

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
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(lPoint.position, lPoint.position + lPoint.normal * 0.25f);
            }
        }
    }

    public class GraphPoint : IEquatable<GraphPoint>
    {
        public Vector3 position;
        public Vector3 normal;
        public HashSet<GraphPoint> neighbors;

        public GraphPoint(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
            neighbors = new HashSet<GraphPoint>();
        }

        public bool Equals(GraphPoint other)
        {
            return position == other.position;
        }
    }
}