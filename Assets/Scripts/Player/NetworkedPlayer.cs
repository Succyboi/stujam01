using Cinemachine;
using HiHi;
using System;
using UnityEngine;

namespace Stupid.stujam01 {
    public class NetworkedPlayer : UnityNetworkObject {
        public Vector3 Velocity => rigidbody.velocity;
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public float Speed => rigidbody.velocity.magnitude;

        [Header("State")]
        [SerializeField] private bool grounded;
        [SerializeField] private Cooldown groundedCoyoteTime = new Cooldown(0f);

        [Header("Body")]
        [SerializeField] private float standingHeight;
        [SerializeField] private float crouchingHeight;
        [SerializeField] private float radius;

        [Header("Physics")]
        [SerializeField] private float defaultGravity;
        [SerializeField] private float crouchGravity;

        [Header("Movement")]
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxCrouchAccel;
        [SerializeField] private float maxGroundAccel;
        [SerializeField] private float maxAirAccel;
        [SerializeField] private float maxDeccel;

        [Header("Crouching")]
        [SerializeField] private float crouchDuration;

        [Header("Jumping")]
        [SerializeField] private Cooldown jumpCoyoteTime = new Cooldown(0f);
        [SerializeField] private Cooldown jumpCooldown = new Cooldown(0f);
        [SerializeField] private float jumpHeight;
        [SerializeField] private float longjumpDistance;
        [SerializeField] private float highJumpHeight;

        [Header("Physics Checks")]
        [SerializeField] private LayerMask groundedCheckMask;
        [SerializeField] private float groundCheckSkinThickness;
        [SerializeField] private float maxGroundCheckAngle;
        [SerializeField] private LayerMask vaultCheckMask;

        [Header("References")]
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private new CapsuleCollider collider;
        [SerializeField] private Transform head;

        private float deltaTime => Time.fixedDeltaTime;
        private float time => Time.time;
        private Vector3 horizontalVelocity => new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        private Vector3 wishDirection => rigidbody.rotation * new Vector3(movementSync.Value.X, 0f, movementSync.Value.Y);
        private float gravity => (grounded && crouching)
            ? crouchGravity
            : defaultGravity;
        private float maxAccel => crouching
            ? maxCrouchAccel 
            : (grounded ? maxGroundAccel : maxAirAccel);
        private bool crouching => crouchSync.Value;
        private float standingT { get; set; } = 1f;
        private float height => Mathf.Lerp(crouchingHeight, standingHeight, standingT);

        private SyncPhysicsBody syncPhysicsBody;
        private Sync<HiHiVector2> movementSync;
        private Sync<bool> crouchSync;
        private RPC<HiHiFloat, HiHiFloat> jumpRPC;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            if (NetworkObject.Authorized) {
                NetworkObject.AbandonmentPolicy = NetworkObjectAbandonmentPolicy.Destroy;
            }

            syncPhysicsBody = new SyncPhysicsBody(this);
            syncPhysicsBody.OnDeserialize += HandleSyncPhysicsBodyDeserialize;

            movementSync = new Sync<HiHiVector2>(this);
            crouchSync = new Sync<bool>(this);

            jumpRPC = new RPC<HiHiFloat, HiHiFloat>(this);
            jumpRPC.Action = new Action<HiHiFloat, HiHiFloat>(HandleJumpRPC);
        }

        protected override void OnUnregister() {
            base.OnUnregister();

            syncPhysicsBody.OnDeserialize -= HandleSyncPhysicsBodyDeserialize;
        }

        private void HandleSyncPhysicsBodyDeserialize() {
            rigidbody.velocity = syncPhysicsBody.LinearVelocity;
            rigidbody.angularVelocity = syncPhysicsBody.AngularVelocity;
        }

        private void HandleJumpRPC(HiHiFloat height, HiHiFloat distance) {
            Jump(height, distance);
        }

        #endregion

        #region Input

        public void SetMovement(Vector2 movement) {
            if (!NetworkObject.Authorized) { return; }

            movementSync.Value = movement;
        }

        public void SetCrouch(bool crouch) {
            if (!NetworkObject.Authorized) { return; }

            crouchSync.Value = crouch;
        }

        public void SetRotation(Vector2 rotation) {
            if (!NetworkObject.Authorized) { return; }

            rigidbody.rotation = Quaternion.Euler(Vector3.up * rotation.x);
            head.localRotation = Quaternion.Euler(Vector3.right * rotation.y);

        }

        public void Jump() {
            if (!NetworkObject.Authorized) { return; }

            jumpCoyoteTime.Start(time);
        }

