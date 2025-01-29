using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Root
{
    public class DoorAnim : MonoBehaviour
    {
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 1f;
        [SerializeField] private Ease ease = Ease.InOutSine;

        private Tween rotationTween;
        private Vector3 initialRotation;

        private void Awake()
        {
            initialRotation = transform.rotation.eulerAngles;
        }

        [Button]
        public void Open()
        {
            rotationTween?.Kill();
            rotationTween = transform.DORotate(initialRotation + new Vector3(0f, openAngle, 0f), openSpeed).SetEase(ease);
        }

        [Button]
        public void Close()
        {
            rotationTween?.Kill();
            rotationTween = transform.DORotate(initialRotation, openSpeed).SetEase(ease);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0f, openAngle, 0f) * transform.forward);
        }
    }
}