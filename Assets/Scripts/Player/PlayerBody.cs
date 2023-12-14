using HiHi;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Stupid.stujam01 {
    public class PlayerBody : MonoBehaviour {
        [Serializable]
        private class BodyPart {
            public Transform Transform;
            public Rigidbody Rigidbody;
            public Collider Collider;

            private Vector3 originalPosition { get; set; }
            private Quaternion originalRotation { get; set; }

            public void Initialize() {
                originalPosition = Transform.localPosition;
                originalRotation = Transform.localRotation;

                Lock();
            }

            public void Lock() {
                Transform.localPosition = originalPosition;
                Transform.localRotation = originalRotation;
                Collider.enabled = false;
                Rigidbody.isKinematic = true;
            }

            public void Release(Vector3 velocity, Vector3 angularVelocity) {
                Collider.enabled = true;
                Rigidbody.isKinematic = false;

                Rigidbody.velocity = velocity;
                Rigidbody.angularVelocity = angularVelocity;
            }

            public void ProcessPhysics(float deltaTime, float gravity) {
                if (Rigidbody.isKinematic) { return; }

                Rigidbody.velocity += UnityEngine.Vector3.down * gravity * deltaTime;
            }

            public void SetScale(float scale) {
                Transform.localScale = Vector3.one * scale;
            }
        }

        private const int BEAK_MATERIAL_INDEX = 0;

        [Header("Animation")]
        [SerializeField] private string animatorJumpTriggerName;
        [SerializeField] private string animatorThrowTriggerName;
        [SerializeField] private string animatorMovingBoolName;

        [Header("Crouching")]
        [SerializeField] private float standingHeight;
        [SerializeField] private float crouchingHeight;
        [SerializeField] private Sterp.Mode crouchingInterpolationMode;

        [Header("Dying")]
        [SerializeField] private float angularVelocityOnDeath;
        [SerializeField] private float shrinkDuration;
        [SerializeField] private Sterp.Mode shrinkInterpolationMode;

        [Header("UI")]
        [SerializeField] private Cooldown overheadTextCooldown;
        [SerializeField] private float deathShowDuration;
        [SerializeField] private string[] deathStringOptions;

        [Header("References")]
        [SerializeField] private NetworkedPlayer player;
        [SerializeField] private BodyPart[] bodyParts;
        [SerializeField] private Animator animator;
        [SerializeField] private MeshRenderer headRenderer;
        [SerializeField] private TextMeshPro overheadText;

        private Random random = new Random();
        private float time => Time.time;
        private float deltaTime => Time.deltaTime;
        private float scale { get; set; }

        public void Initialize() {
            player.OnSpawned += HandlePlayerSpawned;
            player.OnDied += HandlePlayerDied;
            player.OnJump += HandlePlayerJump;
            player.OnThrow += HandlePlayerThrow;

            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.Initialize();
            }

            SetColors();
        }

        public void UnInitialize() {
            player.OnSpawned -= HandlePlayerSpawned;
            player.OnDied -= HandlePlayerDied;
            player.OnJump -= HandlePlayerJump;
            player.OnThrow -= HandlePlayerThrow;
        }

        #region MonoBehaviour

        private void FixedUpdate() {
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.ProcessPhysics(Time.fixedDeltaTime, player.DefaultGravity);
            }
        }

        private void Update() {
            HandleAnimation(deltaTime);
            HandleOverheadText(deltaTime);
        }

        #endregion

        #region Colors

        private void SetColors() {
            MaterialPropertyBlock headBlock = GameUtility.GetPropertyBlock(player.NetworkObject.OwnerID, headRenderer, BEAK_MATERIAL_INDEX);
            headRenderer.SetPropertyBlock(headBlock, BEAK_MATERIAL_INDEX);
        }

        #endregion

        #region Animation

        private void HandlePlayerJump() {
            animator.SetTrigger(animatorJumpTriggerName);
        }

        private void HandlePlayerThrow() {
            animator.SetTrigger(animatorThrowTriggerName);
        }

        private void HandleAnimation(float deltaTime) {
            transform.localScale = new Vector3(1f, Sterp.Ease(crouchingHeight, standingHeight, player.StandingT, crouchingInterpolationMode), 1f);

            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.SetScale(Sterp.Ease(scale, shrinkInterpolationMode));
            }

            animator.SetBool(animatorMovingBoolName, player.Moving);
        }

        #endregion

        #region Death & Spawning

        public IEnumerator ShrinkRoutine() {
            while(scale > 0f) {
                yield return new WaitForFixedUpdate();
                scale = Mathf.MoveTowards(scale, 0f, Time.fixedDeltaTime / shrinkDuration);
            }
            
            yield break;
        }

        private void HandlePlayerSpawned() {
            Lock();
            SetVisible(!player.IsLocalHumanPlayer);
        }

        private void HandlePlayerDied(NetworkedPlayer killer, Dodgeball dodgeball) {
            Vector3 velocity = player.VelocityOnDeath;
            velocity += dodgeball?.Velocity ?? Vector3.zero;
            
            Release(velocity, Random.Instance.InUnitSphere * angularVelocityOnDeath);
            SetVisible(true);

            SetOverheadText(deathStringOptions.OrderBy(o => random.UInt).FirstOrDefault(), deathShowDuration);
        }

        private void Lock() {
            scale = 1f;

            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.Lock();
            }

            animator.enabled = true;
        }

        private void Release(Vector3 velocity, Vector3 angularVelocity) {
            foreach (BodyPart bodyPart in bodyParts) {
                bodyPart.Release(velocity, angularVelocity);
            }

            animator.enabled = false;
        }

        private void SetVisible(bool visible) {
            gameObject.SetActive(visible);
        }

        #endregion

        #region Overhead Text

        private void HandleOverheadText(float deltaTime) {
            if (overheadTextCooldown.IsFinished(time)) {
                overheadText.text = string.Empty;
            }
        }

        private void SetOverheadText(string text, float duration) {
            if (player.IsLocalHumanPlayer) { return; }

            overheadText.text = text;

            overheadTextCooldown.Duration = duration;
            overheadTextCooldown.Start(time);
        }

        #endregion
    }
}