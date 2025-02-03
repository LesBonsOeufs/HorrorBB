using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Root
{
    public class Ambient_Clock : MonoBehaviour
    {
        [SerializeField] private int nRotationsOnStart = 0;
        [SerializeField] private float rotationWaitDuration = 1f;
        [SerializeField] private float rotationDuration = 0.5f;
        [SerializeField, Range(1f, 32f)] private int nRotationsForFull = 16;
        [SerializeField] private Transform bigHand;
        [SerializeField] private Transform smallHand;
        [SerializeField] private AudioSource bigHandMoveSFX;

        [ShowNativeProperty] private float BigHandTimeForFull => (rotationDuration + rotationWaitDuration) * nRotationsForFull;
        [ShowNativeProperty] private float DegreesPerRotation => 360f / nRotationsForFull;
        [ShowNativeProperty] private float SmallHandNRotationsForFull => nRotationsForFull * nRotationsForFull;

        private void Awake()
        {
            Init();
            ExecuteTweens();
        }

        private void Init()
        {
            float lBigHandAddedDegrees = nRotationsOnStart * DegreesPerRotation;
            bigHand.localRotation = Quaternion.AngleAxis(lBigHandAddedDegrees, bigHand.forward);
            smallHand.localRotation = Quaternion.AngleAxis(Mathf.Floor(lBigHandAddedDegrees / 360f) * DegreesPerRotation, smallHand.forward);
        }

        private void ExecuteTweens()
        {
            this.DOKill(true);

            DOTween.Sequence(this)
                .AppendCallback(OnNewRotationStart)
                .Append(bigHand.DOBlendableLocalRotateBy(new(0f, 0f, DegreesPerRotation), rotationDuration).SetEase(Ease.OutBack))
                .AppendInterval(rotationWaitDuration)
                .SetLoops(-1, LoopType.Incremental);
        }

        private void OnNewRotationStart()
        {
            bigHandMoveSFX.Play();

            if (Mathf.Abs(bigHand.localRotation.z) % 360f <= 0.001f)
                smallHand.DOBlendableLocalRotateBy(new(0f, 0f, DegreesPerRotation), rotationDuration).SetEase(Ease.OutBack);
        }

        private void OnValidate()
        {
            Init();
        }
    }
}