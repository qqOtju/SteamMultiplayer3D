using System.Collections.Generic;
using Mirror;
using Project.Scripts.Customization;
using Project.Scripts.Network;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Project.Scripts.UI.MainMenu
{
    public class UILobby : NetworkBehaviour
    {
        [SerializeField] private UILobbyPlayer _lobbyPlayerPrefab;
        [SerializeField] private Transform _lobbyPlayerContainer;
        [SerializeField] private Showcase _showcasePrefab;
        [SerializeField] private Transform _showcaseContainer;

        private readonly List<UILobbyPlayer> _lobbyPlayers = new();
        private readonly List<string> _names = new();
        private readonly List<Showcase> _showcases = new();

        private DiContainer _diContainer;

        private void Awake()
        {
            gameObject.SetActive(true);
            CustomNetworkManager.OnServerAddPlayerAction += HandleServerAddPlayer;
        }

        private void OnDestroy()
        {
            CustomNetworkManager.OnServerAddPlayerAction -= HandleServerAddPlayer;
        }

        [Inject]
        private void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        [Server]
        private UILobbyPlayer HandleServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"Server: HandleServerAddPlayer {conn}");
            var player = Instantiate(_lobbyPlayerPrefab, _lobbyPlayerContainer);
            var showcase = Instantiate(_showcasePrefab, _showcaseContainer);
            showcase.transform.position = new Vector3(2 * _showcases.Count, 0, 0);
            NetworkServer.Spawn(showcase.gameObject, conn);
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
            _diContainer.Inject(showcase.Player);
            var playerName = $"Player {Random.Range(0, 1000)}";
            var showcaseName = $"{playerName} showcase";
            player.SetPlayerName(playerName);
            showcase.gameObject.name = showcaseName;
            RpcInjectUILobbyPlayer(player);
            RpcShowcaseSetup(player, showcase);
            _lobbyPlayers.Add(player);
            _showcases.Add(showcase);
            _names.Add(playerName);
            RpcSetPlayerParent(_lobbyPlayers);
            return player;
        }

        [ClientRpc]
        private void RpcShowcaseSetup(NetworkBehaviour networkPlayer, NetworkBehaviour networkShowcase)
        {
            var player = networkPlayer.gameObject.GetComponent<UILobbyPlayer>();
            var showcase = networkShowcase.GetComponent<Showcase>();
            // showcase.Initialize();
            player.RpcSetShowcase(networkShowcase);
        }

        [ClientRpc]
        private void RpcSetPlayerParent(List<UILobbyPlayer> lobbyPlayers)
        {
            foreach (var player in lobbyPlayers)
                player.transform.SetParent(_lobbyPlayerContainer);
        }

        [ClientRpc]
        private void RpcInjectUILobbyPlayer(NetworkBehaviour network)
        {
            var player = network.GetComponent<UILobbyPlayer>();
            _diContainer.Inject(player);
        }
    }
}