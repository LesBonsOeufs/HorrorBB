using GE;
using UnityEngine;

public class SendGEOnDestroy : MonoBehaviour
{
    [SerializeField] private E_GlobalEvents globalEvent;

    private void OnDestroy()
    {
        GlobalEvents.Spread(globalEvent);
    }
}