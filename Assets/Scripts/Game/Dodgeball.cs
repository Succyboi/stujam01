using HiHi;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stupid.stujam01 {
    public class Dodgeball : UnityNetworkObject {
        public static Dictionary<ushort, Dodgeball> Instances = new Dictionary<ushort, Dodgeball>();
        
        [Header("State")]
        [SerializeField] private bool idle;
        [SerializeField] private bool held;

        [Header("Floating")]
        [SerializeField] private float idleSpeedThreshold;
        [SerializeField] private float minFloatHeight;
        [SerializeField] private float maxFloatHeight;
        [SerializeField] private float floatFrequency;
        [SerializeField] private float floatSmoothingDuration;

        [Header("Player Interaction")]
        [SerializeField] private float pickupRadius;

        [Header("Physics Checks")]
        [SerializeField] private LayerMask environmentMask;
        [SerializeField] private LayerMask playerMask;

        [Header("Physics")]
        [SerializeField] private float defaultDrag;
        [SerializeField] private float idleDrag;
        [SerializeField] private float gravity;

        [Header("References")]
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private new Collider collider;

        private MatchManager matchManager => MatchManager.Instance;

        private float deltaTime => HiHiTime.DeltaTime;
        private float time => HiHiTime.Time;
        private float phase { get; set; } = Random.Instance.Float;
        private Vector3 floatOrigin;
        private Vector3 floatSmoothingVelocity;
        private float drag => idle ? idleDrag : defaultDrag;

        private RPC<HiHiVector3> idleRPC;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            idleRPC = new RPC<HiHiVector3>(this);
            idleRPC.Action = new Action<HiHiVector3>(HandleIdleRPC);

            Instances.Add(NetworkObject.UniqueID, this);
        }

        protected override void OnUnregister() {
            base.OnUnregister();

            Instances.Remove(NetworkObject.UniqueID);
        }

        protected override void UpdateInstance() {
            base.UpdateInstance();

            HandlePickup(deltaTime);
            HandleFloating(deltaTime);
            HandlePhysics(deltaTime);
        }

        private void HandleIdleRPC(HiHiVector3 originPosition) {
            IdleLocal(originPosition);
        }

        #endregion

        #region Player Interaction

        public void Throw(Vector3 direction, float speed) {
            Release();

            rigidbody.velocity = direction * speed;
        }

        private void Hold() {
            rigidbody.isKinematic = true;
            collider.enabled = false;
            held = true;
            idle = false;
        }

        private void Release() {
            rigidbody.isKinematic = false;
            collider.enabled = true;
            held = false;
        }

        private void HandlePickup(float deltaTime) {
            if (!idle) { return; }

            Collider[] collidersInRadius = Physics.OverlapSphere(rigidbody.position, pickupRadius, playerMask, QueryTriggerInteraction.Ignore);

            foreach (Collider collider in collidersInRadius) {
                NetworkedPlayer player = collider.GetComponentInParent<NetworkedPlayer>();

                if(player == null || !player.NetworkObject.Authorized) { continue; }
                if (!player.TryPickUp(this)) { continue; }

                Hold();
            }
        }

        #endregion

        #region Floating

        public void SyncIdle(Vector3 originPosition) {
            if (idle) { return; }

            IdleLocal(originPosition);
            idleRPC.Invoke(originPosition);
        }

        private void IdleLocal(Vector3 originPosition) {
            if (idle) { return; }

            floatOrigin = originPosition;
            transform.position = originPosition;
            rigidbody.velocity = Vector3.zero;

            idle = true;
        }

        private void HandleFloating(float deltaTime) {
            if (held) { return; }
            if (!idle) {
                if (NetworkObject.Authorized && rigidbody.velocity.magnitude < idleSpeedThreshold) {
                    SyncIdle(rigidbody.position);
                }
                return; 
            }

            if (Physics.Raycast(rigidbody.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, environmentMask, QueryTriggerInteraction.Ignore)) {
                floatOrigin = hit.point;
            }

            Vector3 floatPosition = floatOrigin + Vector3.up * Mathf.Lerp(minFloatHeight, maxFloatHeight, Mathf.PingPong((time + phase) * floatFrequency, 1f));
            Vector3 targetPosition = Vector3.SmoothDamp(rigidbody.position, floatPosition, ref floatSmoothingVelocity, floatSmoothingDuration, Mathf.Infinity, deltaTime);
            rigidbody.position = targetPosition;
        }

        #endregion

        #region Physics

        private void HandlePhysics(float deltaTime) {
            rigidbody.drag = drag;

            if (held || idle) { return; }

            rigidbody.velocity += UnityEngine.Vector3.down * gravity * deltaTime;
        }

        #endregion
    }
}