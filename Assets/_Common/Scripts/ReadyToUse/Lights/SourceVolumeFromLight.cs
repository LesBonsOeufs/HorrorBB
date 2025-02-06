using UnityEngine;

namespace Root
{
    public class SourceVolumeFromLight : MonoBehaviour
    {
        [SerializeField] private Light targetLight;
        [SerializeField] private AudioSource targetSource;
        [SerializeField] private float volumeMultiplier = 1f;

        private float initVolume;

        private void Start()
        {
            if (targetSource == null)
                targetSource = GetComponent<AudioSource>();

            if (targetSource != null)
                initVolume = targetSource.volume;
            else
                Debug.LogError("No audio source found. Please assign an audio source in the inspector or attach this script to an object with an audio source.");

            if (targetLight == null)
                Debug.LogError("No light assigned. Please assign a light in the inspector.");
        }

        private void Update()
        {
            if (targetLight != null && targetSource != null)
                targetSource.volume = initVolume * targetLight.intensity * volumeMultiplier;
        }
    }
}