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

        [Foldout("Step"), SerializeField] private float distanceForStep = 1f;
        [Foldout("Step"), SerializeField] private float stepVolumeCoeffPower = 2f;
        [Foldout("Step"), SerializeField] private float headBobbingCoeffPower = 1.1f;
        [Foldout("Step"), SerializeField] private HeadBobbing headbobbing;
        [Foldout("Step"), SerializeField] private AudioSource stepSource;

        [Foldout("Game Over"), SerializeField] private Image blackScreen;
        [Foldout("Game Over"), SerializeField] private float afterDeathDelay = 1f;
        [Foldout("Game Over"), SerializeField] private float afterDeathFadeDuration = 1f;
        [Foldout("Game Over"), SerializeField] private Transform respawnPoint;

#if UNITY_EDITOR
        [SerializeField] private bool godMode = false;
#endif

        public float speed = 2f;

        private CharacterController controller;
        private Interactor interactor;

        private float gravity = -9.81f;
        private Vector3 additionalVelocity;

        private Vector3 lastPosition;
        private float stepDistanceCounter = 0f;
        private float initStepVolume;

        private int deathCount = 0;

        [field: SerializeField] public CinemachineCamera CinemachineCam { get; private set; }
        public PlayerInput Input { get; private set; }
        public Vector3 MoveInput { get; private set; }
        public float InitSpeed { get; private set; }

        /// <summary>
        /// Parameter is death count
        /// </summary>
        public event Action<int> OnDeath;
        public event Action<E_Concealer> OnEnterConcealer;
        public event Action<E_Concealer> OnExitConcealer;

        protected override void Awake()
        {
            base.Awake();
            InitSpeed = speed;
            initStepVolume = stepSource.volume;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            controller = GetComponent<CharacterController>();
            interactor = GetComponent<Interactor>();
            Input = GetComponent<PlayerInput>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

        public void EnterConcealer(E_Concealer concealer)
        {
            SetInactiveMode(true);
            OnEnterConcealer?.Invoke(concealer);
        }

        public void ExitConcealer(E_Concealer concealer)
        {
            SetInactiveMode(false);
            OnExitConcealer?.Invoke(concealer);
        }

        public void SetInactiveMode(bool isHiding)
        {
            Input.SwitchCurrentActionMap(isHiding ? HIDE_ACTION_MAP : DEFAULT_ACTION_MAP);
            enabled = !isHiding;
        }

        public void ResetCamera()
        {
            CinemachineCam.ForceCameraPosition(CinemachineCam.transform.position, transform.rotation);
        }

        private void Update()
        {
            if (controller.isGrounded && additionalVelocity.y < 0)
                additionalVelocity.y = 0f;

            Vector3 lCameraBasedMoveInput = Quaternion.AngleAxis(Camera.main.transform.eulerAngles.y, Vector3.up) * MoveInput;
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
                    Step(speed / InitSpeed);
                    stepDistanceCounter = 0f;
                }
            }

            lastPosition = transform.position;
        }

        private void Step(float forceCoeff = 1f)
        {
            headbobbing.Execute(Mathf.Pow(forceCoeff, headBobbingCoeffPower));
            stepSource.volume = initStepVolume * Mathf.Pow(forceCoeff, stepVolumeCoeffPower);
            stepSource.Play();
        }

        [Button]
        //Quick & dirty use of SetHideMode
        public void Die()
        {
#if UNITY_EDITOR
            if (godMode)
                return;
#endif
            AudioListener.volume = 0f;
            blackScreen.color = Color.black;
            SetInactiveMode(true);
            transform.position = respawnPoint.position;
            CinemachineCam.ForceCameraPosition(CinemachineCam.transform.position, respawnPoint.rotation);
            OnDeath?.Invoke(++deathCount);

            DOVirtual.DelayedCall(afterDeathDelay, () =>
            {
                SetInactiveMode(false);
                DOVirtual.Float(0f, 1f, afterDeathFadeDuration, volume => AudioListener.volume =  volume);
                blackScreen.DOFade(0f, afterDeathFadeDuration);
            }, false);
        }

        #region Input Messages

        private void OnMove(InputValue inputValue)
        {
            Vector2 lInput = inputValue.Get<Vector2>();
            MoveInput = new Vector3(lInput.x, 0, lInput.y);
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