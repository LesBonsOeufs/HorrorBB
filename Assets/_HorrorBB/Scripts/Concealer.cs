using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using NaughtyAttributes;
using DG.Tweening;

namespace Root
{
    //Quick fix for strange dictionary bug when using class ref
    public enum E_Concealer
    {
        SHOWER,
        WARDROBE
    }

    public class Concealer : OutlinedInteractable
    {
        [SerializeField] private E_Concealer identity;
        [Foldout("Events"), SerializeField] private UnityEvent onStartEnter;
        [Foldout("Events"), SerializeField] private UnityEvent onEndEnter;
        [Foldout("Events"), SerializeField] private UnityEvent onStartLeave;
        [Foldout("Events"), SerializeField] private UnityEvent onEndLeave;

        [SerializeField] private InputActionReference outActionRef;
        [SerializeField] private new CinemachineCamera camera;
        [SerializeField, Range(0f, 1f)] private float inBlendRatio = 0.6f;
        [SerializeField, Range(0f, 1f)] private float outBlendRatio = 1f;

        private Vector3 entryPosition;
        private Tween endEnterTween;

        private float? GetCurrentBlendRemainingTime()
        {
            CinemachineBlend lBlend = Camera.main.GetComponent<CinemachineBrain>().ActiveBlend;
            return lBlend == null ? null : (lBlend.Duration - lBlend.TimeInBlend);
        }

        public override void Interact(Interactor interactor, bool isInteracting)
        {
            base.Interact(interactor, isInteracting);

            if (isInteracting)
            {
                onStartEnter?.Invoke();
                camera.enabled = true;
                entryPosition = interactor.transform.position;
                Player.Instance.EnterConcealer(identity);
                StartCoroutine(OutCoroutine());
            }
        }

        private IEnumerator OutCoroutine()
        {
            //Wait one frame for camera blend to start
            yield return null;

            float? lRemainingBlendTime = GetCurrentBlendRemainingTime();

            if (lRemainingBlendTime != null)
                yield return new WaitForSeconds(lRemainingBlendTime.Value * inBlendRatio);

            onEndEnter?.Invoke();

            Player lPlayer = Player.Instance;

            while (true)
            {
                InputAction lAction = GetInputActionFromRef(lPlayer.Input, outActionRef);
                if (lAction.triggered && lAction.IsPressed())
                    break;

                yield return null;
            }

            endEnterTween?.Kill();
            onStartLeave?.Invoke();
            Vector3 lCameraToEntry = entryPosition - camera.transform.position;
            lCameraToEntry.y = 0f;
            lPlayer.transform.rotation = Quaternion.LookRotation(lCameraToEntry, lPlayer.transform.up);
            lPlayer.ResetCamera();
            camera.enabled = false;
            yield return null;

            lRemainingBlendTime = GetCurrentBlendRemainingTime();

            if (lRemainingBlendTime != null)
                yield return new WaitForSeconds(lRemainingBlendTime.Value * outBlendRatio);

            onEndLeave?.Invoke();
            lPlayer.ExitConcealer(identity);
        }

        private InputAction GetInputActionFromRef(PlayerInput input, InputActionReference reference)
        {
            IEnumerator<InputAction> lActions = input.user.actions.GetEnumerator();

            while (lActions.MoveNext())
                if (lActions.Current.id == reference.action.id)
                    return lActions.Current;

            return null;
        }
    }
}