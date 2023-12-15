using HiHi;
using UnityEngine;

namespace Stupid.stujam01 {
    public class DodgeballAudio : MonoBehaviour {
        [Header("Bounce")]
        [SerializeField] private Sound bounce;
        [SerializeField] private float bounceSpeedThreshold;

        [Header("Woosh")]
        [SerializeField] private float wooshMinSpeed;
        [SerializeField] private float wooshMaxSpeed;
        [SerializeField] private float wooshVolume;
        [SerializeField] private float wooshSmoothingDuration;

        [Header("References")]
        [SerializeField] private Dodgeball dodgeball;
        [SerializeField] private AudioSource wooshSource;

        private float deltaTime => Time.deltaTime;
        private float time => Time.time;

        private bool initialized;

        public void Initialize() {
            dodgeball.OnBounce += HandleBounce;
            dodgeball.OnThrow += HandleThrow;
            dodgeball.OnIdle += HandleIdle;

            initialized = true;
        }

        public void Uninitialize() {
            dodgeball.OnBounce -= HandleBounce;
            dodgeball.OnThrow -= HandleThrow;
            dodgeball.OnIdle -= HandleIdle;

            initialized = false;
        }

        private void HandleBounce() {
            if (dodgeball.Velocity.magnitude < bounceSpeedThreshold) { return; }

            bounce.Play(dodgeball.Position);
        }

        private void HandleThrow() {
            wooshSource.Play();
        }

        private void HandleIdle() {
            if (wooshSource.isPlaying) {
                wooshSource.Stop();
            }
        }

        private void Update() {
            if (!initialized) { return; }

            HandleWoosh(deltaTime);
        }

        private void HandleWoosh(float deltaTime) {
            wooshSource.volume = Mathf.MoveTowards(wooshSource.volume, wooshVolume * Mathf.Clamp01(Mathf.InverseLerp(wooshMinSpeed, wooshMaxSpeed, dodgeball.Velocity.magnitude)), deltaTime / wooshSmoothingDuration);

            if (wooshSource.isPlaying && dodgeball.Idle && wooshSource.volume <= 0f) {
                wooshSource.Stop();
            }
        }
    }
}