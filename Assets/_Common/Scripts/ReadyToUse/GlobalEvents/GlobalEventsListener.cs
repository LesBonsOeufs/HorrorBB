using UnityEngine;
using UnityEngine.Events;

namespace GE
{
    public class GlobalEventsListener : MonoBehaviour
    {
        [SerializeField] private E_GlobalEvents eventToAnswer;
        [SerializeField] private UnityEvent reaction;

        private void Awake()
        {
            GlobalEvents.OnSpread += GlobalEvents_OnSpread;
        }

        private void GlobalEvents_OnSpread(E_GlobalEvents globalEvent)
        {
            if (globalEvent == eventToAnswer)
                reaction?.Invoke();
        }
    }
}