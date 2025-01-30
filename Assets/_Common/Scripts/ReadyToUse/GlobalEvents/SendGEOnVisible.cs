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

        private float counter = 0f;

        private void Update()
        {
            counter += Time.deltaTime;

            if (counter < SECONDS_PER_UPDATE)
                return;

            foreach (E_GlobalEvents lRequiredEvents in previouslyRequiredEvents)
            {
                if (!GlobalEvents.Instance.WasSpread(lRequiredEvents))
                    return;
            }

            Camera lCamera = useMainCamera ? Camera.main : camera;

            if (lCamera != null && lCamera.IsPointVisible(transform.position))
                GlobalEvents.Instance.Spread(globalEvent);
        }

        private void OnDrawGizmos()
        {
            GlobalEvents.ToGEGizmosColor();
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawFrustum(-transform.forward * 0.15f, 60f, 0.1f, 0f, 1f);
        }
    }
}