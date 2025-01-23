using Unity.Cinemachine;
using UnityEngine;

namespace Root
{
    public class HeadBobbing : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource source;
        [SerializeField] private CinemachineImpulseListener listener;

        public void Execute()
        {
            listener.ReactionSettings.AmplitudeGain *= -1f;
            source.GenerateImpulse();
        }
    }
}