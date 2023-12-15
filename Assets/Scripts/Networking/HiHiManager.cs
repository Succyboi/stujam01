using UnityEngine;
using HiHi;
using HiHi.Discovery;

namespace Stupid.stujam01 {
    public class HiHiManager : MonoBehaviour {
        [SerializeField] private bool logProcessedMessages;

        [Header("Signaler")]
        [SerializeField] private string signalerAddress;
        [SerializeField] private int signalerPort;

        [Header("References")]
        [SerializeField] private UnityHelper helper;

        private LiteNetTransport transport = new LiteNetTransport();
        private BroadcastFinder broadcastFinder = new BroadcastFinder();
        private UDPSignalerFinder signalerFinder;

        public bool HasRemoteEndPoint => !string.IsNullOrEmpty(Peer.Info.RemoteEndPoint);

        public void StartDiscoveringOnLan() {
            broadcastFinder.Start();
        }

        public void StopDiscoveringOnLan() {
            broadcastFinder.Stop();
        }

        public void StartDiscoveringOnline(int lobbySize) {
            Peer.Info.Verified = false;
            signalerFinder.LobbySize = lobbySize;
            signalerFinder.Start();
        }

        public void StopDiscoveringOnline() {
            signalerFinder.Stop();
        }

        public void SendVerificationRequest() => signalerFinder.SendVerificationRequest();

        private void Start() {
            signalerFinder = new UDPSignalerFinder(signalerAddress, signalerPort);

            HiHiConfiguration.DEFAULT_ABANDONMENT_POLICY = NetworkObjectAbandonmentPolicy.Destroy;
            Peer.Initialize(transport, helper);
            Peer.AcceptingConnections = false;

            transport.Start();

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
            if (logProcessedMessages) {
                Debug.Log($"Received message of type {messageType} from {id}.");
            }
        }
    }
}