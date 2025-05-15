using System;
using Mirror;
using Mirror.Steamworks.NET;
using Steamworks;
using UnityEngine;

namespace Project.Scripts.Network
{
    public class SteamLobby: MonoBehaviour
    {
        [SerializeField] private NetworkManager _networkManager;
        
        private const string HostAddressKey = "HostAddress";

        protected Callback<LobbyCreated_t> LobbyCreated;
        protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequested;
        protected Callback<LobbyEnter_t> LobbyEntered;
        
        public event Action<bool> OnLobbyCreatedEvent;

        public static CSteamID LobbyID { get; private set; }

        private void Start()
        {
            if (!SteamManager.Initialized) { return; }
            LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
            LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
        }
        
        public void OpenSteamFriends()
        {
            if (SteamManager.Initialized)
                SteamFriends.ActivateGameOverlay("Friends");
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                OnLobbyCreatedEvent?.Invoke(false);
                return;
            }
            LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
            _networkManager.StartHost();
            SteamMatchmaking.SetLobbyData(
                LobbyID,
                HostAddressKey,
                SteamUser.GetSteamID().ToString());
            OnLobbyCreatedEvent?.Invoke(true);
        }

        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (NetworkServer.active) { return; }
            var hostAddress = SteamMatchmaking.GetLobbyData(
                new CSteamID(callback.m_ulSteamIDLobby),
                HostAddressKey);
            _networkManager.networkAddress = hostAddress;
            _networkManager.StartClient();
        }
    }
}