#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public class LegacyServer : LegacyCommon, IServer
    {
        private static LegacyServer server;
        private readonly int maxConnections;
        private int nextConnectionID;

        private readonly BidirectionalDictionary<CSteamID, int> steamToMirrorIds;

        private LegacyServer(FizzySteamworks transport, int maxConnections) : base(transport)
        {
            this.maxConnections = maxConnections;
            steamToMirrorIds = new BidirectionalDictionary<CSteamID, int>();
            nextConnectionID = 1;
        }

        public void Disconnect(int connectionId)
        {
            if (steamToMirrorIds.TryGetValue(connectionId, out var steamID))
            {
                SendInternal(steamID, InternalMessages.DISCONNECT);
                steamToMirrorIds.Remove(connectionId);
            }
            else
            {
                Debug.LogWarning("Trying to disconnect unknown connection id: " + connectionId);
            }
        }

        public void Shutdown()
        {
            foreach (KeyValuePair<CSteamID, int> client in steamToMirrorIds)
            {
                Disconnect(client.Value);
                WaitForClose(client.Key);
            }

            Dispose();
        }

        public void Send(int connectionId, byte[] data, int channelId)
        {
            if (steamToMirrorIds.TryGetValue(connectionId, out var steamId))
            {
                Send(steamId, data, channelId);
            }
            else
            {
                Debug.LogError("Trying to send on unknown connection: " + connectionId);
                OnReceivedError.Invoke(connectionId, TransportError.Unexpected, "ERROR Unknown Connection");
                Shutdown();
            }
        }

        public string ServerGetClientAddress(int connectionId)
        {
            if (steamToMirrorIds.TryGetValue(connectionId, out var steamId)) return steamId.ToString();

            Debug.LogError("Trying to get info on unknown connection: " + connectionId);
            OnReceivedError.Invoke(connectionId, TransportError.Unexpected, "ERROR Unknown Connection");
            return string.Empty;
        }

        public void FlushData()
        {
        }

        private event Action<int, string> OnConnectedWithAddress;
        private event Action<int, byte[], int> OnReceivedData;
        private event Action<int> OnDisconnected;
        private event Action<int, TransportError, string> OnReceivedError;

        public static LegacyServer CreateServer(FizzySteamworks transport, int maxConnections)
        {
            server = new LegacyServer(transport, maxConnections);

            server.OnConnectedWithAddress += (id, addres) => transport.OnServerConnectedWithAddress.Invoke(id, addres);
            server.OnDisconnected += id => transport.OnServerDisconnected.Invoke(id);
            server.OnReceivedData += (id, data, channel) =>
                transport.OnServerDataReceived.Invoke(id, new ArraySegment<byte>(data), channel);
            server.OnReceivedError += (id, error, reason) => transport.OnServerError.Invoke(id, error, reason);

            try
            {
#if UNITY_SERVER
                InteropHelp.TestIfAvailableGameServer();
#else
                InteropHelp.TestIfAvailableClient();
#endif
            }
            catch
            {
                Debug.LogError("SteamWorks not initialized.");
            }

            return server;
        }

        protected override void OnNewConnection(P2PSessionRequest_t result)
        {
            try
            {
#if UNITY_SERVER
            SteamGameServerNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
#else
                SteamNetworking.AcceptP2PSessionWithUser(result.m_steamIDRemote);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Steam Server error durring new connect, {ex.Message}");
                Shutdown();
            }
        }

        protected override void OnReceiveInternalData(InternalMessages type, CSteamID clientSteamID)
        {
            switch (type)
            {
                case InternalMessages.CONNECT:
                    if (steamToMirrorIds.Count >= maxConnections)
                    {
                        SendInternal(clientSteamID, InternalMessages.DISCONNECT);
                        return;
                    }

                    SendInternal(clientSteamID, InternalMessages.ACCEPT_CONNECT);

                    var connectionId = nextConnectionID++;
                    steamToMirrorIds.Add(clientSteamID, connectionId);
                    OnConnectedWithAddress.Invoke(connectionId, server.ServerGetClientAddress(connectionId));
                    Debug.Log($"Client with SteamID {clientSteamID} connected. Assigning connection id {connectionId}");
                    break;
                case InternalMessages.DISCONNECT:
                    if (steamToMirrorIds.TryGetValue(clientSteamID, out var connId))
                    {
                        OnDisconnected.Invoke(connId);
                        CloseP2PSessionWithUser(clientSteamID);
                        steamToMirrorIds.Remove(clientSteamID);
                        Debug.Log($"Client with SteamID {clientSteamID} disconnected.");
                    }

                    break;
                default:
                    Debug.Log("Received unknown message type");
                    break;
            }
        }

        protected override void OnReceiveData(byte[] data, CSteamID clientSteamID, int channel)
        {
            try
            {
                if (steamToMirrorIds.TryGetValue(clientSteamID, out var connectionId))
                {
                    OnReceivedData.Invoke(connectionId, data, channel);
                }
                else
                {
                    CloseP2PSessionWithUser(clientSteamID);
                    Debug.LogError("Data received from steam client thats not known " + clientSteamID);
                    OnReceivedError.Invoke(-1, TransportError.DnsResolve, "ERROR Unknown SteamID");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while recive data {ex.Message}");
                Shutdown();
            }
        }

        protected override void OnConnectionFailed(CSteamID remoteId)
        {
            var connectionId = steamToMirrorIds.TryGetValue(remoteId, out var connId) ? connId : nextConnectionID++;
            OnDisconnected.Invoke(connectionId);

            steamToMirrorIds.Remove(remoteId);
        }
    }
}
#endif // !DISABLESTEAMWORKS