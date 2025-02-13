using Unity.Cinemachine;
using UnityEngine;

namespace Root
{
    public class HeadBobbing : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource source;
        [SerializeField] private CinemachineImpulseListener listener;

        private float baseGain;

        private void Awake()
        {
            baseGain = listener.ReactionSettings.AmplitudeGain;
        }

        public void Execute(float forceCoeff = 1f)
        {
            baseGain *= -1f;
            listener.ReactionSettings.AmplitudeGain = baseGain * forceCoeff;
            source.GenerateImpulse();
        }
    }
}