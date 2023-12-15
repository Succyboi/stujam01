using HiHi;
using Stupid.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stupid.stujam01 {
    public class NetworkedPlayer : UnityNetworkObject {
        public static Dictionary<ushort, NetworkedPlayer> Instances = new Dictionary<ushort, NetworkedPlayer>();

        public event Action OnSpawned;
        public event Action<NetworkedPlayer, Dodgeball> OnDied;
        public event Action OnJump;
        public event Action OnRegularJump;
        public event Action OnLongJump;
        public event Action OnHighJump;
        public event Action OnPickup;
        public event Action OnThrow;
        public event Action<bool> OnCrouchChanged;
        public event Action<ushort> OnScoreChanged;

        public bool IsHumanPlayer => isHumanPlayer;
        public bool IsLocalHumanPlayer => isHumanPlayer && NetworkObject.Authorized;
        public bool Alive => alive;
        public ushort Score => scoreSync.Value;
        public bool HoldingBall => dodgeball != null;
        public bool Moving => wishDirection.magnitude > 0 && alive;
        public bool Grounded => grounded;
        public bool Crouching => crouching;
        public float StandingT => standingT;
        public Vector3 Velocity => rigidbody.velocity;
        public Vector3 VelocityOnDeath { get; private set; }
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public float Speed => rigidbody.velocity.magnitude;
        public float DefaultGravity => defaultGravity;
        public Vector3 Center => transform.position + UnityEngine.Vector3.up * height / 2f;
        public Vector3 Position => transform.position;
        public Transform Head => head;
        public Collider AuraCollider => auraCollider;

        [SerializeField] private bool isHumanPlayer;

        [Header("State")]
        [SerializeField] private bool alive;
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

        [Header("Dodgeball Interaction")]
        [SerializeField] private Cooldown throwCoyoteTime = new Cooldown(0f);
        [SerializeField] private Cooldown throwCooldown = new Cooldown(0f);
        [SerializeField] private float dodgeballHoldInterpolationDuration;
        [SerializeField] private float throwingSpeed;

        [Header("Physics Checks")]
        [SerializeField] private LayerMask groundedCheckMask;
        [SerializeField] private float groundCheckSkinThickness;
        [SerializeField] private float maxGroundCheckAngle;
        [SerializeField] private float auraSkinThickness;

        [Header("References")]
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private new CapsuleCollider collider;
        [SerializeField] private CapsuleCollider auraCollider;
        [SerializeField] private Transform head;
        [SerializeField] private Transform localHand;
        [SerializeField] private Transform remoteHand;
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private PlayerBody body;
        [SerializeField] private new PlayerAudio audio;

        private float deltaTime => HiHiTime.DeltaTime;
        private float time => HiHiTime.Time;
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
        private Transform hand => IsLocalHumanPlayer ? localHand : remoteHand;
        private Dodgeball dodgeball;

        private MatchManager matchManager => GameUtility.MatchManager;

        private RPC<HiHiVector3> spawnRPC;
        private RPC<ushort, ushort> killRPC;
        private SyncPhysicsBody syncPhysicsBody;
        private Sync<HiHiVector2> movementSync;
        private Sync<bool> crouchSync;
        private RPC<HiHiFloat, HiHiFloat> jumpRPC;
        private RPC<ushort> pickUpRPC;
        private RPC<HiHiVector3, HiHiVector3, HiHiFloat> throwRPC;
        private Sync<ushort> scoreSync;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            cameraController.Initialize();
            body.Initialize();
            audio.Initialize();

            spawnRPC = new RPC<HiHiVector3>(this);
            spawnRPC.Action = new Action<HiHiVector3>(HandleSpawnRPC);

            killRPC = new RPC<ushort, ushort>(this);
            killRPC.Action = new Action<ushort, ushort>(HandleKillRPC);

            syncPhysicsBody = new SyncPhysicsBody(this);
            syncPhysicsBody.OnDeserialize += HandleSyncPhysicsBodyDeserialize;

            movementSync = new Sync<HiHiVector2>(this);
            crouchSync = new Sync<bool>(this);
            crouchSync.OnValueChanged += OnCrouchChanged;

            jumpRPC = new RPC<HiHiFloat, HiHiFloat>(this);
            jumpRPC.Action = new Action<HiHiFloat, HiHiFloat>(HandleJumpRPC);

            pickUpRPC = new RPC<ushort>(this);
            pickUpRPC.Action = new Action<ushort>(HandlePickUpRPC);

            throwRPC = new RPC<HiHiVector3, HiHiVector3, HiHiFloat>(this);
            throwRPC.Action = new Action<HiHiVector3, HiHiVector3, HiHiFloat>(HandleThrowRPC);

            scoreSync = new Sync<ushort>(this);
            scoreSync.OnValueChanged += OnScoreChanged;

            Instances.Add(NetworkObject.UniqueID, this);
        }

        protected override void OnUnregister() {
            base.OnUnregister();

            cameraController.UnInitialize();
            body.UnInitialize();
            audio.Uninitialize();

            syncPhysicsBody.OnDeserialize -= HandleSyncPhysicsBodyDeserialize;
            crouchSync.OnValueChanged -= OnCrouchChanged;
            scoreSync.OnValueChanged -= OnScoreChanged;

            Instances.Remove(NetworkObject.UniqueID);
        }

        protected override void UpdateInstance() {
            HandleCrouching(deltaTime);
            
            if (!alive) { return; }

            HandleBody(deltaTime);
            HandleGroundedCheck(deltaTime);
            HandlePhysics(deltaTime);
            HandleJumping(deltaTime);
            HandleMovement(deltaTime);
            HandleNetworking(deltaTime);
            HandleDodgeballInteraction(deltaTime);
        }

        private void HandleSyncPhysicsBodyDeserialize() {
            if (!alive) { return; }

            rigidbody.velocity = syncPhysicsBody.LinearVelocity;
            rigidbody.angularVelocity = syncPhysicsBody.AngularVelocity;
        }

        private void HandleSpawnRPC(HiHiVector3 spawnPosition) => SpawnLocal(spawnPosition);

        private void HandleKillRPC(ushort player, ushort dodgeball) {
            Instances[player].KillLocal(this, Dodgeball.Instances.ContainsKey(dodgeball) ? Dodgeball.Instances[dodgeball] : null);
        }

        private void HandleJumpRPC(HiHiFloat height, HiHiFloat distance) {
            Jump(height, distance);
        }

        private void HandlePickUpRPC(ushort dodgeball) {
            LocalPickUp(Dodgeball.Instances[dodgeball]);
        }

        private void HandleThrowRPC(HiHiVector3 position, HiHiVector3 direction, HiHiFloat speed) {
            LocalThrow(position, direction, speed);
        }

        #endregion

        #region Death & Spawning

        public void SyncSpawn(Vector3 spawnPosition) {
            if (alive) { return; }
            if (!matchManager.MatchOngoing) { return; }

            SpawnLocal(spawnPosition);
            spawnRPC.Invoke(spawnPosition);
        }

        public void SyncKill(NetworkedPlayer player, Dodgeball dodgeball) {
            if (!player.alive) { return; }

            player.SyncDropBall();
            player.KillLocal(this, dodgeball);
            killRPC.Invoke(player.NetworkObject.UniqueID, dodgeball?.NetworkObject.UniqueID ?? default);
        
            if(player != this) {
                scoreSync.Value++;
            }
        }

        public void Respawn() => StartCoroutine(RespawnRoutine());

        private void SpawnLocal(Vector3 spawnPosition) {
            if (alive) { return; }

            rigidbody.isKinematic = false;
            collider.enabled = true;
            rigidbody.position = transform.position = spawnPosition;

            alive = true;
            OnSpawned?.Invoke();
        }

        private void KillLocal(NetworkedPlayer killer, Dodgeball dodgeball) {
            if (!alive) { return; }

            VelocityOnDeath = rigidbody.velocity;
            rigidbody.isKinematic = true;
            collider.enabled = false;

            alive = false;
            OnDied?.Invoke(killer, dodgeball);

            if (NetworkObject.Authorized) {
                Respawn();
            }
        }

        private IEnumerator RespawnRoutine() {
            yield return new WaitForSeconds(matchManager.Settings.RespawnDuration);

            yield return body.ShrinkRoutine();

            matchManager.TryGetSafeSpawnPosition(out Vector3 spawnPosition);
            SyncSpawn(spawnPosition);

            yield break;
        }

        #endregion

        #region Input

        public void SetMovement(Vector2 movement) {
            if (!NetworkObject.Authorized) { return; }

            movementSync.Value = movement;
        }

        public void SetCrouch(bool crouch) {
            if (!NetworkObject.Authorized) { return; }

            bool changed = crouch != crouchSync.Value;
            crouchSync.Value = crouch;

            if (changed) {
                OnCrouchChanged?.Invoke(crouch);
            }
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

        public void Throw() {
            if (!NetworkObject.Authorized) { return; }

            throwCoyoteTime.Start(time);
        }

        #endregion

        #region Movement

        public void SyncJump(float height, float distance) {
            Jump(height, distance);

            if (NetworkObject.Authorized) {
                jumpRPC.Invoke(height, distance);
            }
        }

        private void HandleCrouching(float deltaTime) {
            float rawStandingT = (crouching && alive) ? 0f : 1f;
            standingT = Mathf.MoveTowards(standingT, rawStandingT, deltaTime / crouchDuration);
        }

        private void HandleBody(float deltaTime) {
            collider.height = height;
            collider.center = Vector3.up * height / 2f;
            collider.radius = radius;

            auraCollider.height = height + auraSkinThickness;
            collider.center = Vector3.up * height / 2f;
            auraCollider.radius = radius + auraSkinThickness;

            head.localPosition = Vector3.up * (height - crouchingHeight / 2f);
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

            bool highJump = crouching && !Moving;
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

            OnJump?.Invoke();

            if(height > jumpHeight) {
                OnHighJump?.Invoke();
            }
            else {
                if(distance > 0f) {
                    OnLongJump?.Invoke();
                }
                else {
                    OnRegularJump?.Invoke();
                }
            }
        }

        #endregion

        #region Dodgeball Interaction

        public bool TryPickUp(Dodgeball dodgeball) {
            if (!alive) { return false; }
            if (!NetworkObject.Authorized) { return false; }
            if (this.dodgeball != null) { return false; }

            SyncPickUp(dodgeball);

            return true;
        }

        private void SyncPickUp(Dodgeball dodgeball) {
            dodgeball.NetworkObject.Claim();

            LocalPickUp(dodgeball);
            pickUpRPC.Invoke(dodgeball.NetworkObject.UniqueID);
        }

        private void LocalPickUp(Dodgeball dodgeball) {
            this.dodgeball = dodgeball;
            dodgeball.transform.parent = hand;
            dodgeball.Hold(this);

            OnPickup?.Invoke();
        }

        private void SyncThrow() {
            if(dodgeball == null) { return; }

            LocalThrow(head.position, head.forward, throwingSpeed);
            throwRPC.Invoke(head.position, head.forward, throwingSpeed);
        }

        private void SyncDropBall() {
            if (dodgeball == null) { return; }

            LocalThrow(head.position, head.forward, 0f);
            throwRPC.Invoke(head.position, head.forward, 0f);
        }

        private void LocalThrow(Vector3 position, Vector3 direction, float speed) {
            if (dodgeball == null) { return; }

            dodgeball.transform.parent = transform.parent;
            dodgeball.Throw(position, direction, speed);
            dodgeball = null;

            OnThrow?.Invoke();
        }

        private void HandleDodgeballInteraction(float deltaTime) {
            if (dodgeball == null) { return; }

            dodgeball.transform.localPosition = Vector3.MoveTowards(dodgeball.transform.localPosition, Vector3.zero, deltaTime / dodgeballHoldInterpolationDuration);

            if (!throwCooldown.IsFinished(time)) { return; }
            if (throwCoyoteTime.IsFinished(time)) { return; }

            throwCooldown.Start(time);
            SyncThrow();
        }

        #endregion
    }
}