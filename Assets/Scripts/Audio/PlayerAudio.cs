using Stupid.Audio;
using UnityEngine;

namespace Stupid.stujam01 {
    public class PlayerAudio : MonoBehaviour {
        [Header("Footsteps")]
        [SerializeField] private Sound localFootstepsSound;
        [SerializeField] private Sound remoteFootstepsSound;
        [SerializeField] private float footstepFrequency;
        private Sound footstepsSound => IsLocalHumanPlayer ? localFootstepsSound : remoteFootstepsSound;

        [Header("Jumps")]
        [SerializeField] private Sound localRegularJump;
        [SerializeField] private Sound remoteRegularJump;
        [SerializeField] private Sound localLongJump;
        [SerializeField] private Sound remoteLongJump;
        [SerializeField] private Sound localHighJump;
        [SerializeField] private Sound remoteHighJump;
        private Sound regularJump => IsLocalHumanPlayer ? localRegularJump : remoteRegularJump;
        private Sound longJump => IsLocalHumanPlayer ? localLongJump : remoteLongJump;
        private Sound highJump => IsLocalHumanPlayer ? localHighJump : remoteHighJump;

        [Header("Spawn")]
        [SerializeField] private Sound localSpawn;

        [Header("Die")]
        [SerializeField] private Sound localDeath;
        [SerializeField] private Sound remoteDeath;
        private Sound death => IsLocalHumanPlayer ? localDeath : remoteDeath;

        [Header("Pick Up")]
        [SerializeField] private Sound pickUpLocal;
        [SerializeField] private Sound pickUpRemote;
        private Sound pickUp => IsLocalHumanPlayer ? pickUpLocal: pickUpRemote;

        [Header("Throw")]
        [SerializeField] private Sound thrownLocal;
        [SerializeField] private Sound thrownRemote;
        private Sound thrown => IsLocalHumanPlayer ? thrownLocal : thrownRemote;

        [Header("Crouch")]
        [SerializeField] private Sound crouchLocal;
        [SerializeField] private Sound crouchRemote;
        [SerializeField] private Sound uncrouchLocal;
        [SerializeField] private Sound uncrouchRemote;
        private Sound crouch => IsLocalHumanPlayer ? crouchLocal : crouchRemote;
        private Sound uncrouch => IsLocalHumanPlayer ? uncrouchLocal : uncrouchRemote;

        [Header("Game")]
        [SerializeField] private Sound win;
        [SerializeField] private Sound lose;

        [Header("References")]
        [SerializeField] private NetworkedPlayer player;
        [SerializeField] private AudioSource scoreSource;

        private Vector3 lastFootstepPosition;
        private bool initialized;
        private MatchManager matchManager => GameUtility.MatchManager;
        private bool IsLocalHumanPlayer => player.IsLocalHumanPlayer;
        private float deltaTime => Time.deltaTime;
        private float time => Time.time;

        public void Initialize() {
            player.OnSpawned += HandlePlayerSpawned;
            player.OnDied += HandlePlayerDied;

            player.OnRegularJump += HandlePlayerRegularJump;
            player.OnLongJump += HandlePlayerLongJump;
            player.OnHighJump += HandlePlayerHighJump;

            player.OnPickup += HandlePlayerPickup;

            player.OnThrow += HandlePlayerThrow;

            player.OnCrouchChanged += HandleCrouchChanged;

            player.OnScoreChanged += HandleScoreChanged;

            matchManager.OnMatchEnd += HandleMatchEnd;

            initialized = true;
        }

        public void Uninitialize() {
            player.OnSpawned -= HandlePlayerSpawned;
            player.OnDied -= HandlePlayerDied;

            player.OnRegularJump -= HandlePlayerRegularJump;
            player.OnLongJump -= HandlePlayerLongJump;
            player.OnHighJump -= HandlePlayerHighJump;

            player.OnPickup -= HandlePlayerPickup;

            player.OnThrow -= HandlePlayerThrow;

            player.OnCrouchChanged -= HandleCrouchChanged;

            player.OnScoreChanged -= HandleScoreChanged;

            matchManager.OnMatchEnd -= HandleMatchEnd;

            initialized = false;
        }

        private void HandlePlayerSpawned() {
            lastFootstepPosition = player.Position;

            if (IsLocalHumanPlayer) {
                localSpawn.Play(null);
            }
        }

        private void HandlePlayerDied(NetworkedPlayer killer, Dodgeball dodgeball) {
            death.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandlePlayerRegularJump() {
            regularJump.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandlePlayerLongJump() {
            longJump.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandlePlayerHighJump() {
            highJump.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandlePlayerPickup() {
            pickUp.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandlePlayerThrow() {
            thrown.Play(IsLocalHumanPlayer ? null : player.Position);
        }

        private void HandleCrouchChanged(bool crouched) {
            if (crouched) {
                crouch.Play(IsLocalHumanPlayer ? null : player.Position);
            }
            else {
                uncrouch.Play(IsLocalHumanPlayer ? null : player.Position);
            }
        }

        private void HandleScoreChanged(ushort score) {
            if (!IsLocalHumanPlayer) { return; }

            scoreSource.pitch = Pitch.SemitoneToPlaybackSpeed(score);
            scoreSource.Play();
        }

        private void HandleMatchEnd(NetworkedPlayer player) {
            if (!IsLocalHumanPlayer) { return; }

            bool won = player == this.player;
            (won ? win : lose).Play();
        }

        private void Update() {
            if (!initialized) { return; }

            HandleFootsteps(deltaTime);
        }

        private void HandleFootsteps(float deltaTime) {
            if (!player.Grounded) { return; }
            if (Vector3.Distance(lastFootstepPosition, player.Position) < 1f / footstepFrequency) { return; }

            footstepsSound.Play(IsLocalHumanPlayer ? null : player.Position);
            lastFootstepPosition = player.Position;
        }
    }
}