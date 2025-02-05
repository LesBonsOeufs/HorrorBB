using UnityEngine.Audio;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using System.Collections.Generic;

public class SoundObstruction : MonoBehaviour
{
    [SerializeField, InfoBox("Sources closer than this will not be muffled")] private float minDistance = 0.7f;
    [SerializeField] private float checkMaxDistance = 10f;
    [SerializeField] private bool isObstructedAfterMax = false;
    [HorizontalLine]
    [SerializeField, InfoBox("Sources that are not in either groups will not be touched.\nNon-obstructable sounds should be placed in a separate group.")]
    private AudioMixerGroup directGroup;
    [SerializeField] private AudioMixerGroup obstructedGroup;
    [HorizontalLine]
    [SerializeField] private LayerMask obstructorMask = ~0;

    private IEnumerable<AudioSource> audioSources;

    private void Start()
    {
        RefreshAudioSources();
    }

    public void RefreshAudioSources()
    {
        audioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(
            source => source.outputAudioMixerGroup == obstructedGroup || source.outputAudioMixerGroup == directGroup);
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