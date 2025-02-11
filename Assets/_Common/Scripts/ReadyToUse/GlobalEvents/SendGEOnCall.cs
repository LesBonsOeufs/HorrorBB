using UnityEngine;

namespace GE
{
    public class SendGEOnCall : MonoBehaviour
    {
        [SerializeField] private E_GlobalEvents globalEvent;

        public void Execute()
        {
            GlobalEvents.Instance.Spread(globalEvent);
        }
    }
}