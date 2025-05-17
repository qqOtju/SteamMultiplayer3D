using System.Collections;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.Customization;
using Project.Scripts.Network;
using Steamworks;
using TMPro;
using UnityEngine;
using Zenject;

namespace Project.Scripts.TEST
{
    public class TestLobby : NetworkBehaviour
    {
        [SerializeField] private TestLobbyPlayer _playerPrefab;
        [SerializeField] private Transform _playerContainer;
        [SerializeField] private Showcase _showcasePrefab;
        [SerializeField] private Transform _showcaseContainer;
        [SerializeField] private TMP_Text _startTimer;
        [Scene] [SerializeField] private string _gameScene;
        private readonly List<TestLobbyPlayer> _players = new();

        private readonly List<NetworkBehaviour> _playersNet = new();
        private readonly List<NetworkBehaviour> _showcaseNet = new();
        private readonly List<Showcase> _showcases = new();

        private DiContainer _diContainer;

        private void Awake()
        {
            TestNetworkManager.OnServerAddPlayerAction += OnServerAddPlayer;
        }

        private void OnDestroy()
        {
            TestNetworkManager.OnServerAddPlayerAction -= OnServerAddPlayer;
        }

        [Inject]
        private void Construct(DiContainer diContainer, SkinData skinData)
        {
            _diContainer = diContainer;
        }

        [Server]
        private void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            SetPlayer(conn);
        }

        [Server]
        private void SetPlayer(NetworkConnectionToClient conn)
        {
            var showcase = Instantiate(_showcasePrefab, _showcaseContainer);
            var player = Instantiate(_playerPrefab, _playerContainer);
            NetworkServer.AddPlayerForConnection(conn, player.gameObject);
            NetworkServer.Spawn(showcase.gameObject, conn);
            var isLeader = conn.connectionId == 0;
            player.ServerSetPlayer(TestNetworkManager.SteamIDs[conn], false, isLeader);
            player.OnReadyChangedEvent += HandleOnReadyChanged;
            _playersNet.Add(player);
            _players.Add(player);
            _showcaseNet.Add(showcase);
            _showcases.Add(showcase);
            RpcSetPlayer(_playersNet, _showcaseNet);
        }

        [ClientRpc]
        private void RpcSetPlayer(List<NetworkBehaviour> playersNetworks, List<NetworkBehaviour> showcasesNetworks) //
        {
            var showcaseIndex = 0;
            foreach (var network in showcasesNetworks)
            {
                network.transform.SetParent(_showcaseContainer); //
                network.transform.position = new Vector3(2 * showcaseIndex, 0, 0);
                var showcase = network.gameObject.GetComponent<Showcase>();
                _diContainer.Inject(showcase.Player);
                showcaseIndex++;
            }

            showcaseIndex = 0;
            foreach (var network in playersNetworks)
            {
                network.transform.SetParent(_playerContainer); //
                var player = network.gameObject.GetComponent<TestLobbyPlayer>();
                var isLeader = player.IsLeader;
                var isReady = player.IsReady;
                player.ClientSetPlayer(isReady, isLeader, showcasesNetworks[showcaseIndex],
                    SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyID, showcaseIndex));
                showcaseIndex++;
            }
        }

        [Server]
        private void HandleOnReadyChanged(TestLobbyPlayer player, bool isReady)
        {
            foreach (var p in _players)
                if (!p.IsReady)
                {
                    StopAllCoroutines();
                    _startTimer.text = "Waiting for players...";
                    return;
                }

            RpcGameStartTimer();
            StartCoroutine(ServerGameStartTimer());
        }

        [ClientRpc]
        private void RpcGameStartTimer()
        {
            StartCoroutine(ClientGameStartTimer());
        }

        [Client]
        private IEnumerator ClientGameStartTimer()
        {
            var wfs = new WaitForSeconds(1);
            _startTimer.text = "Game starting in <color=red>3</color>...";
            yield return wfs;
            _startTimer.text = "Game starting in <color=red>2</color>...";
            yield return wfs;
            _startTimer.text = "Game starting in <color=red>1</color>...";
            yield return wfs;
            _startTimer.text = "Game starting...";
        }

        [Server]
        private IEnumerator ServerGameStartTimer()
        {
            var wfs = new WaitForSeconds(3);
            yield return wfs;
            foreach (var p in _players)
                p.OnReadyChangedEvent -= HandleOnReadyChanged;
            TestNetworkManager.Instance.ServerChangeScene(_gameScene);
        }
    }
}