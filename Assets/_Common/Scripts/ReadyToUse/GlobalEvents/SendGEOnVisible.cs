using NaughtyAttributes;
using UnityEngine;

namespace GE
{
    public class SendGEOnVisible : MonoBehaviour
    {
        private const float SECONDS_PER_UPDATE = 0.25f;

        [SerializeField] private bool useMainCamera = true;
        [HideIf(nameof(useMainCamera)), SerializeField] private new Camera camera;

        [SerializeField] private E_GlobalEvents globalEvent;
        [SerializeField] private E_GlobalEvents[] previouslyRequiredEvents;
        [SerializeField] private float maxDistance = Mathf.Infinity;
        [SerializeField, Tooltip("Use for triggering the event only if the player is looking for more than X updates")] 
        private int requiredConsecutiveUpdates = 0;

        private float counter = 0f;
        private int consecutiveUpdates = 0;

        private void Update()
        {
            counter += Time.deltaTime;

            if (counter < SECONDS_PER_UPDATE)
                return;

            counter = 0f;

            foreach (E_GlobalEvents lRequiredEvents in previouslyRequiredEvents)
            {
                if (!GlobalEvents.Instance.WasSpread(lRequiredEvents))
                    return;
            }

            Camera lCamera = useMainCamera ? Camera.main : camera;

            if (lCamera != null && lCamera.IsPointVisible(transform.position) && (lCamera.transform.position - transform.position).magnitude < maxDistance)
                consecutiveUpdates++;
            else
                consecutiveUpdates = 0;

            if (consecutiveUpdates > requiredConsecutiveUpdates)
                GlobalEvents.Instance.Spread(globalEvent);
        }

        private void OnDrawGizmos()
        {
            GlobalEvents.ToGEGizmosColor();
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawFrustum(-transform.forward * 0.15f, 60f, 0.1f, 0f, 1f);
            Gizmos.DrawWireSphere(Vector3.zero, maxDistance);
        }
    }
}