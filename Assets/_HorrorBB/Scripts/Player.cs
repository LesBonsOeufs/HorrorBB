using UnityEngine;
using UnityEngine.InputSystem;

namespace Root
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private float speed = 2f;
        [SerializeField] private float distanceForStep = 1f;
        [SerializeField] private HeadBobbing headbobbing;

        private CharacterController controller;
        private float gravity = -9.81f;
        private Vector3 moveInput;
        private Vector3 additionalVelocity;

        private Vector3 lastPosition;
        private float stepDistanceCounter = 0f;

        [SerializeField] private float testStepCd = 0.5f;
        [SerializeField] private bool testStepRepeating = false;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            controller = GetComponent<CharacterController>();

            if (testStepRepeating)
                InvokeRepeating(nameof(Step), 0f, testStepCd);

            lastPosition = transform.position;
        }

        void Update()
        {
            if (controller.isGrounded && additionalVelocity.y < 0)
                additionalVelocity.y = 0f;

            Vector3 lCameraBasedMoveInput = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * moveInput;
            controller.Move(speed * Time.deltaTime * lCameraBasedMoveInput);
            additionalVelocity.y += gravity * Time.deltaTime;
            controller.Move(additionalVelocity * Time.deltaTime);

            if (lastPosition == transform.position)
                stepDistanceCounter = 0f;
            else
            {
                stepDistanceCounter += new Vector3(transform.position.x - lastPosition.x, 0f, transform.position.z - lastPosition.z).magnitude;

                if (stepDistanceCounter >= distanceForStep)
                {
                    Step();
                    stepDistanceCounter = 0f;
                }
            }

            lastPosition = transform.position;
        }

        private void Step()
        {
            headbobbing.Execute();
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