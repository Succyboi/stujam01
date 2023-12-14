using Stupid.Utilities;
using TMPro;
using UnityEngine;

namespace Stupid.stujam01 {
    public class PlayerScreen : SingletonMonoBehaviour<PlayerScreen> {
        [Header("Big Text")]
        [SerializeField] private string deathString;
        [SerializeField] private string spawnedString;
        [SerializeField] private float deathShowDuration;
        [SerializeField] private float spawnedShowDuration;
        [SerializeField] private string wonString;
        [SerializeField] private string lostString;
        [SerializeField] private Cooldown bigTextCooldown;

        [Header("Objective")]
        [SerializeField] private string objectiveTextTemplate;

        [Header("Sway")]
        [SerializeField] private float swayAmount;

        [Header("References")]
        [SerializeField] private RectTransform screenRectTransform;
        [SerializeField] private TextMeshProUGUI bigText;
        [SerializeField] private GameObject speed;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private GameObject score;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI objectiveText;

        private NetworkedPlayer player;
        private PlayerCameraController cameraController;
        private MatchManager matchManager => GameUtility.MatchManager;
        private MainMenu mainMenu => MainMenu.Instance;

        private bool initialized;
        private float time => Time.time;
        private float deltaTime => Time.deltaTime;

        #region MonoBehaviour

        public void Initialize(NetworkedPlayer player, PlayerCameraController cameraController) {
            this.player = player;
            this.cameraController = cameraController;

            player.OnDied += HandlePlayerDied;
            player.OnSpawned += HandlePlayerSpawned;
            matchManager.OnMatchEnd += HandleMatchEnd;

            initialized = true;
        }

        public void UnInitialize() {
            player.OnDied -= HandlePlayerDied;
            player.OnSpawned -= HandlePlayerSpawned;
            matchManager.OnMatchEnd -= HandleMatchEnd;

            initialized = false;
        }

        private void HandlePlayerDied(NetworkedPlayer killer, Dodgeball dodgeball) {
            if (!matchManager.MatchOngoing) { return; }

            SetBigText(deathString, deathShowDuration);
        }

        private void HandlePlayerSpawned() {
            if (!matchManager.MatchOngoing) { return; }
            
            SetBigText(spawnedString, spawnedShowDuration);
        }

        private void HandleMatchEnd(NetworkedPlayer player) {
            bool won = player == this.player;

            SetBigText(string.Format(won ? wonString : lostString, GameUtility.GetColorCode(player.NetworkObject.OwnerID)), matchManager.Settings.TimeBeforeDisconnectAfterGameEnd);
        }

        private void Update() {
            screenRectTransform.gameObject.SetActive(!mainMenu.Showing);

            if (!initialized) { return; }

            HandlePlayerInfo(deltaTime);
            HandleBigText(deltaTime);
            HandleSway(deltaTime);
        }

        #endregion

        #region Player Info

        private void HandlePlayerInfo(float deltaTime) {
            speed.SetActive(player.Alive);
            speedText.text = $"{Mathf.RoundToInt(player.HorizontalSpeed)}";

            score.SetActive(player.Alive);
            scoreText.color = GameUtility.GetColor(player.NetworkObject.OwnerID);
            scoreText.text = $"{player.Score}";

            objectiveText.text = string.Format(objectiveTextTemplate, matchManager.Settings.ScoreWinThreshold);
        }

        #endregion

        #region Big Text

        private void HandleBigText(float deltaTime) {
            if (bigTextCooldown.IsFinished(time)) {
                bigText.text = string.Empty;
            }
        }

        private void SetBigText(string text, float showDuration) {
            bigText.text = text;
            bigTextCooldown.Duration = showDuration;
            bigTextCooldown.Start(time);
        }

        #endregion

        #region Sway

        private void HandleSway(float deltaTime) {
            screenRectTransform.anchoredPosition3D = cameraController.Sway * swayAmount;
        }

        #endregion
    }
}