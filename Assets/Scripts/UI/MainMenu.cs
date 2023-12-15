using Cinemachine;
using Stupid.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Stupid.stujam01 {
    public class MainMenu : SingletonMonoBehaviour<MainMenu> {
        public override bool ForceSingleInstance => false;

        public bool Showing { get; private set; }

        [Header("Audio")]
        [SerializeField] private Sound clickSound;
        [SerializeField] private Sound confirmSound;
        [SerializeField] private Sound tickSound;
        [SerializeField] private float backgroundMusicVolume;
        [SerializeField] private float backgroundMusicVolumeAdjustDuration;

        [Header("References")]
        [SerializeField] private new CinemachineVirtualCamera camera;
        [SerializeField] private GameObject uiParent;
        [SerializeField] private Canvas[] canvasses;
        [SerializeField] private GameObject rightMenu;
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button playLANButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button howToButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private MatchScreen matchScreen;
        [SerializeField] private AudioSource backgroundMusicSource;

        private GameUtility gameUtility => GameUtility.Instance;
        private MatchManager matchManager => GameUtility.MatchManager;
        private InputManager input => InputManager.Instance;

        public void Show() {
            camera.Priority = CameraConstants.MAIN_MENU_PRIORITY;
            uiParent.SetActive(true);

            foreach(Canvas canvas in canvasses) {
                canvas.worldCamera = GameUtility.Camera;
            }

            input.UnlockCursor();

            playOnlineButton.onClick.AddListener(HandlePlayOnlineButtonClicked);
            playLANButton.onClick.AddListener(HandlePlayLanButtonClicked);
            settingsButton.onClick.AddListener(HandleSettingsButtonClicked);
            howToButton.onClick.AddListener(HandleHowToButtonClicked);
            quitButton.onClick.AddListener(HandleQuitButtonClicked);

            PlayTickSound();

            Showing = true;
        }

        public void Hide() {
            camera.Priority = CameraConstants.MIN_PRIORITY;
            uiParent.SetActive(false);

            input.LockCursor();

            playOnlineButton.onClick.RemoveListener(HandlePlayOnlineButtonClicked);
            playLANButton.onClick.RemoveListener(HandlePlayLanButtonClicked);
            settingsButton.onClick.RemoveListener(HandleSettingsButtonClicked);
            howToButton.onClick.RemoveListener(HandleHowToButtonClicked);
            quitButton.onClick.RemoveListener(HandleQuitButtonClicked);

            PlayTickSound();

            Showing = false;
        }

        public void ResetMenu() {
            matchScreen.Hide();

            rightMenu.SetActive(true);
        }

        private void HandlePlayOnlineButtonClicked() {
            rightMenu.SetActive(false);
            matchScreen.Show();

            PlayClickSound();
        }

        private void HandlePlayLanButtonClicked() {
            rightMenu.SetActive(false);
            matchScreen.Show();

            PlayClickSound();
        }

        private void HandleSettingsButtonClicked() {
            PlayClickSound();
        }

        private void HandleHowToButtonClicked() {
            PlayClickSound();
        }

        private void HandleQuitButtonClicked() {
            gameUtility.Quit();

            PlayClickSound();
        }

        private void Update() {
            HandleSound();
        }

        #region Sound

        public void PlayClickSound() => clickSound.Play();
        public void PlayMatchmakingSound() => confirmSound.Play();
        public void PlayTickSound() => tickSound.Play();

        private void HandleSound() {
            backgroundMusicSource.volume = Mathf.MoveTowards(backgroundMusicSource.volume, (Showing && !matchManager.InMatch) ? backgroundMusicVolume : 0f, Time.deltaTime / backgroundMusicVolumeAdjustDuration);
        }

        #endregion
    }
}