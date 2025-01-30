using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Root
{
    public class CrawlingMan : MonoBehaviour
    {
        [InfoBox("From RigLook"), SerializeField] private Transform lookTarget;
        [InfoBox("Fill for dynamic leg anim duration & maxTipWait (each initial duration will be divided with speed)"), SerializeField]
        private LegController legController;

        [Foldout("Movement"), SerializeField] private float speed = .5f;
        [Foldout("Movement"), SerializeField] private float rotationSpeed = 5f;
        [Foldout("Movement"), SerializeField] private float acceptedDistanceFromTarget = .2f;
        [Foldout("Movement"), SerializeField] private float pathfindingCooldown = 2f;

        [Foldout("Raycasting"), SerializeField] private float sphereCastRadius = 0.2f;
        [Foldout("Raycasting"), SerializeField] private float castLength = 1f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castAngle = 45f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castOpening = 90f;

        [SerializeField] private bool autoInitElevation = true;
        [InfoBox("If auto, will be used as fallback value"), SerializeField] private float initialElevation = 0.4f;

        private Transform target;
        private float initControllerMaxTipWait;
        private float[] initLegAnimDurations;
        private List<GraphPoint> currentPath;

        private void Start()
        {
            initControllerMaxTipWait = legController.maxTipWait;
            initLegAnimDurations = legController.Legs.Select(leg => leg.tipAnimationDuration).ToArray(); 

            if (autoInitElevation && Physics.Raycast(new Ray(transform.position, transform.up * -1), out RaycastHit lHit, 1f))
                initialElevation = lHit.distance;

            StartCoroutine(RefreshPath());
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private void Update()
        {
            UpdateDynamicLegAnimDurations();
            RemoveReachedPathPoints();
            FollowPath();
            lookTarget.position = target.position;
        }

        #region PathFinding

        private IEnumerator RefreshPath()
        {
            while (true)
            {
                currentPath = PathFinding(target.position);
                yield return new WaitForSeconds(pathfindingCooldown);
            }
        }

        private List<GraphPoint> PathFinding(Vector3 target)
        {
            GraphPoint lOriginPoint = SurfaceGraph.Instance.GetClosestPoint(transform.position, 1.5f);
            GraphPoint lTargetPoint = SurfaceGraph.Instance.GetClosestPoint(target, 2.5f);

            if (lOriginPoint == null || lTargetPoint == null)
                return null;

            static IEnumerable<GraphPoint> lNeighborsFunc(GraphPoint graphPoint) => graphPoint.neighbors.ToArray();
            static bool lIsWalkableFunc(GraphPoint graphPoint) => true;
            static float lHeuristic(GraphPoint point1, GraphPoint point2) => (point1.position - point2.position).sqrMagnitude;
            //Cost varies from 1 to 3
            static float lCostFromToFunc(GraphPoint point1, GraphPoint point2) => 2 - Vector3.Dot(point1.normal, point2.normal);

            List<GraphPoint> lGraphPath =
                SimpleAGreedy<GraphPoint>.Execute(lOriginPoint, lTargetPoint, lNeighborsFunc, lIsWalkableFunc, 
                lHeuristic, out IEnumerable<GraphPoint> lAttempts, lCostFromToFunc);

            bool lPathfindingFailed = lGraphPath == null;
            if (lPathfindingFailed)
                lGraphPath = new() { lOriginPoint, lTargetPoint };

#if UNITY_EDITOR
            Color lDrawColor = lPathfindingFailed ? Color.red : Color.green;

            if (Selection.Contains(gameObject))
            {
                for (int i = 1; i < lGraphPath.Count; i++)
                    Debug.DrawLine(lGraphPath[i - 1].position, lGraphPath[i].position, lDrawColor, pathfindingCooldown);

                foreach (GraphPoint lNeighbor in lOriginPoint.neighbors)
                    Debug.DrawLine(lOriginPoint.position, lNeighbor.position, new Color(0f, 1f, 0f, 0.5f), pathfindingCooldown);
                foreach (GraphPoint lAttempt in lAttempts)
                    Extension_Debug.DrawCross(lAttempt.position, 0.1f, lDrawColor, pathfindingCooldown);
            }
            
#endif

            Plane lLastPathPointSurface = new(lGraphPath[^1].normal, lGraphPath[^1].position);
            lGraphPath.Add(new GraphPoint(lLastPathPointSurface.ClosestPointOnPlane(target), lLastPathPointSurface.normal));

            return lGraphPath;
        }

        private Vector3? RemoveReachedPathPoints() => RemoveReachedPathPoints(out _);
        private Vector3? RemoveReachedPathPoints(out GraphPoint lastReached)
        {
            lastReached = null;

            if (currentPath == null)
                return null;

            while (IsPathPointReached(currentPath[0].position, currentPath.Count == 1 ? null : currentPath[1].position))
            {
                lastReached = currentPath[0];

                if (currentPath.Count == 1)
                {
                    currentPath = null;
                    break;
                }
                else
                    currentPath.RemoveAt(0);
            }

            Vector3 lTargetPosition;

            if (currentPath == null)
                lTargetPosition = transform.position;
            else
                lTargetPosition = currentPath[0].position;

            return lTargetPosition;
        }

        private bool IsPathPointReached(Vector3 point, Vector3? nextPoint = null)
        {
            //Reached if distance is less than distanceFromPathPointForNext
            float lDistance = Vector3.Distance(transform.position, point);

            if (lDistance < acceptedDistanceFromTarget)
                return true;

            // Reached if crawler is between point and next point
            if (nextPoint != null)
            {
                Vector3 lPointToNext = (nextPoint.Value - point).normalized;
                Vector3 lPointToThis = (transform.position - point).normalized;

                if (Vector3.Dot(lPointToNext, lPointToThis) > 0f)
                    return true;
            }

            return false;
        }

        #endregion

        private void FollowPath()
        {
            GraphPoint lPoint = currentPath?[0];

            if (lPoint == null)
                return;

            Vector3 lDirection = (lPoint.position - transform.position).normalized;
            Vector3 lMovement = speed * Time.deltaTime * lDirection;
            transform.position += lMovement;

            Quaternion lTargetRotation = Quaternion.LookRotation(lDirection, lPoint.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, rotationSpeed * Time.deltaTime);
        }

        private Vector3 PositionCorrection()
        {
            RaycastHit lFrontHit;
            RaycastHit lBackHit;

            ///This method allows smooth point average
            bool lFrontHasHit = Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(-castAngle - castOpening, transform.right) * transform.up * -1), sphereCastRadius, out lFrontHit, castLength);
            bool lBackHasHit = Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(-castAngle + castOpening, transform.right) * transform.up * -1), sphereCastRadius, out lBackHit, castLength);

            Vector3 lFrontToBack = lBackHit.point - lFrontHit.point;
            float lFrontProximityRatio = lFrontHasHit ? 1f - (lFrontHit.distance / castLength) : 0f;
            float lBackProximityRatio = lBackHasHit ? 1f - (lBackHit.distance / castLength) : 0f;

            //0 = front point, 1 = back point
            float lDistanceBasedMultiplier = lFrontProximityRatio + lBackProximityRatio;
            if (lDistanceBasedMultiplier == 0)
                lDistanceBasedMultiplier = 0.5f;
            else
                lDistanceBasedMultiplier = lBackProximityRatio / lDistanceBasedMultiplier;
            Vector3 lAveragePoint = lFrontHit.point + lFrontToBack.normalized * lFrontToBack.magnitude * lDistanceBasedMultiplier;
            Vector3 lAverageNormal = ((lFrontHit.normal * lFrontProximityRatio) + (lBackHit.normal * lBackProximityRatio)).normalized;
            Vector3 lPlanePoint = new Plane(lAverageNormal, lAveragePoint).ClosestPointOnPlane(transform.position);
            Vector3 lElevation = lAverageNormal * initialElevation;

            return lPlanePoint + lElevation;
        }

        private void UpdateDynamicLegAnimDurations()
        {
            if (speed == 0f)
                return;

            for (int i = legController.Legs.Length - 1; i >= 0; i--)
                legController.Legs[i].tipAnimationDuration = initLegAnimDurations[i] / Mathf.Abs(speed);

            legController.maxTipWait = initControllerMaxTipWait / Mathf.Abs(speed);
        }

        private void OnDrawGizmosSelected()
        {
            ColorUtility.TryParseHtmlString("#0000FF80", out Color lColor);
            Gizmos.color = lColor;
            Gizmos.DrawSphere(transform.position, acceptedDistanceFromTarget);
        }
    }
}