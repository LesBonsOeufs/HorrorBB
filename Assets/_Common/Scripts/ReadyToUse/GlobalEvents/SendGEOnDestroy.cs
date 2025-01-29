using UnityEngine;

namespace GE
{
    public class SendGEOnDestroy : MonoBehaviour
    {
        [SerializeField] private E_GlobalEvents globalEvent;

        private void OnDestroy()
        {
            if (GlobalEvents.Instance == null)
                return;

            GlobalEvents.Instance.Spread(globalEvent);
        }
    }
}