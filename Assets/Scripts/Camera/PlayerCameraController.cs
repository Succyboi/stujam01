using UnityEngine;
using Cinemachine;

namespace Stupid.stujam01 {
    public class PlayerCameraController : MonoBehaviour {
        public Vector3 Sway => sway;

        [Header("Speed")]
        [SerializeField] private float speedSmoothingTime;

        [Header("Speed wobble")]
        [SerializeField] private AnimationCurve wobbleCurve;

        [Header("Sway")]
        [SerializeField] private float swaySmoothingTime;
        [SerializeField] private float swayMaxSpeed;
        [SerializeField] private float swayAmount;

        [Header("References")]
        [SerializeField] private NetworkedPlayer player;
        [SerializeField] private new CinemachineVirtualCamera camera;
        [SerializeField] private CinemachineVirtualCamera deathCamera;

        private float speed;
        private float speedSmoothingVelocity;
        private Vector3 sway;
        private Vector3 swaySmoothingVelocity;
        private CinemachineBasicMultiChannelPerlin cameraShake;
        private float deltaTime => Time.deltaTime;

        private PlayerScreen screen => PlayerScreen.Instance;

        #region MonoBehaviour

        private void Start() {
            cameraShake = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update() {
            // HANDLE PROPERLY
            camera.Priority = player.IsLocalHumanPlayer
                ? CameraConstants.PLAYER_ALIVE_PRIORITY
                : CameraConstants.MIN_PRIORITY;

            HandleReset(deltaTime);
            HandleSpeed(deltaTime);
            HandleSpeedWobble(deltaTime);
            HandleSway(deltaTime);
        }

        public void Initialize() {
            if (player.IsLocalHumanPlayer) {
                screen.Initialize(player, this);
            }

            player.OnDied += HandlePlayerDied;
            player.OnSpawned += HandlePlayerSpawned;
        }

        public void UnInitialize() {
            if (player.IsLocalHumanPlayer) {
                screen.UnInitialize();
            }

            player.OnDied -= HandlePlayerDied;
            player.OnSpawned -= HandlePlayerSpawned;
        }

        private void HandlePlayerDied(NetworkedPlayer killer, Dodgeball dodgeball) {
            deathCamera.Priority = player.IsLocalHumanPlayer ? CameraConstants.PLAYER_DEAD_PRIORITY : CameraConstants.MIN_PRIORITY;
        }

        private void HandlePlayerSpawned() {
            deathCamera.Priority = CameraConstants.MIN_PRIORITY;
        }

        #endregion

        #region Main Camera

        private void HandleReset(float deltaTime) {
            transform.localPosition = Vector3.zero;
        }

        private void HandleSpeed(float deltaTime) {
            speed = Mathf.SmoothDamp(speed, player.Speed, ref speedSmoothingVelocity, speedSmoothingTime, Mathf.Infinity, deltaTime);
        }

        private void HandleSpeedWobble(float deltaTime) {
            cameraShake.m_AmplitudeGain = wobbleCurve.Evaluate(speed);
        }

        private void HandleSway(float deltaTime) {
            Vector3 rawSway = transform.rotation * player.Velocity.normalized * player.Speed / swayMaxSpeed;
            rawSway = rawSway.magnitude > 1f ? rawSway.normalized : rawSway;
            sway = Vector3.SmoothDamp(sway, rawSway, ref swaySmoothingVelocity, swaySmoothingTime, Mathf.Infinity, deltaTime);

            transform.localPosition = -sway * swayAmount;
        }

        #endregion
    }
}