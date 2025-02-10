using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Root
{
    //METHOD FOR "USED" IS QUICK AND DIRTY!!
    public class Lever : OutlinedInteractable
    {
        [SerializeField] private UnityEvent onUsed;
        [SerializeField] private Transform handle;
        [Foldout("Tweening"), SerializeField] private float animDuration = 1f;
        [Foldout("Tweening"), SerializeField] private Vector3 animEndRotation;

        private bool used = false;

        public override void InteractorEnter()
        {
            if (used)
                return;

            base.InteractorEnter();
        }

        public override void InteractorExit()
        {
            if (used)
                return;

            base.InteractorExit();
        }

        public override void Interact(Interactor interactor, bool isInteracting)
        {
            if (used)
                return;

            base.Interact(interactor, isInteracting);

            if (isInteracting)
            {
                used = true;
                outline.enabled = false;
                handle.DOLocalRotate(animEndRotation, animDuration)
                    .SetEase(Ease.InSine)
                    .OnComplete(() => onUsed?.Invoke());
            }
        }
    }
}