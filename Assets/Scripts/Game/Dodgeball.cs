using HiHi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stupid.stujam01 {
    public class Dodgeball : UnityNetworkObject {
        public static Dictionary<ushort, Dodgeball> Instances = new Dictionary<ushort, Dodgeball>();

        public bool Idle => idle;
        public Vector3 Velocity => rigidbody.velocity;

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

        [Header("Honing")]
        [SerializeField] private float honingAngleThreshold;
        [SerializeField] private float honingDuration;
        [SerializeField] private float lethalVelocityThreshold;

        [Header("Physics Checks")]
        [SerializeField] private LayerMask environmentMask;
        [SerializeField] private LayerMask playerMask;

        [Header("Physics")]
        [SerializeField] private float defaultDrag;
        [SerializeField] private float idleDrag;
        [SerializeField] private float gravity;

        [Header("References")]
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private new SphereCollider collider;
        [SerializeField] private new MeshRenderer renderer;

        private MatchManager matchManager => GameUtility.MatchManager;

        private float deltaTime => HiHiTime.DeltaTime;
        private float time => HiHiTime.Time;
        private float phase { get; set; } = Random.Instance.Float;
        private Vector3 horizontalVelocity => new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        private Vector3 floatOrigin;
        private Vector3 floatSmoothingVelocity;
        private float drag => idle ? idleDrag : defaultDrag;
        private NetworkedPlayer thrower;
        private bool honing => thrower != null;
        private bool atLethalVelocity => rigidbody.velocity.magnitude > lethalVelocityThreshold;

        private SyncPhysicsBody syncPhysicsBody;
        private RPC<HiHiVector3> idleRPC;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            syncPhysicsBody = new SyncPhysicsBody(this);
            syncPhysicsBody.OnDeserialize += HandleSyncPhysicsBodyDeserialize;

            idleRPC = new RPC<HiHiVector3>(this);
            idleRPC.Action = new Action<HiHiVector3>(HandleIdleRPC);

            NetworkObject.OnOwnershipChanged += HandleOwnershipChanged;

            HandleColors();

            Instances.Add(NetworkObject.UniqueID, this);
        }

        protected override void OnUnregister() {
            base.OnUnregister();

            NetworkObject.OnOwnershipChanged -= HandleOwnershipChanged;

            Instances.Remove(NetworkObject.UniqueID);
        }

        protected override void UpdateInstance() {
            base.UpdateInstance();

            HandlePickup(deltaTime);
            HandleFloating(deltaTime);
            HandleHoning(deltaTime);
            HandlePhysics(deltaTime);
            HandleNetworking(deltaTime);
        }

        private void HandleSyncPhysicsBodyDeserialize() {
            if (!NetworkObject.Owned) { return; }

            rigidbody.velocity = syncPhysicsBody.LinearVelocity;
            rigidbody.angularVelocity = syncPhysicsBody.AngularVelocity;
        }

        private void HandleIdleRPC(HiHiVector3 originPosition) {
            IdleLocal(originPosition);
        }

        private void HandleOwnershipChanged() => HandleColors();

        #endregion

        #region Colors

        private void HandleColors() {
            renderer.SetPropertyBlock(null, 0);

            MaterialPropertyBlock block = GameUtility.GetPropertyBlock(NetworkObject.OwnerID, renderer, 0);
            renderer.SetPropertyBlock(block, 0);
        }

        #endregion

        #region Player Interaction

        public void Throw(Vector3 position, Vector3 direction, float speed) {
            Release();

            transform.position = position;
            rigidbody.velocity = direction * speed;
        }

        public void Hold(NetworkedPlayer player) {
            thrower = player;

            rigidbody.isKinematic = true;
            collider.enabled = false;
            held = true;
            idle = false;
        }

        private void Release() {
            transform.localScale = Vector3.one;
            rigidbody.isKinematic = false;
            held = false;
        }

        private void HandlePickup(float deltaTime) {
            if (!idle) { return; }

            Collider[] collidersInRadius = Physics.OverlapSphere(rigidbody.position, pickupRadius, playerMask, QueryTriggerInteraction.Ignore);

            foreach (Collider collider in collidersInRadius) {
                NetworkedPlayer player = collider.GetComponentInParent<NetworkedPlayer>();

                if (player == null) { continue; }
                if (!player.TryPickUp(this)) { continue; }
            }
        }

        private void HandleHoning(float deltaTime) {
            if (!honing || !atLethalVelocity) { return; }

            NetworkedPlayer target = NetworkedPlayer.Instances
                .Select(p => p.Value)
                .Where(p => p != thrower)
                .Where(p => p.Alive)
                .Where(p => Vector3.Angle(horizontalVelocity.normalized, (p.Head.position - transform.position).normalized) < honingAngleThreshold)
                .Where(p => !Physics.Linecast(transform.position, p.Head.position, environmentMask))
                .OrderBy(p => Vector3.Angle(horizontalVelocity.normalized, (p.Head.position - transform.position).normalized))
                .FirstOrDefault();
            
            if(target == null) { return; }

            Vector3 targetDirection = (target.Head.position - transform.position).normalized;

            rigidbody.velocity = Vector3.MoveTowards(horizontalVelocity, targetDirection * horizontalVelocity.magnitude, deltaTime / honingDuration)
                + Vector3.up * rigidbody.velocity.y;

            if(!Physics.SphereCast(rigidbody.position, collider.radius, rigidbody.velocity.normalized, out RaycastHit hit, rigidbody.velocity.magnitude * deltaTime, playerMask)) { return; }

            NetworkedPlayer player = hit.collider.GetComponentInParent<NetworkedPlayer>();

            if (player == null) { return; }
            if (player == thrower) { return; }
            if (!thrower.NetworkObject.Authorized) { return; }

            thrower.SyncKill(player, this);
        }

        #endregion

        #region Floating

        public void SyncIdle(Vector3 originPosition) {
            IdleLocal(originPosition);
            idleRPC.Invoke(originPosition);
        }

        private void IdleLocal(Vector3 originPosition) {
            thrower = null;

            if (NetworkObject.Authorized) {
                NetworkObject.Forfeit();
            }

            floatOrigin = originPosition;
            rigidbody.position = transform.position = originPosition;
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

            if (!Physics.Raycast(rigidbody.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, environmentMask, QueryTriggerInteraction.Ignore)) {
                rigidbody.velocity += UnityEngine.Vector3.down * gravity * deltaTime;
                return;
            }

            floatOrigin = hit.point;
            Vector3 floatPosition = floatOrigin + Vector3.up * Mathf.Lerp(minFloatHeight, maxFloatHeight, Mathf.PingPong((time + phase) * floatFrequency, 1f));
            Vector3 targetPosition = Vector3.SmoothDamp(rigidbody.position, floatPosition, ref floatSmoothingVelocity, floatSmoothingDuration, Mathf.Infinity, deltaTime);
            rigidbody.position = targetPosition;
        }

        #endregion

        #region Physics

        private void HandleNetworking(float deltaTime) {
            if (!NetworkObject.Owned || held || idle) { return; }

            if (NetworkObject.Authorized) {
                syncPhysicsBody.Sleeping = rigidbody.IsSleeping();
                syncPhysicsBody.Set(rigidbody.position, rigidbody.rotation, Vector3.zero, rigidbody.velocity, rigidbody.angularVelocity);
            }
            else {
                if (syncPhysicsBody.TryGetPosition(rigidbody.position, out HiHiVector3 newPosition)) {
                    rigidbody.position = newPosition;
                }

                if (syncPhysicsBody.TryGetRotation(rigidbody.rotation, out HiHiQuaternion newRotation)) {
                    rigidbody.rotation = newRotation;
                }
            }
        }

        private void HandlePhysics(float deltaTime) {
            rigidbody.drag = drag;

            if(!collider.enabled && !held) {
                bool inBounds = thrower?.AuraCollider.bounds.Contains(collider.transform.position) ?? default;
                bool inRadius = thrower?.AuraCollider.Raycast(new Ray(transform.position, (thrower.AuraCollider.transform.position - transform.position).normalized), out _, collider.radius) ?? default;

                if (!honing || (!inBounds && !inRadius)) {
                    collider.enabled = true;
                }
            }

            if (held || idle) { return; }

            rigidbody.velocity += UnityEngine.Vector3.down * gravity * deltaTime;
        }

        private void OnCollisionEnter(Collision collision) {
            if (!honing || !atLethalVelocity) { return; }

            NetworkedPlayer player = collision.collider.GetComponentInParent<NetworkedPlayer>();

            if(player == null) { return; }
            if(player == thrower) { return; }
            if (!thrower.NetworkObject.Authorized) { return; }

            thrower.SyncKill(player, this);
        }

        #endregion
    }
}