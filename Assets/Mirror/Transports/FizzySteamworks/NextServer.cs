#if !DISABLESTEAMWORKS
using System;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public class NextServer : NextCommon, IServer
    {
        private static NextServer server;

        private Callback<SteamNetConnectionStatusChangedCallback_t> c_onConnectionChange;

        private readonly BidirectionalDictionary<HSteamNetConnection, int> connToMirrorID;

        private HSteamListenSocket listenSocket;
        private readonly int maxConnections;
        private int nextConnectionID;
        private readonly BidirectionalDictionary<CSteamID, int> steamIDToMirrorID;

        private NextServer(int maxConnections)
        {
            this.maxConnections = maxConnections;
            connToMirrorID = new BidirectionalDictionary<HSteamNetConnection, int>();
            steamIDToMirrorID = new BidirectionalDictionary<CSteamID, int>();
            nextConnectionID = 1;
#if UNITY_SERVER
            c_onConnectionChange =
 Callback<SteamNetConnectionStatusChangedCallback_t>.CreateGameServer(OnConnectionStatusChanged);
#else
            c_onConnectionChange =
                Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);
#endif
        }

        public void Disconnect(int connectionId)
        {
            if (connToMirrorID.TryGetValue(connectionId, out var conn))
            {
                Debug.Log($"Connection id {connectionId} disconnected.");
#if UNITY_SERVER
                SteamGameServerNetworkingSockets.CloseConnection(conn, 0, "Disconnected by server", false);
#else
                SteamNetworkingSockets.CloseConnection(conn, 0, "Disconnected by server", false);
#endif
                steamIDToMirrorID.Remove(connectionId);
                connToMirrorID.Remove(connectionId);
                OnDisconnected?.Invoke(connectionId);
            }
            else
            {
                Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
            }
        }

        public void FlushData()
        {
            foreach (var conn in connToMirrorID.FirstTypes.ToList())
            {
#if UNITY_SERVER
                SteamGameServerNetworkingSockets.FlushMessagesOnConnection(conn);
#else
                SteamNetworkingSockets.FlushMessagesOnConnection(conn);
#endif
            }
        }

        public void ReceiveData()
        {
            foreach (var conn in connToMirrorID.FirstTypes.ToList())
                if (connToMirrorID.TryGetValue(conn, out var connId))
                {
                    var ptrs = new IntPtr[MAX_MESSAGES];
                    int messageCount;

#if UNITY_SERVER
                    if ((messageCount =
 SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection(conn, ptrs, MAX_MESSAGES)) > 0)
#else
                    if ((messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, ptrs, MAX_MESSAGES)) >
                        0)
#endif
                        for (var i = 0; i < messageCount; i++)
                        {
                            var (data, ch) = ProcessMessage(ptrs[i]);
                            OnReceivedData?.Invoke(connId, data, ch);
                        }
                }
        }

        public void Send(int connectionId, byte[] data, int channelId)
        {
            if (connToMirrorID.TryGetValue(connectionId, out var conn))
            {
                var res = SendSocket(conn, data, channelId);

                if (res == EResult.k_EResultNoConnection || res == EResult.k_EResultInvalidParam)
                {
                    Debug.Log($"Connection to {connectionId} was lost.");
                    InternalDisconnect(connectionId, conn);
                }
                else if (res != EResult.k_EResultOK)
                {
                    Debug.LogError($"Could not send: {res}");
                }
            }
            else
            {
                Debug.LogError("Trying to send on an unknown connection: " + connectionId);
                OnReceivedError?.Invoke(connectionId, TransportError.Unexpected, "ERROR Unknown Connection");
            }
        }

        public string ServerGetClientAddress(int connectionId)
        {
            if (steamIDToMirrorID.TryGetValue(connectionId, out var steamId)) return steamId.ToString();

            Debug.LogError("Trying to get info on an unknown connection: " + connectionId);
            OnReceivedError?.Invoke(connectionId, TransportError.Unexpected, "ERROR Unknown Connection");
            return string.Empty;
        }

        public void Shutdown()
        {
#if UNITY_SERVER
            SteamGameServerNetworkingSockets.CloseListenSocket(listenSocket);
#else
            SteamNetworkingSockets.CloseListenSocket(listenSocket);
#endif

            c_onConnectionChange?.Dispose();
            c_onConnectionChange = null;
        }

        private event Action<int, string> OnConnectedWithAddress;
        private event Action<int, byte[], int> OnReceivedData;
        private event Action<int> OnDisconnected;
        private event Action<int, TransportError, string> OnReceivedError;

        public static NextServer CreateServer(FizzySteamworks transport, int maxConnections)
        {
            server = new NextServer(maxConnections);

            server.OnConnectedWithAddress += (id, addres) => transport.OnServerConnectedWithAddress.Invoke(id, addres);
            server.OnDisconnected += id => transport.OnServerDisconnected.Invoke(id);
            server.OnReceivedData += (id, data, ch) =>
                transport.OnServerDataReceived.Invoke(id, new ArraySegment<byte>(data), ch);
            server.OnReceivedError += (id, error, reason) => transport.OnServerError.Invoke(id, error, reason);

            try
            {
#if UNITY_SERVER
                SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                SteamNetworkingUtils.InitRelayNetworkAccess();
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            server.Host();

            return server;
        }

        private void Host()
        {
            var options = new SteamNetworkingConfigValue_t[] { };
#if UNITY_SERVER
            listenSocket = SteamGameServerNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
#else
            listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
#endif
        }

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
        {
            var clientSteamID = param.m_info.m_identityRemote.GetSteamID64();
            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
            {
                if (connToMirrorID.Count >= maxConnections)
                {
                    Debug.Log($"Incoming connection {clientSteamID} would exceed max connection count. Rejecting.");
#if UNITY_SERVER
                    SteamGameServerNetworkingSockets.CloseConnection(param.m_hConn, 0, "Max Connection Count", false);
#else
                    SteamNetworkingSockets.CloseConnection(param.m_hConn, 0, "Max Connection Count", false);
#endif
                    return;
                }

                EResult res;

#if UNITY_SERVER
                if ((res = SteamGameServerNetworkingSockets.AcceptConnection(param.m_hConn)) == EResult.k_EResultOK)
#else
                if ((res = SteamNetworkingSockets.AcceptConnection(param.m_hConn)) == EResult.k_EResultOK)
#endif
                    Debug.Log($"Accepting connection {clientSteamID}");
                else
                    Debug.Log($"Connection {clientSteamID} could not be accepted: {res}");
            }
            else if (param.m_info.m_eState ==
                     ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                var connectionId = nextConnectionID++;
                connToMirrorID.Add(param.m_hConn, connectionId);
                steamIDToMirrorID.Add(param.m_info.m_identityRemote.GetSteamID(), connectionId);
                OnConnectedWithAddress?.Invoke(connectionId, server.ServerGetClientAddress(connectionId));
                Debug.Log($"Client with SteamID {clientSteamID} connected. Assigning connection id {connectionId}");
            }
            else if (param.m_info.m_eState ==
                     ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer ||
                     param.m_info.m_eState == ESteamNetworkingConnectionState
                         .k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                if (connToMirrorID.TryGetValue(param.m_hConn, out var connId))
                    InternalDisconnect(connId, param.m_hConn);
            }
            else
            {
                Debug.Log($"Connection {clientSteamID} state changed: {param.m_info.m_eState}");
            }
        }

        private void InternalDisconnect(int connId, HSteamNetConnection socket)
        {
            OnDisconnected?.Invoke(connId);
#if UNITY_SERVER
            SteamGameServerNetworkingSockets.CloseConnection(socket, 0, "Graceful disconnect", false);
#else
            SteamNetworkingSockets.CloseConnection(socket, 0, "Graceful disconnect", false);
#endif
            connToMirrorID.Remove(connId);
            steamIDToMirrorID.Remove(connId);
            Debug.Log($"Client with ConnectionID {connId} disconnected.");
        }
    }
}
#endif // !DISABLESTEAMWORKS