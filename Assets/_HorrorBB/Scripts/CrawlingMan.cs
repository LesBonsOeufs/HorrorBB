using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Root
{
    public class CrawlingMan : MonoBehaviour
    {
        [InfoBox("Must not be child of this"), SerializeField] private Transform moveTarget;
        [InfoBox("From RigLook"), SerializeField] private Transform lookTarget;
        [InfoBox("Fill for dynamic leg anim duration & maxTipWait (each initial duration will be divided with speed)"), SerializeField]
        private LegController legController;

        [Foldout("Movement"), SerializeField] private float speed = .5f;
        [Foldout("Movement"), SerializeField] private float rotationSpeed = 2f;
        [Foldout("Movement"), SerializeField] private float distanceFromPathPointForNext = .2f;

        [Foldout("Raycasting"), SerializeField] private float sphereCastRadius = 0.2f;
        [Foldout("Raycasting"), SerializeField] private float castLength = 1f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castAngle = 45f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castOpening = 90f;

        [SerializeField, ReadOnly] private float initialElevation;

        private float initControllerMaxTipWait;
        private float[] initLegAnimDurations;

        private void Start()
        {
            initControllerMaxTipWait = legController.maxTipWait;
            initLegAnimDurations = legController.Legs.Select(leg => leg.tipAnimationDuration).ToArray(); 

            if (Physics.Raycast(new Ray(transform.position, transform.up * -1), out RaycastHit lHit, 1f))
                initialElevation = lHit.distance;
            else
                initialElevation = 1f;
        }

        private void Update()
        {
            lookTarget.position = moveTarget.position;
            UpdateDynamicLegAnimDurations();
            List<GraphPoint> lPath = PathFinding(moveTarget.position);

            if (lPath == null)
                return;

#if UNITY_EDITOR
            //If editor & is selected, show path
            if (Selection.Contains(gameObject))
            {
                for (int i = 0; i < lPath.Count; i++)
                    Debug.DrawLine(i == 0 ? transform.position : lPath[i - 1].position, lPath[i].position, Color.red);
            }
#endif

            bool lIsLastPathPoint = lPath.Count == 1;
            Vector3 lTargetPosition = lPath[0].position;

            if (IsPathPointReached(lPath[0], lIsLastPathPoint ? null : lPath[1]))
                lTargetPosition = lIsLastPathPoint ? transform.position : lPath[1].position;

            Crawl(lTargetPosition - transform.position);
        }

        private bool IsPathPointReached(GraphPoint point, GraphPoint nextPoint)
        {
            //Reached if distance is less than distanceFromPathPointForNext
            Vector3 lOnSurfacePosition = transform.position - transform.up * initialElevation;
            float lDistance = Vector3.Distance(lOnSurfacePosition, point.position);

            if (lDistance < distanceFromPathPointForNext)
                return true;

            // Reached if crawler is between point and next point
            if (nextPoint != null)
            {
                Vector3 lPointToNext = (nextPoint.position - point.position).normalized;
                Vector3 lPointToThis = (lOnSurfacePosition - point.position).normalized;

                if (Vector3.Dot(lPointToNext, lPointToThis) > 0f)
                    return true;
            }

            return false;
        }

        private List<GraphPoint> PathFinding(Vector3 target)
        {
            GraphPoint lOriginPoint = SurfaceGraph.Instance.GetClosestPoint(transform.position, 2.5f);
            GraphPoint lTargetPoint = SurfaceGraph.Instance.GetClosestPoint(target, 2.5f);

            if (lOriginPoint == null || lTargetPoint == null)
                return null;

            return SimpleDjikstra<GraphPoint>.Execute(lOriginPoint, lTargetPoint, graphPoint => graphPoint.neighbors.ToArray(), graphPoint => true);
        }

        /// <param name="direction">Does not need to be normalized</param>
        private void Crawl(Vector3 direction)
        {
            RaycastHit lFrontHit;
            RaycastHit lBackHit;

            ///This method allows smooth point average, but can easily lose contact
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
            Vector3 lVelocity = Vector3.ProjectOnPlane(direction, lAverageNormal).normalized * speed * Time.deltaTime;
            transform.position = lPlanePoint + lElevation + lVelocity;

            if (lVelocity != Vector3.zero)
            {
                Quaternion lTargetRotation = Quaternion.LookRotation(lVelocity, lAverageNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, rotationSpeed * Time.deltaTime);
            }
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
            Gizmos.DrawSphere(transform.position - transform.up * initialElevation, distanceFromPathPointForNext);
        }
    }
}