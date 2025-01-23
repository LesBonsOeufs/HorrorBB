using UnityEngine;
using UnityEngine.InputSystem;

namespace Root
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        [SerializeField]private float speed = 2.0f;

        private CharacterController controller;
        private float gravity = -9.81f;
        private Vector3 moveInput;
        private Vector3 additionalVelocity;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (controller.isGrounded && additionalVelocity.y < 0)
                additionalVelocity.y = 0f;

            Vector3 lCameraBasedMoveInput = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * moveInput;
            controller.Move(speed * Time.deltaTime * lCameraBasedMoveInput);
            additionalVelocity.y += gravity * Time.deltaTime;
            controller.Move(additionalVelocity * Time.deltaTime);
        }

        #region Player Input Messages

        private void OnMove(InputValue inputValue)
        {
            Vector2 lInput = inputValue.Get<Vector2>();
            moveInput = new Vector3(lInput.x, 0, lInput.y);
        }

        #endregion
    }
}