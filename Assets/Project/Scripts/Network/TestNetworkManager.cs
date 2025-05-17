using System;
using System.Collections.Generic;
using Mirror;
using Project.Scripts.Network;
using Steamworks;
using UnityEngine;

namespace Project.Scripts.TEST
{
    public class TestNetworkManager : NetworkManager
    {
        [Scene] [SerializeField] private string _gameScene;

        public static TestNetworkManager Instance { get; private set; }
        public static List<NetworkConnectionToClient> RoomConnections { get; } = new();
        public static Dictionary<NetworkConnectionToClient, CSteamID> SteamIDs { get; } = new();

        public override void Awake()
        {
            base.Awake();
            //ToDo: Remove this singleton pattern and use Zenject instead
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static event Action<NetworkConnectionToClient> OnServerAddPlayerAction;
        public static event Action OnGameStarted;

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Server started");
        }

        public override void OnStartHost()
        {
            base.OnStartHost();
            Debug.Log("Host started");
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            Debug.Log("Client connected to server");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("Client started");
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            Debug.Log("Client is ready");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log("Server adding player");
            RoomConnections.Add(conn);
            var steamId = SteamMatchmaking.GetLobbyMemberByIndex(SteamLobby.LobbyID, numPlayers - 1);
            SteamIDs.Add(conn, steamId);
            OnServerAddPlayerAction?.Invoke(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected to server");
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            if (sceneName == _gameScene)
                OnGameStarted?.Invoke();
        }
    }
}