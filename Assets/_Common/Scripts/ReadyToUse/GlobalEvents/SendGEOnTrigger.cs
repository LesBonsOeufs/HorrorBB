using NaughtyAttributes;
using UnityEngine;

namespace GE
{
    [RequireComponent(typeof(Collider))]
    public class SendGEOnTrigger : MonoBehaviour
    {
        [SerializeField, Tag] private string requiredTag = "Player";
        [SerializeField] private E_GlobalEvents globalEvent;
        [SerializeField] private E_GlobalEvents[] previouslyRequiredEvents;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(requiredTag))
                return;

            foreach (E_GlobalEvents lRequiredEvents in previouslyRequiredEvents)
            {
                if (!GlobalEvents.Instance.WasSpread(lRequiredEvents))
                    return;
            }

            GlobalEvents.Instance.Spread(globalEvent);
        }

        private void OnValidate()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            GlobalEvents.ToGEGizmosColor();
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
        }
    }
}