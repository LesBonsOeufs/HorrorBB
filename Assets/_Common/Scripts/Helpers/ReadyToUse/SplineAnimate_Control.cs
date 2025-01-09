using DG.Tweening;
using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Root
{
    public class SplineAnimate_Control : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private float animDuration = 2f;
        [SerializeField] private float targetDistance = 0f;
        [SerializeField] private float[] registeredTargets = new float[] { };

        private int currentTargetIndex = 0;
        private float currentDistance;
        private Tween anim;
        
        private void Start()
        {
            targetDistance = registeredTargets[currentTargetIndex];
            currentDistance = targetDistance;
            Apply();
        }

        [Button]
        private void ToNext()
        {
            currentTargetIndex++;

            if (currentTargetIndex >= registeredTargets.Length)
                currentTargetIndex = 0;

            GoToTarget();
        }

        [Button]
        private void ToPrevious()
        {
            currentTargetIndex--;

            if (currentTargetIndex < 0)
                currentTargetIndex = registeredTargets.Length - 1;

            GoToTarget();
        }

        private void GoToTarget()
        {
            targetDistance = registeredTargets[currentTargetIndex];
            anim.Kill();
            anim = DOVirtual.Float(currentDistance, targetDistance, animDuration, updatedDistance =>
            {
                currentDistance = updatedDistance;
                Apply();
            });
        }

        [Button]
        private void ForceApply()
        {
            anim.Kill();
            currentDistance = targetDistance;
            Apply();
        }

        private void Apply()
        {
            splineContainer.Evaluate(currentDistance, out float3 lPosition, out float3 lTangent, out float3 lUp);
            transform.SetPositionAndRotation(lPosition, Quaternion.LookRotation(lTangent, lUp));
        }
    }
}
