using HiHi;
using Stupid.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stupid.stujam01 {
    public class MapPart : MonoBehaviour {
        [Serializable]
        public class GenerationSettings {
            [Header("Constant")]
            public float Weight = 1f;
        }

        public Vector3 SpawnPosition => spawnTransform.position;
        public GenerationSettings Settings => settings;

        [SerializeField] private GenerationSettings settings;

        [Header("Spawning")]
        [SerializeField] private LayerMask environmentMask;
        [SerializeField] private float spawnCheckHeightOffset;
        [SerializeField] private float spawnCheckHeightRadius;
        [SerializeField] private bool canBeMirrored = true;
        [SerializeField] private bool canBeRotated = true;

        [Header("References")]
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private UnityNetworkObject[] networkObjects;

        private MapGenerator mapGenerator;

        public void Initialize(MapGenerator mapGenerator, Random random, bool registerNetworkObjects) {
            this.mapGenerator = mapGenerator;

            if (canBeRotated) {
                transform.Rotate(Vector3.up, 90f * random.Range(0, 3));
            }

            if (canBeMirrored) {
                transform.localScale = new Vector3(random.Bool ? 1f : -1f, 1f, random.Bool ? 1f : -1f);
            }

            if (registerNetworkObjects) {
                foreach (UnityNetworkObject networkObject in networkObjects) {
                    ushort id = (ushort)random.Range(0, ushort.MaxValue);
                    while (!INetworkObject.IsIDAvailable(id)) { id = (ushort)random.Range(0, ushort.MaxValue); }

                    ushort electedPlayerID = Peer.Network.PeerIDs.OrderBy(p => p).Skip(((int)Peer.Network.Hash + id) % Peer.Network.PeerCount).First();

                    networkObject.NetworkObject.Register(id, electedPlayerID);
                }
            }

            Coroutiner.Start(SetSpawnPosition());
        }
        
        public void Destroy() {
            foreach (UnityNetworkObject networkObject in networkObjects) {
                networkObject.NetworkObject.UnRegister();
            }

            Destroy(gameObject);
        }

        public bool SpawnIsVisible() {
            foreach (KeyValuePair<ushort, NetworkedPlayer> playerPair in NetworkedPlayer.Instances) {
                NetworkedPlayer player = playerPair.Value;

                if (!Physics.Linecast(player.Head.position, (UnityEngine.Vector3)SpawnPosition + (player.Head.position - player.transform.position), environmentMask, QueryTriggerInteraction.Ignore)) { return true; }
            }

            return false;
        }

        private IEnumerator SetSpawnPosition() {
            yield return new WaitForFixedUpdate();

            if (!Physics.SphereCast(spawnTransform.position + UnityEngine.Vector3.up * spawnCheckHeightOffset, spawnCheckHeightRadius, Vector3.down, out RaycastHit hit, Mathf.Infinity, environmentMask, QueryTriggerInteraction.Ignore)) { yield break; }

            spawnTransform.position = hit.point;

            yield break;
        }
    }
}