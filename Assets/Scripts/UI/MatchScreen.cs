using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stupid.stujam01 {
    public class MatchScreen : MonoBehaviour {
        public bool Showing { get; private set; }

        [SerializeField] private string canVoteToStartEarlyString;
        [SerializeField] private string cannotVoteToStartEarlyString;
        [SerializeField] private string abortString;
        [SerializeField] private string leaveString;
        [SerializeField] private string usingBotsString;
        [SerializeField] private string notUsingBotsString;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI backText;
        [SerializeField] private Button startEarlyButton;
        [SerializeField] private TextMeshProUGUI startEarlyText;
        [SerializeField] private Button toggleBotsButton;
        [SerializeField] private TextMeshProUGUI toggleBotsText;

        private MatchManager matchManager => GameUtility.MatchManager;
        private MainMenu mainMenu => MainMenu.Instance;

        public void Show(bool online) {
            if (Showing) { return; }

            matchManager.StartSearchingForMatch(online);

            backButton.onClick.AddListener(HandleBackButtonPressed);
            startEarlyButton.onClick.AddListener(HandleStartEarlyButtonPressed);
            toggleBotsButton.onClick.AddListener(HandleToggleBotsButtonPressed);

            gameObject.SetActive(true);

            Showing = true;
        }

        public void Hide() {
            if (!Showing) { return; }

            matchManager.AbortSearchingForMatch();

            backButton.onClick.RemoveAllListeners();
            startEarlyButton.onClick.RemoveAllListeners();
            toggleBotsButton.onClick.RemoveAllListeners();

            gameObject.SetActive(false);

            Showing = false;
        }

        private void HandleBackButtonPressed() {
            if (matchManager.CanLeave) {
                matchManager.LeaveMatch();
            }
            else {
                mainMenu.ResetMenu();
            }

            mainMenu.PlayClickSound();
        }

        private void HandleStartEarlyButtonPressed() {
            matchManager.VoteToStartEarly();

            mainMenu.PlayClickSound();
        }

        private void HandleToggleBotsButtonPressed() {
            matchManager.SetUseBots(!matchManager.UsingBots);

            mainMenu.PlayClickSound();
        }

        private void Update() {
            statusText.text = matchManager.Status;

            backButton.interactable = matchManager.CanAbort || matchManager.CanLeave;
            backText.text = matchManager.CanLeave
                ? leaveString
                : abortString;

            startEarlyButton.interactable = !matchManager.VotedToStartEarly;
            startEarlyText.enabled = matchManager.CanAbort;
            startEarlyText.text = matchManager.VotedToStartEarly
                ? cannotVoteToStartEarlyString 
                : canVoteToStartEarlyString;

            toggleBotsButton.enabled = matchManager.CanAbort;
            toggleBotsText.enabled = matchManager.CanAbort;
            toggleBotsText.text = matchManager.UsingBots
                ? usingBotsString
                : notUsingBotsString;
        }
    }
}