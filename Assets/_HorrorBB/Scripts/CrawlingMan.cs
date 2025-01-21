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
        [Foldout("Movement"), SerializeField] private float pitchSpeed = 6f;
        [Foldout("Movement"), SerializeField] private float yawSpeed = 6f;
        [Foldout("Movement"), SerializeField] private float rollSpeed = 6f;
        [Foldout("Movement"), SerializeField] private float acceptedDistanceFromTarget = .2f;

        [Foldout("Raycasting"), SerializeField] private float sphereCastRadius = 0.2f;
        [Foldout("Raycasting"), SerializeField] private float castLength = 1f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castAngle = 45f;
        [Foldout("Raycasting"), SerializeField, Range(0f, 90f)] private float castOpening = 90f;

        [SerializeField] private float initialElevation = 0.4f;

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
            UpdateDynamicLegAnimDurations();
            Vector3 lNextPos = NextPositionToTarget(moveTarget.position) ?? transform.position;
            Crawl(lNextPos - transform.position);
            lookTarget.position = moveTarget.position;
        }

        #region PathFinding

        private Vector3? NextPositionToTarget(Vector3 target)
        {
            List<GraphPoint> lPath = PathFinding(target);

            if (lPath == null)
                return null;

            while (IsPathPointReached(lPath[0].position, lPath.Count == 1 ? null : lPath[1].position))
            {
                if (lPath.Count == 1)
                {
                    lPath = null;
                    break;
                }
                else
                    lPath.RemoveAt(0);
            }

            Vector3 lTargetPosition;

            if (lPath == null)
                lTargetPosition = transform.position;
            else
            {
                lTargetPosition = lPath[0].position;

#if UNITY_EDITOR
                //If editor & is selected, show path
                if (Selection.Contains(gameObject))
                {
                    for (int i = 0; i < lPath.Count; i++)
                        Debug.DrawLine(i == 0 ?
                            transform.position :
                            lPath[i - 1].position, lPath[i].position, Color.red);
                }
#endif
            }

            return lTargetPosition;
        }

        private List<GraphPoint> PathFinding(Vector3 target)
        {
            float lMaxDistance = 2.5f;
            GraphPoint lOriginPoint = SurfaceGraph.Instance.GetClosestPoint(transform.position, lMaxDistance);
            GraphPoint lTargetPoint = SurfaceGraph.Instance.GetClosestPoint(target, lMaxDistance);

            if (lOriginPoint == null || lTargetPoint == null)
                return null;

            List<GraphPoint> lGraphPath =
                SimpleAGreedy<GraphPoint>.Execute(lOriginPoint, lTargetPoint, graphPoint => graphPoint.neighbors.ToArray(), 
                graphPoint => true, (point1, point2) => (point1.position - point2.position).sqrMagnitude);

            if (lGraphPath == null)
            {
                //If no path could be made, try reaching target directly
                return new List<GraphPoint> { lTargetPoint }; ;
            }

            Plane lLastPathPointSurface = new(lGraphPath[^1].normal, lGraphPath[^1].position);
            lGraphPath.Add(new GraphPoint(lLastPathPointSurface.ClosestPointOnPlane(target), lLastPathPointSurface.normal));

            return lGraphPath;
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

            Vector3 lVelocity;

            if (direction == Vector3.zero)
                lVelocity = Vector3.zero;
            else
                lVelocity = speed * Time.deltaTime * Vector3.ProjectOnPlane(transform.forward, lAverageNormal).normalized;

            transform.position = lPlanePoint + lElevation + lVelocity;

            if (lVelocity != Vector3.zero)
            {
                Quaternion lTargetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, lAverageNormal), lAverageNormal);
                Quaternion lRelativeRot = Quaternion.Inverse(transform.rotation) * lTargetRotation;
                Vector3 lRelativeEuleer = lRelativeRot.eulerAngles;

                Quaternion lPitchRot = Quaternion.Euler(lRelativeEuleer.x, 0, 0);
                Quaternion lYawRot = Quaternion.Euler(0, lRelativeEuleer.y, 0);
                Quaternion lRollRot = Quaternion.Euler(0, 0, lRelativeEuleer.z);

                transform.rotation *= Quaternion.Slerp(Quaternion.identity, lPitchRot, pitchSpeed * Time.deltaTime);
                transform.rotation *= Quaternion.Slerp(Quaternion.identity, lYawRot, yawSpeed * Time.deltaTime);
                transform.rotation *= Quaternion.Slerp(Quaternion.identity, lRollRot, rollSpeed * Time.deltaTime);
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
            Gizmos.DrawSphere(transform.position, acceptedDistanceFromTarget);
        }
    }
}