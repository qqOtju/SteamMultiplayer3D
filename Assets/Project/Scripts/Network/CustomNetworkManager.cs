using System;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.UI.MainMenu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Network
{
    public class CustomNetworkManager : NetworkManager
    {
        private const int LobbySceneIndex = 1;
        [Scene] [SerializeField] private string _gameScene;

        public static List<UILobbyPlayer> RoomPlayers { get; } = new();
        public static List<NetworkConnectionToClient> RoomConnections { get; } = new();

        public static event Func<NetworkConnectionToClient, UILobbyPlayer> OnServerAddPlayerAction;
        public static event Action OnGameStarted;

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            //Prevent players from connecting to running game. Maybe change this later
            if (SceneManager.GetActiveScene().buildIndex != LobbySceneIndex)
                conn.Disconnect();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (SceneManager.GetActiveScene().buildIndex != LobbySceneIndex) return;
            var isLeader = RoomPlayers.Count == 0;
            var player = OnServerAddPlayerAction?.Invoke(conn);
            Debug.Log("OnServerAddPlayer: " + player);
            if (player == null)
            {
                Debug.Log("OnServerAddPlayerAction returned null" + player);
                return;
            }

            Debug.Log("OnServerAddPlayerAction: " + player);
            if (isLeader)
            {
                player.OnStartGame += HandleStartGame;
                player.SetReadyStatus(true);
                player.RpcUpdateReadyStatus(true);
            }
            else
            {
                player.SetReadyStatus(false);
                player.RpcUpdateReadyStatus(false);
            }

            player.SetLeader(isLeader);
            player.RpcUpdateLeaderStatus(isLeader);
            RoomPlayers.Add(player);
            RoomConnections.Add(conn);
        }

        private void HandleStartGame(UILobbyPlayer player)
        {
            player.OnStartGame -= HandleStartGame;
            ServerChangeScene(_gameScene);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            if (sceneName == _gameScene)
                OnGameStarted?.Invoke();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                var player = conn.identity.GetComponent<UILobbyPlayer>();
                if (player == null) return;
                RoomPlayers.Remove(player);
                // NotifyPlayersOfReadyStatus();
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            RoomPlayers.Clear();
        }

        /*private void NotifyPlayersOfReadyStatus()
        {
            var isReadyToStart = IsReadyToStart();
            foreach (var player in RoomPlayers)
                player.HandleReadyToStart(isReadyToStart);
        }*/

        private bool IsReadyToStart()
        {
            foreach (var player in RoomPlayers)
                if (!player.IsReady)
                    return false;
            return true;
        }
    }
}