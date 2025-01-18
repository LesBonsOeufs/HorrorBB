using NaughtyAttributes;
using System.Linq;
using UnityEngine;

namespace Root
{
    public class CrawlingMan : MonoBehaviour
    {
        private const float SPHERE_RADIUS = 0.2f;

        [SerializeField] private float speed = .5f;
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float castLength = 1f;
        [SerializeField] private float castAngle = 120f;
        [SerializeField, ReadOnly] private float initialElevation;

        [InfoBox("Fill for dynamic leg anim duration (each initial anim duration will be divided with speed)"), SerializeField] private Leg[] legs = default;
        private float[] initLegAnimDurations;

        private void Start()
        {
            initLegAnimDurations = legs.Select(leg => leg.tipAnimationDuration).ToArray(); 

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
            Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(-castAngle * 0.5f, transform.right) * transform.up * -1), SPHERE_RADIUS, out lBackHit, castLength);
            Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(castAngle * 0.5f, transform.right) * transform.up * -1), SPHERE_RADIUS, out lFrontHit, castLength);

            Vector3 lFrontToBack = lBackHit.point - lFrontHit.point;
            float lFrontDistanceRatio = lFrontHit.distance / castLength;
            float lBackDistanceRatio = lBackHit.distance / castLength;

            Vector3 lAveragePoint = lFrontHit.point + lFrontToBack.normalized *
                lFrontToBack.magnitude * (0.5f - 0.5f * (1f - lFrontDistanceRatio) + 0.5f * (1f - lBackDistanceRatio));
            Vector3 lAverageNormal = ((lFrontHit.normal * (1f - lFrontDistanceRatio)) + 
                (lBackHit.normal * (1f - lBackDistanceRatio))).normalized;

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
            for (int i = legs.Length - 1; i >= 0; i--)
                legs[i].tipAnimationDuration = initLegAnimDurations[i] / speed;
        }
    }
}