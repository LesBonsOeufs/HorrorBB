using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

namespace Root
{
    public class CrawlingMan : MonoBehaviour
    {
        [SerializeField] private float speed = .5f;
        [SerializeField] private float rotationSpeed = 2f;

        private Vector3 lastPosition;
        private float initialElevation;

        private void Start()
        {
            lastPosition = transform.position;
            if (Physics.Raycast(new Ray(transform.position, transform.up * -1), out RaycastHit lHit, 1f))
                initialElevation = lHit.distance;
            else
                initialElevation = 1f;
        }

        private void Update()
        {
            RaycastHit lForwardHit;
            RaycastHit lDownHit;
            Vector3 lLocalDirection = Vector3.forward;
            Vector3 lForwardMovement = (transform.position - lastPosition).normalized;

            bool lBothHit = Physics.SphereCast(new Ray(transform.position, transform.up * -1), 0.2f, out lDownHit, 1f) &
                            Physics.SphereCast(new Ray(transform.position, lForwardMovement), 0.2f, out lForwardHit, 1f);

            Vector3 lAveragePoint = (lForwardHit.point + lDownHit.point) * (lBothHit ? 0.5f : 1f);

            //KNOWN ISSUE: when changing surface, normal stops lerping and goes to new surface as soon as the down spherecast hits it.
            //Slerping rotation is a quick fix.
            Vector3 lAverageNormal = ((lForwardHit.normal * (1f - lForwardHit.distance)) + 
                (lDownHit.normal * (1f - lDownHit.distance))).normalized;

            Quaternion FromTo = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.up, lAverageNormal, transform.right), transform.right);
            lastPosition = transform.position;
            transform.position = lAveragePoint + (transform.position - lAveragePoint).normalized * initialElevation;
            transform.position += FromTo * lLocalDirection * Time.deltaTime * speed;
            Debug.DrawRay(transform.position, lAverageNormal, Color.red);

            Quaternion lTargetRotation = transform.rotation * Quaternion.FromToRotation(transform.up, lAverageNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
