using NaughtyAttributes;
using System.Linq;
using UnityEngine;

namespace Root
{
    public class CrawlingMan : MonoBehaviour
    {

        [SerializeField] private float speed = .5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float sphereCastRadius = 0.2f;
        [SerializeField] private float castLength = 1f;
        [SerializeField, Range(0f, 90f)] private float castAngle = 45f;
        [SerializeField, Range(0f, 90f)] private float castOpening = 90f;
        [SerializeField, ReadOnly] private float initialElevation;

        [InfoBox("Fill for dynamic leg anim duration & maxTipWait (each initial duration will be divided with speed)"), SerializeField] 
        private LegController legController;

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
            Vector3 lVelocity = Vector3.ProjectOnPlane(transform.forward, lAverageNormal) * speed * Time.deltaTime;
            transform.position = lPlanePoint + lElevation + lVelocity;

            Debug.DrawRay(transform.position, lVelocity / Time.deltaTime);

            Quaternion lTargetRotation = Quaternion.LookRotation(lVelocity, lAverageNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, rotationSpeed * Time.deltaTime);
        }

        private void UpdateDynamicLegAnimDurations()
        {
            for (int i = legController.Legs.Length - 1; i >= 0; i--)
                legController.Legs[i].tipAnimationDuration = initLegAnimDurations[i] / speed;

            legController.maxTipWait = initControllerMaxTipWait / speed;
        }
    }
}