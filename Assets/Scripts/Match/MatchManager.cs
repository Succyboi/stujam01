using HiHi;
using Stupid.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MatchManager : UnityNetworkObject {
        [Serializable]
        public class MatchSettings {
            public int PlayersToStart;
            public float RespawnDuration;
            public int MinDodgeballCount;
            public float DodgeballsPerPlayer;
            public int ScoreWinThreshold;
            public float MatchStartStepInterval;
            public int PreMatchCountDown;
            public float TimeBeforeDisconnectAfterGameEnd;
        }

        public event Action<NetworkedPlayer> OnMatchEnd;

        public MatchSettings Settings => settings;
        public bool CanAbort => Searching;
        public bool VotedToStartEarly => votedToStartEarly;
        public bool InMatch => matchOngoing || matchEnding;
        public bool MatchOngoing => matchOngoing;
        public bool MatchEnding => matchEnding;
        public bool CanLeave => canLeave;
        public bool Searching { get; set; }
        public int Players => Peer.Network.PeerCount;
        public string Status { get; private set; }

        [SerializeField] private MatchSettings settings;
        
        [Header("Out of bounds")]
        [SerializeField] private float boundsHeight;

        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private UnitySpawnData humanPlayerSpawnData;
        [SerializeField] private UnitySpawnData botPlayerSpawnData;
        [SerializeField] private UnitySpawnData dodgeballSpawnData;

        private GameUtility gameUtility => GameUtility.Instance;
        private HiHiManager hihiManager => GameUtility.HiHiManager;
        private bool matchOngoing;
        private bool matchEnding;
        private bool votedToStartEarly;
        private bool canLeave;

        private Sync<ushort> StartEarlyVotesSync;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            StartEarlyVotesSync = new Sync<ushort>(this);
        }

        protected override void UpdateInstance() {
            base.UpdateInstance();

            if (!matchOngoing) { return; }

            foreach(KeyValuePair<ushort, NetworkedPlayer> playerPair in NetworkedPlayer.Instances) {
                NetworkedPlayer player = playerPair.Value;

                if (!player.NetworkObject.Authorized) { continue; }
                if (!player.Alive) { continue; }
                if (player.transform.position.y > boundsHeight) { continue; }

                player.SyncKill(player, null);
            }

            foreach (KeyValuePair<ushort, Dodgeball> dodgeballPair in Dodgeball.Instances) {
                Dodgeball dodgeball = dodgeballPair.Value;

                if (!dodgeball.NetworkObject.Authorized) { continue; }
                if(dodgeball.transform.position.y > boundsHeight) { continue; }

                TryGetSafeSpawnPosition(out Vector3 spawnPosition);
                dodgeball.SyncIdle(spawnPosition);
            }

            foreach (KeyValuePair<ushort, NetworkedPlayer> playerPair in NetworkedPlayer.Instances) {
                NetworkedPlayer player = playerPair.Value;

                if (!player.IsHumanPlayer) { continue; }
                if(player.Score < settings.ScoreWinThreshold) { continue; }

                Coroutiner.Start(EndMatchRoutine(player));
                return;
            }
        }

        #endregion

        #region MonoBehaviour

        private void Start() {
            NetworkObject.Register(0);
        }

        #endregion

        #region Match

        public void StartSearchingForMatch() {
            Coroutiner.Start(SearchForMatchRoutine());
        }

        public void AbortSearchingForMatch() {
            Searching = false;
        }

        public void VoteToStartEarly() {
            if (votedToStartEarly) { return; }

            votedToStartEarly = true;
            StartEarlyVotesSync.Value++;
        }

        public void LeaveMatch() {
            if(!InMatch) { return; }

            Peer.DisconnectAll();

            while (NetworkedPlayer.Instances.Count > 0) {
                NetworkedPlayer player = NetworkedPlayer.Instances[NetworkedPlayer.Instances.Keys.FirstOrDefault()];

                player.NetworkObject.SyncDestroy();
            }

            while (Dodgeball.Instances.Count > 0) {
                Dodgeball dodgeball = Dodgeball.Instances[Dodgeball.Instances.Keys.FirstOrDefault()];

                dodgeball.NetworkObject.SyncDestroy();
            }

            mapGenerator.Clear();
            mapGenerator.Generate(Random.GetSeed(), false);
            gameUtility.ShowMainMenu();
            gameUtility.ResetMainMenu();

            canLeave = false;
            matchEnding = false;
            matchOngoing = false;
        }

        public bool TryGetSafeSpawnPosition(out Vector3 spawnPosition) {
            foreach(MapPart mapPart in mapGenerator.GetMapParts()) {
                if (!mapPart.SpawnIsVisible()) {
                    spawnPosition = mapPart.SpawnPosition;
                    return true;
                }
            }

            spawnPosition = GetRandomSpawnPosition();
            return false;
        }

        public Vector3 GetRandomSpawnPosition() {
            Random random = new Random();
            MapPart part = mapGenerator.GetMapParts()
                .OrderBy(p => random.UInt)
                .FirstOrDefault();

            return part.SpawnPosition;
        }

        private IEnumerator SearchForMatchRoutine() {
            StartEarlyVotesSync.Value = 0;
            votedToStartEarly = false;

            Peer.ConnectionKey = $"{Application.companyName}/{Application.productName}/{Application.version}/{settings.PlayersToStart}";
            Peer.AcceptingConnections = Searching = true;
            hihiManager.StartDiscoveringOnLan();

            while (Players < settings.PlayersToStart && StartEarlyVotesSync.Value < Players) {
                Status = $"Searching{string.Empty.PadRight(Mathf.RoundToInt(HiHiTime.Time) % 3 + 1, '.')}\n{Players} of {settings.PlayersToStart} players found";

                yield return new WaitForEndOfFrame();

                if (!Searching) {
                    Peer.AcceptingConnections = Searching = false;
                    yield break;
                }
            }

            Status = $"Starting match";

            Peer.AcceptingConnections = Searching = false;
            hihiManager.StopDiscoveringOnLan();
            matchOngoing = true;

            yield return new WaitForSeconds(settings.MatchStartStepInterval);

            Status = $"Generating map with seed {Peer.Network.Hash}";

            yield return new WaitForSeconds(settings.MatchStartStepInterval);

            mapGenerator.Clear();
            mapGenerator.Generate(Peer.Network.Hash);

            for (int c = 0; c < settings.PreMatchCountDown; c++) {
                Status = $"{settings.PreMatchCountDown - c}";

                yield return new WaitForSeconds(settings.MatchStartStepInterval);
            }

            Status = "In match";

            NetworkedPlayer networkedPlayer = INetworkObject.SyncSpawn(humanPlayerSpawnData, Peer.Info.UniqueID) as NetworkedPlayer;
            networkedPlayer.SyncKill(networkedPlayer, null);
            networkedPlayer.SyncSpawn(GetRandomSpawnPosition());

            ushort electedPlayerID = Peer.Network.PeerIDs.OrderBy(p => p).Skip((int)Peer.Network.Hash % Peer.Network.PeerCount).First();
            if (electedPlayerID == Peer.Info.UniqueID) {
                int dodgeballCount = settings.MinDodgeballCount + Mathf.RoundToInt(settings.DodgeballsPerPlayer * settings.PlayersToStart);

                for (int b = 0; b < dodgeballCount; b++) {
                    Dodgeball dodgeball = INetworkObject.SyncSpawn(dodgeballSpawnData) as Dodgeball;
                    dodgeball.SyncIdle(GetRandomSpawnPosition());
                }
            }

            for(int p = 0; p < settings.PlayersToStart - Peer.Network.PeerCount; p++) {
                electedPlayerID = Peer.Network.PeerIDs.OrderBy(p => p).Skip(((int)Peer.Network.Hash + p) % Peer.Network.PeerCount).First();

                if (electedPlayerID != Peer.Info.UniqueID) { continue; }

                NetworkedPlayer botPlayer = INetworkObject.SyncSpawn(botPlayerSpawnData, Peer.Info.UniqueID) as NetworkedPlayer;
                botPlayer.SyncKill(botPlayer, null);
                botPlayer.SyncSpawn(GetRandomSpawnPosition());
            }

            yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();

            canLeave = true;

            gameUtility.HideMainMenu();

            yield break;
        }

        private IEnumerator EndMatchRoutine(NetworkedPlayer winner) {
            Status = "Match ending";

            canLeave = false;
            matchEnding = true;
            matchOngoing = false;
            OnMatchEnd?.Invoke(winner);

            yield return new WaitForSeconds(settings.TimeBeforeDisconnectAfterGameEnd);

            Status = "Match ended";

            LeaveMatch();

            matchEnding = false;

            yield break;
        }

        #endregion
    }
}