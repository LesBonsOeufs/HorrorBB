using UnityEngine.Audio;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using System.Collections.Generic;

public class SoundObstruction : MonoBehaviour
{
    [SerializeField] private float checkMaxDistance = 10f;
    [SerializeField, InfoBox("Sources closer than this will not be muffled")] private float minDistance = 0.7f;
    [SerializeField] private bool isObstructedAfterMax = false;
    [SerializeField, InfoBox("Sources that are not in either groups will not be touched")] private AudioMixerGroup directGroup;
    [SerializeField] private AudioMixerGroup obstructedGroup;
    [SerializeField] private LayerMask obstructorMask = ~0;
    [SerializeField, Range(0f, 1f)] private float minSpatialBlend = 0.3f;

    private IEnumerable<AudioSource> audioSources;

    private void Start()
    {
        RefreshAudioSources();
    }

    public void RefreshAudioSources()
    {
        audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(
            source => (source.outputAudioMixerGroup == obstructedGroup || source.outputAudioMixerGroup == directGroup) &&
            source.spatialBlend > minSpatialBlend);
    }

    private void Update()
    {
        foreach (AudioSource lSource in audioSources)
        {
            Vector3 lDirection = lSource.transform.position - transform.position;
            float lDistance = lDirection.magnitude;

            if (lDistance <= minDistance)
            {
                SoundToDirect(lSource);
                continue;
            }

            if (lDistance <= checkMaxDistance)
            {
                if (Physics.Raycast(transform.position, lDirection.normalized, out _, lDistance, obstructorMask, QueryTriggerInteraction.Ignore))
                    SoundToObstruct(lSource);
                else
                    SoundToDirect(lSource);
            }
            else
            {
                if (isObstructedAfterMax)
                    SoundToObstruct(lSource);
                else
                    SoundToDirect(lSource);
            }
        }
    }

    void SoundToObstruct(AudioSource source)
    {
        source.outputAudioMixerGroup = obstructedGroup;
    }

    void SoundToDirect(AudioSource source)
    {
        source.outputAudioMixerGroup = directGroup;
    }
}