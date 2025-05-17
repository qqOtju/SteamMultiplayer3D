using Project.Scripts.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI.MainMenu
{
    public class UIMenu : MonoBehaviour
    {
        [Title("Buttons")] [SerializeField] private Button _hostButton;

        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _customizationButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        [Title("Panels")] [SerializeField] private Canvas _customizationPanel;

        [SerializeField] private Canvas _settingsPanel;

        [Title("Network")] [SerializeField] private CustomNetworkManager _networkManager;

        [SerializeField] private SteamLobby _steamLobby;

        private Canvas _mainMenuPanel;

        private void Awake()
        {
            _hostButton.onClick.AddListener(OnHostButtonClicked);
            _joinButton.onClick.AddListener(OnJoinButtonClicked);
            _customizationButton.onClick.AddListener(OnCustomizationButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            _exitButton.onClick.AddListener(OnExitButtonClicked);
            _steamLobby.OnLobbyCreatedEvent += OnLobbyCreated;
        }

        private void Start()
        {
            _mainMenuPanel = GetComponent<Canvas>();
        }

        private void OnDestroy()
        {
            _hostButton.onClick.RemoveListener(OnHostButtonClicked);
            _joinButton.onClick.RemoveListener(OnJoinButtonClicked);
            _customizationButton.onClick.RemoveListener(OnCustomizationButtonClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            _exitButton.onClick.RemoveListener(OnExitButtonClicked);
            _steamLobby.OnLobbyCreatedEvent -= OnLobbyCreated;
        }

        private void OnHostButtonClicked()
        {
            _steamLobby.HostLobby();
            // _networkManager.StartHost();
        }

        private void OnJoinButtonClicked()
        {
            _steamLobby.OpenSteamFriends();
            /*var ipAddress = "localhost";
            _networkManager.networkAddress = ipAddress;
            _networkManager.StartClient();
            _joinButton.interactable = false;
            _hostButton.interactable = false;*/
        }

        private void OnCustomizationButtonClicked() //
        {
            _customizationPanel.enabled = !_customizationPanel.enabled;
        }

        private void OnSettingsButtonClicked()
        {
        }

        private void OnExitButtonClicked()
        {
        }

        private void OnLobbyCreated(bool obj)
        {
            _hostButton.interactable = !obj;
        }
    }
}