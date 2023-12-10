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
        }

        private const float COUNTDOWN_DURATION = 30f;

        public static MatchManager Instance { get; private set; }

        public MatchSettings Settings => settings;

        [SerializeField] private MatchSettings settings;
        
        [Header("Out of bounds")]
        [SerializeField] private float boundsHeight;

        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private UnitySpawnData humanPlayerSpawnData;
        [SerializeField] private UnitySpawnData dodgeballSpawnData;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();
        }

        protected override void UpdateInstance() {
            base.UpdateInstance();

            foreach(KeyValuePair<ushort, NetworkedPlayer> playerPair in NetworkedPlayer.Instances) {
                NetworkedPlayer player = playerPair.Value;

                if (!player.NetworkObject.Authorized) { continue; }
                if (!player.Alive) { continue; }
                if (player.transform.position.y > boundsHeight) { continue; }

                player.SyncKill(player);
            }

            foreach (KeyValuePair<ushort, Dodgeball> dodgeballPair in Dodgeball.Instances) {
                Dodgeball dodgeball = dodgeballPair.Value;

                if (!dodgeball.NetworkObject.Authorized) { continue; }
                if(dodgeball.transform.position.y > boundsHeight) { continue; }

                dodgeball.SyncIdle(GetRandomSpawnPosition());
            }
        }

        #endregion

        #region MonoBehaviour

        private void Start() {
            Instance = this;

            NetworkObject.Register(0);
            StartMatchCountdown();
        }

        #endregion

        #region Match

        public void StartMatchCountdown() {
            Coroutiner.Start(StartMatchRoutine());
        }

        public Vector3 GetRandomSpawnPosition() => mapGenerator.GetRandomSpawnPosition();

        private IEnumerator StartMatchRoutine() {
            while (HiHiTime.Time < COUNTDOWN_DURATION && Peer.Network.Connections + 1 < settings.PlayersToStart) {
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(1f);

            Debug.Log($"Generating map with seed {Peer.Network.Hash}");

            mapGenerator.Generate(Peer.Network.Hash);

            NetworkedPlayer networkedPlayer = INetworkObject.SyncSpawn(humanPlayerSpawnData, Peer.Info.UniqueID) as NetworkedPlayer;
            networkedPlayer.SyncSpawn(GetRandomSpawnPosition());

            ushort electedPlayerID = Peer.Network.PeerIDs.Skip((int)Peer.Network.Hash % Peer.Network.PeerCount).First();
            if (electedPlayerID != Peer.Info.UniqueID) { yield break; }

            int dodgeballCount = settings.MinDodgeballCount + Mathf.RoundToInt(settings.DodgeballsPerPlayer * Peer.Network.PeerCount);

            for (int b = 0; b < dodgeballCount; b++) {
                Dodgeball dodgeball = INetworkObject.SyncSpawn(dodgeballSpawnData) as Dodgeball;
                dodgeball.SyncIdle(GetRandomSpawnPosition());
            }

            yield break;
        }

        #endregion
    }
}