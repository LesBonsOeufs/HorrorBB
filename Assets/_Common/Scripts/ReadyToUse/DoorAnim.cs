using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

namespace Root
{
    public class DoorAnim : MonoBehaviour
    {
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 1f;
        [SerializeField] private Ease ease = Ease.InOutSine;

        [Foldout("Audio"), SerializeField] private AudioSource sfxSource;
        [Foldout("Audio"), SerializeField] private AudioResource openSFX;
        [Foldout("Audio"), SerializeField] private AudioResource closeSFX;
        [Foldout("Audio"), SerializeField] private AudioResource closeEndSFX;

        private Tween rotationTween;
        private Vector3 initialRotation;

        private void Awake()
        {
            initialRotation = transform.rotation.eulerAngles;
        }

        [Button]
        public void Open()
        {
            if (sfxSource != null && openSFX != null)
            {
                sfxSource.resource = openSFX;
                sfxSource.Play();
            }

            rotationTween?.Kill();
            rotationTween = transform.DORotate(initialRotation + new Vector3(0f, openAngle, 0f), openSpeed).SetEase(ease);
        }

        [Button]
        public void Close()
        {
            if (sfxSource != null && closeSFX != null)
            {
                sfxSource.resource = closeSFX;
                sfxSource.Play();
            }

            rotationTween?.Kill();
            rotationTween = transform.DORotate(initialRotation, openSpeed).SetEase(ease)
                .OnComplete(OnCloseEnd);
        }

        private void OnCloseEnd()
        {
            if (sfxSource != null && closeEndSFX != null)
            {
                sfxSource.resource = closeEndSFX;
                sfxSource.Play();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, openAngle, 0f) * transform.forward);
        }
    }
}