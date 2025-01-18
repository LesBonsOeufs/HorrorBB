using NaughtyAttributes;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

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

        private void Start()
        {
            if (Physics.Raycast(new Ray(transform.position, transform.up * -1), out RaycastHit lHit, 1f))
                initialElevation = lHit.distance;
            else
                initialElevation = 1f;
        }

        private void Update()
        {
            RaycastHit lForwardHit;
            RaycastHit lBackwardHit;
            Vector3 lLocalDirection = Vector3.forward;

            ///This method allows smooth point average, but can easily lose contact
            Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(-castAngle * 0.5f, transform.right) * transform.up * -1), SPHERE_RADIUS, out lBackwardHit, castLength);
            Physics.SphereCast(new Ray(transform.position,
                Quaternion.AngleAxis(castAngle * 0.5f, transform.right) * transform.up * -1), SPHERE_RADIUS, out lForwardHit, castLength);

            Vector3 lForwardToBack = lBackwardHit.point - lForwardHit.point;
            float lForwardDistanceRatio = lForwardHit.distance / castLength;
            float lBackwardDistanceRatio = lBackwardHit.distance / castLength;

            Vector3 lAveragePoint = lForwardHit.point + lForwardToBack.normalized *
                lForwardToBack.magnitude * (0.5f - 0.5f * (1f - lForwardDistanceRatio) + 0.5f * (1f - lBackwardDistanceRatio));
            Vector3 lAverageNormal = ((lForwardHit.normal * (1f - lForwardDistanceRatio)) + 
                (lBackwardHit.normal * (1f - lBackwardDistanceRatio))).normalized;

            Quaternion FromTo = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, lAverageNormal, transform.right), transform.right);
            transform.position = new Plane(lAverageNormal, lAveragePoint).ClosestPointOnPlane(transform.position) + lAverageNormal * initialElevation;
            transform.position += FromTo * lLocalDirection * speed * Time.deltaTime;
            Debug.DrawLine(transform.position, lAveragePoint, Color.red);

            Quaternion lTargetRotation = transform.rotation * Quaternion.FromToRotation(transform.up, lAverageNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, rotationSpeed * Time.deltaTime);
        }

        ///PREVIOUS ISSUE (with forward & down spherecast): when changing surface, normal stops lerping and goes to new surface as soon as the down spherecast hits it.
        ///Slerping rotation is a quick fix.
        ///Was fixed by making the casts forward-down & backward-down, instead of forward & down

        //Vector3 lForwardMovementAxis = Quaternion.AngleAxis(90f, transform.up) * (transform.position - lastPosition).normalized;
        ///Using this instead allows moving without necessary going forward (automatic rotation towards direction therefore becomes not mandatory).
        ///However there are currently issues.
        //bool lBothHit = Physics.SphereCast(new Ray(transform.position, Quaternion.AngleAxis(-castAngle * 0.5f, lForwardMovementAxis) * transform.up * -1), SPHERE_RADIUS, out lBackwardHit, 1f) &
        //                Physics.SphereCast(new Ray(transform.position, Quaternion.AngleAxis(castAngle * 0.5f, lForwardMovementAxis) * transform.up * -1), SPHERE_RADIUS, out lForwardHit, 1f);
    }
}
