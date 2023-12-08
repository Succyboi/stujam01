using UnityEngine;
using HiHi;
using HiHi.Discovery;

namespace Stupid.stujam01 {
    public class HiHiManager : MonoBehaviour {
        [SerializeField] private bool LogProcessedMessages;

        [Header("References")]
        [SerializeField] private UnityHelper helper;

        private LiteNetTransport transport = new LiteNetTransport();
        private BroadcastFinder broadcastFinder = new BroadcastFinder();

        private void Start() {
            Peer.Initialize(transport, helper);

            transport.Start();
            broadcastFinder.Start();

            Peer.OnLog += HandleLog;
            Peer.OnConnect += HandleConnect;
            Peer.OnDisconnect += HandleDisconnect;
            Peer.OnMessageProcessed += HandleMessageProcessed;
        }

        private void OnDestroy() {
            Peer.UnInitialize();
            transport.Stop();
            broadcastFinder.Stop();

            Peer.OnLog -= HandleLog;
            Peer.OnConnect -= HandleConnect;
            Peer.OnDisconnect -= HandleDisconnect;
            Peer.OnMessageProcessed -= HandleMessageProcessed;
        }

        private void OnApplicationQuit() {
            Peer.UnInitialize();
            transport.Stop();
            broadcastFinder.Stop();

            Peer.OnLog -= HandleLog;
            Peer.OnConnect -= HandleConnect;
            Peer.OnDisconnect -= HandleDisconnect;
            Peer.OnMessageProcessed -= HandleMessageProcessed;
        }

        private void FixedUpdate() {
            Peer.Update(Time.fixedDeltaTime);
        }

        private void HandleConnect(ushort id, PeerConnectReason reason) => Debug.Log($"Connected to {id}. ({reason})");
        private void HandleDisconnect(ushort id, PeerDisconnectReason reason) => Debug.Log($"Disconnected from {id}. ({reason})");
        private void HandleLog(ushort id, string message) => Debug.Log($"[{id}]: {message}");
        private void HandleMessageProcessed(PeerMessageType messageType, ushort id) {
            if (LogProcessedMessages) {
                Debug.Log($"Received message of type {messageType} from {id}.");
            }
        }
    }
}