using Cinemachine;
using Stupid.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Stupid.stujam01 {
    public class MainMenu : SingletonMonoBehaviour<MainMenu> {
        public override bool ForceSingleInstance => false;

        public bool Showing { get; private set; }

        [Header("References")]
        [SerializeField] private new CinemachineVirtualCamera camera;
        [SerializeField] private GameObject uiParent;
        [SerializeField] private Canvas[] canvasses;
        [SerializeField] private GameObject rightMenu;
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button playLANButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button howToButton;
        [SerializeField] private MatchScreen matchScreen;

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

            Showing = false;
        }

        public void ResetMenu() {
            matchScreen.Hide();

            rightMenu.SetActive(true);
        }

        private void HandlePlayOnlineButtonClicked() {
            rightMenu.SetActive(false);
            matchScreen.Show();
        }

        private void HandlePlayLanButtonClicked() {
            rightMenu.SetActive(false);
            matchScreen.Show();
        }

        private void HandleSettingsButtonClicked() {

        }

        private void HandleHowToButtonClicked() {

        }
    }
}