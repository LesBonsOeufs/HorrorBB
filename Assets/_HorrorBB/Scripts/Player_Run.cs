using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Root
{
    [RequireComponent(typeof(Player))]
    public class Player_Run : MonoBehaviour
    {
        [SerializeField] private float baseRunSpeed = 4f;
        [SerializeField, MinMaxSlider(0f, 10f)] private Vector2 minMaxStaminaLimit = new(3f, 5f);
        [SerializeField] private float staminaRecoveryRate = 0.8f;

        [Foldout("Visuals"), SerializeField] private Volume tirednessVolume;
        [Foldout("Visuals"), SerializeField] private Ease volumeWeightEasing = Ease.InSine;
        [Foldout("Visuals"), SerializeField] private float volumeWeightStartTiredValue = 0.35f;
        [Foldout("Visuals"), SerializeField] private float volumeWeightSmoothSpeed = 0.15f;
        [Foldout("Visuals"), SerializeField] private float runningCameraFov = 65f;
        [Foldout("Visuals"), SerializeField] private float cameraFovSmoothSpeed = 0.15f;

        [Foldout("SFX"), SerializeField] private AudioSource breathSource;
        [Foldout("SFX"), SerializeField] private float breathVolumeSmoothSpeed = 0.15f;
        [Foldout("SFX"), SerializeField, MinMaxSlider(0f, 1f)] private Vector2 minMaxRunningBreathVolume = new (0.07f, 0.5f);

        [SerializeField, ReadOnly] private float stamina;

        private bool runInput = false;
        private Player player;

        private float baseCamFov;

        private float targetBreathVolume = 0f;
        private float _breathVolumeVelocityForSmoothDamp;

        private float targetCameraFov;
        private float _cameraFovVelocityForSmoothDamp;

        private float _volumeWeightVelocityForSmoothDamp;

        public bool IsRunning
        {
            get => _isRunning;

            private set
            {
                if (_isRunning == value)
                    return;

                _isRunning = value;

                if (!_isRunning)
                    player.speed = player.InitSpeed;

                targetCameraFov = _isRunning ? runningCameraFov : baseCamFov;
            }
        }
        private bool _isRunning = false;

        private void Awake()
        {
            stamina = 0f;
            player = GetComponent<Player>();
            baseCamFov = player.CinemachineCam.Lens.FieldOfView;
            targetCameraFov = baseCamFov;
        }

        private void Update()
        {
            IsRunning = runInput && player.MoveInput != Vector3.zero;
            UpdateEffects();
            float lTiredness = Mathf.InverseLerp(minMaxStaminaLimit.x, minMaxStaminaLimit.y, stamina);

            if (IsRunning)
            {
                targetBreathVolume = stamina > minMaxStaminaLimit.x ? minMaxRunningBreathVolume.y : minMaxRunningBreathVolume.x;
                player.speed = Mathf.Lerp(baseRunSpeed, player.InitSpeed, lTiredness);
                stamina += Time.deltaTime;

                if (stamina > minMaxStaminaLimit.y)
                    stamina = minMaxStaminaLimit.y;
            }
            else
            {
                targetBreathVolume = lTiredness;
                stamina -= Time.deltaTime * staminaRecoveryRate;

                if (stamina < 0f)
                    stamina = 0f;
            }
        }

        private void UpdateEffects()
        {
            player.CinemachineCam.Lens.FieldOfView =
                Mathf.SmoothDamp(player.CinemachineCam.Lens.FieldOfView, targetCameraFov, ref _cameraFovVelocityForSmoothDamp, cameraFovSmoothSpeed);
            breathSource.volume =
                Mathf.SmoothDamp(breathSource.volume, targetBreathVolume, ref _breathVolumeVelocityForSmoothDamp, breathVolumeSmoothSpeed);

            float lTargetVolumeWeight = DOVirtual.EasedValue(0f, 1f, Mathf.InverseLerp(minMaxStaminaLimit.x, minMaxStaminaLimit.y, stamina), volumeWeightEasing);

            if (lTargetVolumeWeight > 0f)
                lTargetVolumeWeight = Mathf.Lerp(volumeWeightStartTiredValue, 1f, lTargetVolumeWeight);

            tirednessVolume.weight =
                Mathf.SmoothDamp(tirednessVolume.weight, lTargetVolumeWeight, ref _volumeWeightVelocityForSmoothDamp, volumeWeightSmoothSpeed);
        }

        #region Input Messages

        private void OnRun(InputValue inputValue)
        {
            runInput = inputValue.isPressed;
        }

        #endregion
    }
}