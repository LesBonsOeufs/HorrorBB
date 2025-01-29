using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Root
{
    public class Concealer : OutlinedInteractable
    {
        [SerializeField] private InputActionReference outActionRef;
        [SerializeField] private new CinemachineCamera camera;
        [SerializeField, Range(0f, 1f)] private float blendPercentageForOut = 0.9f;
        private Vector3 entryPosition;

        public override void Interact(Interactor interactor, bool isInteracting)
        {
            base.Interact(interactor, isInteracting);

            if (isInteracting)
            {
                camera.enabled = true;
                entryPosition = interactor.transform.position;
                Player.Instance.SetHideMode(true);
                StartCoroutine(OutCoroutine());
            }
        }

        private IEnumerator OutCoroutine()
        {
            Player lPlayer = Player.Instance;

            while (true)
            {
                InputAction lAction = GetInputActionFromRef(lPlayer.Input, outActionRef);
                if (lAction.triggered && lAction.IsPressed())
                    break;

                yield return null;
            }

            Vector3 lCameraToEntry = entryPosition - camera.transform.position;
            lCameraToEntry.y = 0f;
            lPlayer.transform.rotation = Quaternion.LookRotation(lCameraToEntry, lPlayer.transform.up);
            lPlayer.ResetCamera();
            camera.enabled = false;
            yield return null;

            CinemachineBlend lBlend = Camera.main.GetComponent<CinemachineBrain>().ActiveBlend;

            if (lBlend != null)
                yield return new WaitForSeconds((lBlend.Duration - lBlend.TimeInBlend) * blendPercentageForOut);

            lPlayer.SetHideMode(false);
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