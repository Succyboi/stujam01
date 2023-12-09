using HiHi;
using Stupid.Utilities;
using System.Collections;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MatchManager : UnityNetworkObject {
        private const float COUNTDOWN_DURATION = 30f;

        [SerializeField] private int playersToStart;

        [Header("References")]
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private UnitySpawnData humanPlayerSpawnData;

        private int requiredPlayers;

        public void StartMatchCountdown(int requiredPlayers) {
            this.requiredPlayers = requiredPlayers;

            Coroutiner.Start(StartMatchRoutine());
        }

        private void Start() {
            NetworkObject.Register(0);
            StartMatchCountdown(playersToStart);
        }

        private IEnumerator StartMatchRoutine() {
            while (HiHiTime.Time < COUNTDOWN_DURATION && Peer.Network.Connections + 1 < requiredPlayers) {
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(1f);

            Debug.Log($"Generating map with seed {Peer.Network.Hash}");

            mapGenerator.Generate(Peer.Network.Hash);

            INetworkObject.SyncSpawn(humanPlayerSpawnData, Peer.Info.UniqueID);

            yield break;
        }
    }
}