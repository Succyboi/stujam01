using UnityEngine;
using Cinemachine;

namespace Stupid.stujam01 {
    public class PlayerCameraController : MonoBehaviour {
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

        private float speed;
        private float speedSmoothingVelocity;
        private Vector3 sway;
        private Vector3 swaySmoothingVelocity;
        private CinemachineBasicMultiChannelPerlin cameraShake;
        private float deltaTime => Time.deltaTime;

        private void Start() {
            cameraShake = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update() {
            // HANDLE PROPERLY
            camera.Priority = player.NetworkObject.Authorized
                ? CameraConstants.PLAYER_ALIVE_PRIORITY
                : CameraConstants.MIN_PRIORITY;

            HandleReset(deltaTime);
            HandleSpeed(deltaTime);
            HandleSpeedWobble(deltaTime);
            HandleSway(deltaTime);
        }

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
    }
}