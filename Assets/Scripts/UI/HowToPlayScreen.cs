using UnityEngine;
using UnityEngine.UI;

namespace Stupid.stujam01 {
    public class HowToPlayScreen : MonoBehaviour {
        public bool Showing { get; private set; }

        [Header("References")]
        [SerializeField] private Button backButton;

        private MainMenu mainMenu => MainMenu.Instance;

        public void Show() {
            if (Showing) { return; }

            backButton.onClick.AddListener(HandleBackButtonPressed);

            gameObject.SetActive(true);

            Showing = true;
        }

        public void Hide() {
            if (!Showing) { return; }

            backButton.onClick.RemoveAllListeners();

            gameObject.SetActive(false);

            Showing = false;
        }

        private void HandleBackButtonPressed() {
            mainMenu.ResetMenu();

            mainMenu.PlayClickSound();
        }
    }
}