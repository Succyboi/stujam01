using HiHi;
using Stupid.Utilities;
using System.Collections;
using UnityEngine;

namespace Stupid.stujam01 {
    public class GameUtility : SingletonMonoBehaviour<GameUtility> {
        public static HiHiManager HiHiManager => Instance.hihiManager;
        public static MatchManager MatchManager => Instance.matchManager;
        public static MapGenerator MapGenerator => Instance.mapGenerator;
        public static Camera Camera => Instance.camera;

        [SerializeField] private float quitDelay = 1f;

        [Header("Color")]
        [SerializeField] private Color fallbackColor;
        [SerializeField] private Color fallbackShadowColor;

        [Header("References")]
        [SerializeField] private HiHiManager hihiManager;
        [SerializeField] private MatchManager matchManager;
        [SerializeField] private MapGenerator mapGenerator;
        [SerializeField] private new Camera camera;

        #region Initialization

        protected override void Awake() {
            base.Awake();

            Coroutiner.Start(InitializationRoutine());
        }

        private IEnumerator InitializationRoutine() {
            mapGenerator.Generate(Random.GetSeed(), false);
            ShowMainMenu();

            yield break;
        }

        public void ShowMainMenu() => MainMenu.Instance.Show();
        public void HideMainMenu() => MainMenu.Instance.Hide();
        public void ResetMainMenu() => MainMenu.Instance.ResetMenu();

        public void Quit() => StartCoroutine(QuitRoutine());

        private IEnumerator QuitRoutine() {
            yield return new WaitForSeconds(quitDelay);
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif

            yield break;
        }

#endregion

        #region Color

        public static string GetColorCode(ushort? peerID) {
            if (peerID == null || !Peer.Network.Contains(peerID ?? default)) {
                return ColorUtility.ToHtmlStringRGB(Instance.fallbackColor);
            }

            PeerInfo peerInfo = Peer.Network[peerID ?? default];
            return peerInfo.ColorCode;
        }

        public static Color GetColor(ushort? peerID) {
            if (peerID == null || !Peer.Network.Contains(peerID ?? default)) {
                return Instance.fallbackColor;
            }

            PeerInfo peerInfo = Peer.Network[peerID ?? default];
            return Color.HSVToRGB((float)peerInfo.Hue, 1f, 1f);
        }

        public static MaterialPropertyBlock GetPropertyBlock(ushort? peerID, MeshRenderer meshRenderer, int materialIndex) {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);

            if (peerID == null || !Peer.Network.Contains(peerID ?? default)) {
                block.SetColor("_Color", Instance.fallbackColor);
                block.SetColor("_ShadowColor", Instance.fallbackShadowColor);

                return block;
            }

            PeerInfo peerInfo = Peer.Network[peerID ?? default];

            Color mainColor = meshRenderer.materials[materialIndex].GetColor("_Color");
            Color.RGBToHSV(mainColor, out _, out _, out float mainV);
            mainColor = Color.HSVToRGB((float)peerInfo.Hue, 1f, mainV);
            block.SetColor("_Color", mainColor);

            Color shadowColor = meshRenderer.materials[materialIndex].GetColor("_ShadowColor");
            Color.RGBToHSV(shadowColor, out _, out _, out float shadowV);
            shadowColor = Color.HSVToRGB((float)peerInfo.Hue, 1f, shadowV);
            block.SetColor("_ShadowColor", shadowColor);

            return block;
        }

        #endregion
    }
}