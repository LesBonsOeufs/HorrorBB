using DG.Tweening;
using NaughtyAttributes;
using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Root
{
    [RequireComponent(typeof(CharacterController), typeof(Interactor), typeof(PlayerInput))]
    public class Player : Singleton<Player>
    {
        private const string DEFAULT_ACTION_MAP = "Player";
        private const string HIDE_ACTION_MAP = "Hiding";

        [SerializeField] private float speed = 2f;
        [SerializeField] private new CinemachineCamera camera;

        [Foldout("Step"), SerializeField] private float distanceForStep = 1f;
        [Foldout("Step"), SerializeField] private HeadBobbing headbobbing;
        [Foldout("Step"), SerializeField] private AudioSource stepSource;

        [Foldout("Game Over"), SerializeField] private Image blackScreen;
        [Foldout("Game Over"), SerializeField] private float afterDeathDelay = 1f;
        [Foldout("Game Over"), SerializeField] private float afterDeathFadeDuration = 1f;
        [Foldout("Game Over"), SerializeField] private Transform respawnPoint;

        private CharacterController controller;
        private Interactor interactor;

        private float gravity = -9.81f;
        private Vector3 moveInput;
        private Vector3 additionalVelocity;

        private Vector3 lastPosition;
        private float stepDistanceCounter = 0f;

        private int deathCount = 0;

        public PlayerInput Input { get; private set; }

        /// <summary>
        /// Parameter is death count
        /// </summary>
        public event Action<int> OnDeath;

        protected override void Awake()
        {
            base.Awake();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            controller = GetComponent<CharacterController>();
            interactor = GetComponent<Interactor>();
            Input = GetComponent<PlayerInput>();
        }

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void OnEnable()
        {
            controller.enabled = true;
            interactor.enabled = true;
        }

        private void OnDisable()
        {
            controller.enabled = false;
            interactor.enabled = false;
        }

        public void SetHideMode(bool isHiding)
        {
            Input.SwitchCurrentActionMap(isHiding ? HIDE_ACTION_MAP : DEFAULT_ACTION_MAP);
            enabled = !isHiding;
        }

        public void ResetCamera()
        {
            camera.ForceCameraPosition(camera.transform.position, transform.rotation);
        }

        private void Update()
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
            stepSource.Play();
        }

        [Button]
        //Quick & dirty use of SetHideMode
        public void Die()
        {
            AudioListener.volume = 0f;
            blackScreen.color = Color.black;
            SetHideMode(true);
            transform.position = respawnPoint.position;
            camera.ForceCameraPosition(camera.transform.position, respawnPoint.rotation);
            OnDeath?.Invoke(++deathCount);

            DOVirtual.DelayedCall(afterDeathDelay, () =>
            {
                SetHideMode(false);
                DOVirtual.Float(0f, 1f, afterDeathFadeDuration, volume => AudioListener.volume =  volume);
                blackScreen.DOFade(0f, afterDeathFadeDuration);
            });
        }

        #region Player Input Messages

        private void OnMove(InputValue inputValue)
        {
            Vector2 lInput = inputValue.Get<Vector2>();
            moveInput = new Vector3(lInput.x, 0, lInput.y);
        }

        private void OnInteract(InputValue inputValue)
        {
            if (inputValue.isPressed)
                interactor.StartInteraction();
            else
                interactor.StopInteraction();
        }

        #endregion
    }
}