using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stupid.stujam01 {
    public class MatchScreen : MonoBehaviour {
        [SerializeField] private string canVoteToStartEarlyString;
        [SerializeField] private string cannotVoteToStartEarlyString;
        [SerializeField] private string abortString;
        [SerializeField] private string leaveString;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI backText;
        [SerializeField] private Button startEarlyButton;
        [SerializeField] private TextMeshProUGUI startEarlyText;

        private MatchManager matchManager => GameUtility.MatchManager;
        private MainMenu mainMenu => MainMenu.Instance;

        public void Show() {
            matchManager.StartSearchingForMatch();

            backButton.onClick.AddListener(HandleBackButtonPressed);
            startEarlyButton.onClick.AddListener(HandleStartEarlyButtonPressed);

            gameObject.SetActive(true);
        }

        public void Hide() {
            matchManager.AbortSearchingForMatch();

            backButton.onClick.RemoveListener(HandleBackButtonPressed);
            startEarlyButton.onClick.RemoveListener(HandleStartEarlyButtonPressed);

            gameObject.SetActive(false);
        }

        private void HandleBackButtonPressed() {
            if (matchManager.CanLeave) {
                matchManager.LeaveMatch();
            }
            else {
                Hide();
                mainMenu.ResetMenu();
            }
        }

        private void HandleStartEarlyButtonPressed() {
            matchManager.VoteToStartEarly();
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
        }
    }
}