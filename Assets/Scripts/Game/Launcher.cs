using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HiHi;
using Stupid.Utilities;
using UnityEngine;

namespace Stupid.stujam01 {
    public class Launcher : UnityNetworkObject {
        [SerializeField] private float launchHeight;
        [SerializeField] private Cooldown launchCooldown;
        [SerializeField] private Cooldown launchDelay;
        [SerializeField] private AnimationCurve launchCurve;

        [Header("References")]
        [SerializeField] private new BoxCollider collider;
        [SerializeField] private Rigidbody platform;
        [SerializeField] private MeshRenderer[] renderers;

        private float time => HiHiTime.Time;
        private bool launching;

        private RPC<HiHiFloat> launchRPC;
        private RPC<ushort> launchPlayerRPC;

        #region HiHi

        protected override void OnRegister() {
            base.OnRegister();

            launchRPC = new RPC<HiHiFloat>(this);
            launchRPC.Action = new Action<HiHiFloat>(HandleLaunchRPC);

            launchPlayerRPC = new RPC<ushort>(this);
            launchPlayerRPC.Action = new Action<ushort>(HandleLaunchPlayerRPC);

            HandleColors();
        }

        protected override void UpdateInstance() {
            base.UpdateInstance();

            if (!NetworkObject.Authorized) { return; }
            if (!launchCooldown.IsFinished(time)) { return; }
            if (!launchDelay.IsFinished(time - HiHiTime.DeltaTime)) { return; }

            foreach (KeyValuePair<ushort, NetworkedPlayer> playerPair in NetworkedPlayer.Instances) {
                NetworkedPlayer player = playerPair.Value;

                if (!player.Alive) { continue; }
                if (!collider.bounds.Contains(player.Center)) { continue; }

                SyncLaunch(time);
                break;
            }
        }

        private void HandleLaunchRPC(HiHiFloat launchTime) {
            LocalLaunch(launchTime);
        }

        private void HandleLaunchPlayerRPC(ushort playerID) {
            NetworkedPlayer.Instances[playerID].SyncJump(launchHeight, 0f);
        }

        #endregion

        #region Launching

        private void SyncLaunch(float launchTime) {
            LocalLaunch(launchTime);
            launchRPC.Invoke(launchTime);
        }

        private void LocalLaunch(float launchTime) {
            if (launching) { return; }

            StartCoroutine(LaunchRoutine(launchTime));
        }

        private IEnumerator LaunchRoutine(float launchTime) {
            launching = true;
            launchDelay.Start(launchTime);

            while (!launchDelay.IsFinished(time)) {
                yield return new WaitForFixedUpdate();
            }

            launchCooldown.Start(launchTime);

            if (NetworkObject.Authorized) {
                IEnumerable<NetworkedPlayer> playersInLaunchArea = NetworkedPlayer.Instances
                    .Select(p => p.Value)
                    .Where(p => p.Alive)
                    .Where(p => collider.bounds.Contains(p.Center));

                foreach (NetworkedPlayer player in playersInLaunchArea) {
                    SyncLaunchPlayer(player);
                }
            }

            while(!launchCooldown.IsFinished(time)) {
                yield return new WaitForFixedUpdate();

                platform.MovePosition(transform.position + transform.up * launchCurve.Evaluate(launchCooldown.Progress(time)));
            }

            launching = false;
            yield break;
        }

        private void SyncLaunchPlayer(NetworkedPlayer player) {
            player.SyncJump(launchHeight, 0f);
            launchPlayerRPC.Invoke(player.NetworkObject.UniqueID);
        }

        #endregion

        #region Colors

        private void HandleColors() {
            foreach(MeshRenderer renderer in renderers) {
                renderer.SetPropertyBlock(null, 0);

                MaterialPropertyBlock headBlock = GameUtility.GetPropertyBlock(NetworkObject.OwnerID, renderer, 0);
                renderer.SetPropertyBlock(headBlock, 0);
            }
        }

        #endregion
    }
}