        #endregion

        #region Movement

        private void FixedUpdate() {
            HandleCrouching(deltaTime);
            HandleBody(deltaTime);
            HandleGroundedCheck(deltaTime);
            HandlePhysics(deltaTime);
            HandleJumping(deltaTime);
            HandleMovement(deltaTime);
            HandleNetworking(deltaTime);
        }

        private void HandleCrouching(float deltaTime) {
            float rawStandingT = crouching ? 0f : 1f;
            standingT = Mathf.MoveTowards(standingT, rawStandingT, deltaTime / crouchDuration);
        }

        private void HandleBody(float deltaTime) {
            collider.height = height;
            collider.center = Vector3.up * height / 2f;
            head.localPosition = Vector3.up * (height - crouchingHeight / 2f);

            collider.radius = radius;
        }

        private void HandleGroundedCheck(float deltaTime) {
            bool previousGrounded = grounded;
            grounded = false;

            RaycastHit[] hits = Physics.SphereCastAll(collider.transform.position + UnityEngine.Vector3.up * (collider.radius + collider.height),
                collider.radius + groundCheckSkinThickness,
                Vector3.down,
                collider.height,
                groundedCheckMask,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits) {
                Vector3 localHitPosition = collider.transform.InverseTransformPoint(hit.point);

                if (localHitPosition.y > collider.center.y)
                    continue;

                localHitPosition.y = 0.0f;
                float threshold = collider.radius * Mathf.Sin(maxGroundCheckAngle * Mathf.Deg2Rad);
                threshold *= threshold;

                if (localHitPosition.sqrMagnitude <= threshold) {
                    grounded = true;
                    groundedCoyoteTime.Start(time);
                    break;
                }
            }

            if(grounded != previousGrounded && !crouching) {
                rigidbody.velocity = new UnityEngine.Vector3(rigidbody.velocity.x, Mathf.Max(rigidbody.velocity.y), rigidbody.velocity.z);
            }
        }

        private void HandlePhysics(float deltaTime) {
            if (rigidbody.IsSleeping()) {
                rigidbody.WakeUp();
            }

            if (!grounded || crouching) {
                rigidbody.velocity += UnityEngine.Vector3.down * gravity * deltaTime;
            }
        }

        private void HandleJumping(float deltaTime) {
            if (groundedCoyoteTime.IsFinished(time)) { return; }
            if (jumpCoyoteTime.IsFinished(time)) { return; }
            if (!jumpCooldown.IsFinished(time)) { return; }

            bool highJump = crouching && wishDirection.magnitude <= 0;
            bool longJump = crouching && !highJump;

            float height = highJump ? highJumpHeight : jumpHeight;
            float distance = longJump ? longjumpDistance : 0f;

            Jump(height, distance);
            groundedCoyoteTime.Finish(time);
            jumpCoyoteTime.Finish(time);
            jumpCooldown.Start(time);

            jumpRPC.Invoke(height, distance);
        }

        private void HandleMovement(float deltaTime) {
            if (grounded) {
                float subtractSpeed = maxDeccel * deltaTime;
                float frictionMult = Mathf.Max((horizontalVelocity.magnitude - subtractSpeed) / horizontalVelocity.magnitude, 0f);

                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z) * frictionMult
                    + Vector3.up * rigidbody.velocity.y;
            }

            float currentSpeed = Vector3.Dot(horizontalVelocity, wishDirection);
            float addSpeed = Mathf.Clamp(maxSpeed - currentSpeed, 0f, maxAccel * deltaTime);

            rigidbody.velocity += (UnityEngine.Vector3)wishDirection * addSpeed;
        }

        private void HandleNetworking(float deltaTime) {
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

        private void Jump(float height, float distance) {
            if (!TrajectoryMoths.TryGetSpeedFromHeightPotential(gravity, 0f, height, out float heightSpeed)) { return; }

            if (!TrajectoryMoths.TryGetLaunchSpeed(gravity, distance, 45f, out float distanceSpeed)) { return; }

            float currentSpeed = Vector3.Dot(horizontalVelocity, wishDirection);
            distanceSpeed = Mathf.Max(distanceSpeed / 2f - currentSpeed, 0f);

            rigidbody.velocity = horizontalVelocity;
            rigidbody.velocity += UnityEngine.Vector3.up * (heightSpeed - distanceSpeed);
            rigidbody.velocity += (UnityEngine.Vector3)(wishDirection + Vector3.up) * distanceSpeed;
            grounded = false;
        }

        #endregion
    }
}