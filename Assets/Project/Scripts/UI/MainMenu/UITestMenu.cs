using Project.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.TEST
{
    public class UITestMenu : MonoBehaviour
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _customizationButton;
        [SerializeField] private Canvas _customizationCanvas;
        [SerializeField] private TestNetworkManager _networkManager;
        [SerializeField] private SteamLobby _steamLobby;

        private void Awake()
        {
            _hostButton.onClick.AddListener(Host);
            _joinButton.onClick.AddListener(Join);
            _customizationButton.onClick.AddListener(OperCustomization);
            _steamLobby.OnLobbyCreatedEvent += OnLobbyCreated;
        }

        private void OnDestroy()
        {
            _hostButton.onClick.RemoveListener(Host);
            _joinButton.onClick.RemoveListener(Join);
            _customizationButton.onClick.RemoveListener(OperCustomization);
            _steamLobby.OnLobbyCreatedEvent -= OnLobbyCreated;
        }

        private void Host()
        {
            _steamLobby.HostLobby();
            // _networkManager.StartHost();
        }

        private void Join()
        {
            _steamLobby.OpenSteamFriends();
            /*var ipAddress = "localhost";
            _networkManager.networkAddress = ipAddress;
            _networkManager.StartClient();*/
        }

        private void OperCustomization()
        {
            _customizationCanvas.enabled = !_customizationCanvas.enabled;
        }

        private void OnLobbyCreated(bool obj)
        {
            _hostButton.interactable = !obj;
        }
    }
}