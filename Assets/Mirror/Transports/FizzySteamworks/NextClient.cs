#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public class NextClient : NextCommon, IClient
    {
        private readonly List<Action> BufferedData;
        private Callback<SteamNetConnectionStatusChangedCallback_t> c_onConnectionChange;

        private CancellationTokenSource cancelToken;
        private TaskCompletionSource<Task> connectedComplete;

        private readonly TimeSpan ConnectionTimeout;
        private HSteamNetConnection HostConnection;
        private CSteamID hostSteamID = CSteamID.Nil;

        private NextClient(FizzySteamworks transport)
        {
            ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.Timeout));
            BufferedData = new List<Action>();
        }

        public bool Connected { get; private set; }
        public bool Error { get; private set; }

        public void Disconnect()
        {
            cancelToken?.Cancel();
            Dispose();

            if (Connected) InternalDisconnect();

            if (HostConnection.m_HSteamNetConnection != 0)
            {
                Debug.Log("Sending Disconnect message");
                SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Graceful disconnect", false);
                HostConnection.m_HSteamNetConnection = 0;
            }
        }

        public void ReceiveData()
        {
            var ptrs = new IntPtr[MAX_MESSAGES];
            int messageCount;

            if ((messageCount =
                    SteamNetworkingSockets.ReceiveMessagesOnConnection(HostConnection, ptrs, MAX_MESSAGES)) > 0)
                for (var i = 0; i < messageCount; i++)
                {
                    var (data, ch) = ProcessMessage(ptrs[i]);
                    if (Connected)
                        OnReceivedData(data, ch);
                    else
                        BufferedData.Add(() => OnReceivedData(data, ch));
                }
        }

        public void Send(byte[] data, int channelId)
        {
            try
            {
                var res = SendSocket(HostConnection, data, channelId);

                if (res == EResult.k_EResultNoConnection || res == EResult.k_EResultInvalidParam)
                {
                    Debug.Log("Connection to server was lost.");
                    InternalDisconnect();
                }
                else if (res != EResult.k_EResultOK)
                {
                    Debug.LogError($"Could not send: {res.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SteamNetworking exception during Send: {ex.Message}");
                InternalDisconnect();
            }
        }

        public void FlushData()
        {
            SteamNetworkingSockets.FlushMessagesOnConnection(HostConnection);
        }

        private event Action<byte[], int> OnReceivedData;
        private event Action OnConnected;
        private event Action OnDisconnected;

        public static NextClient CreateClient(FizzySteamworks transport, string host)
        {
            var c = new NextClient(transport);

            c.OnConnected += () => transport.OnClientConnected.Invoke();
            c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
            c.OnReceivedData += (data, ch) => transport.OnClientDataReceived.Invoke(new ArraySegment<byte>(data), ch);

            try
            {
#if UNITY_SERVER
                SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                SteamNetworkingUtils.InitRelayNetworkAccess();
#endif
                c.Connect(host);
            }
            catch (FormatException)
            {
                Debug.LogError("Connection string was not in the right format. Did you enter a SteamId?");
                c.Error = true;
                c.OnConnectionFailed();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected exception: {ex.Message}");
                c.Error = true;
                c.OnConnectionFailed();
            }

            return c;
        }

        private async void Connect(string host)
        {
            cancelToken = new CancellationTokenSource();
            c_onConnectionChange =
                Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionStatusChanged);

            try
            {
                hostSteamID = new CSteamID(ulong.Parse(host));
                connectedComplete = new TaskCompletionSource<Task>();
                OnConnected += SetConnectedComplete;

                var smi = new SteamNetworkingIdentity();
                smi.SetSteamID(hostSteamID);

                var options = new SteamNetworkingConfigValue_t[] { };
                HostConnection = SteamNetworkingSockets.ConnectP2P(ref smi, 0, options.Length, options);

                Task connectedCompleteTask = connectedComplete.Task;
                var timeOutTask = Task.Delay(ConnectionTimeout, cancelToken.Token);

                if (await Task.WhenAny(connectedCompleteTask, timeOutTask) != connectedCompleteTask)
                {
                    if (cancelToken.IsCancellationRequested)
                        Debug.LogError("The connection attempt was cancelled.");
                    else if (timeOutTask.IsCompleted) Debug.LogError($"Connection to {host} timed out.");

                    OnConnected -= SetConnectedComplete;
                    OnConnectionFailed();
                }

                OnConnected -= SetConnectedComplete;
            }
            catch (FormatException)
            {
                Debug.LogError("Connection string was not in the right format. Did you enter a SteamId?");
                Error = true;
                OnConnectionFailed();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected exception: {ex.Message}");
                Error = true;
                OnConnectionFailed();
            }
            finally
            {
                if (Error)
                {
                    Debug.LogError("Connection failed.");
                    OnConnectionFailed();
                }
            }
        }

        private void OnConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t param)
        {
            var clientSteamID = param.m_info.m_identityRemote.GetSteamID64();
            if (param.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                Connected = true;
                OnConnected.Invoke();
                Debug.Log("Connection established.");

                if (BufferedData.Count > 0)
                {
                    Debug.Log($"{BufferedData.Count} received before connection was established. Processing now.");
                    {
                        foreach (var a in BufferedData) a();
                    }
                }
            }
            else if (param.m_info.m_eState ==
                     ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer ||
                     param.m_info.m_eState == ESteamNetworkingConnectionState
                         .k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                Debug.Log($"Connection was closed by peer, {param.m_info.m_szEndDebug}");
                Disconnect();
            }
            else
            {
                Debug.Log(
                    $"Connection state changed: {param.m_info.m_eState.ToString()} - {param.m_info.m_szEndDebug}");
            }
        }

        protected void Dispose()
        {
            if (c_onConnectionChange != null)
            {
                c_onConnectionChange.Dispose();
                c_onConnectionChange = null;
            }
        }

        private void InternalDisconnect()
        {
            Connected = false;
            OnDisconnected.Invoke();
            Debug.Log("Disconnected.");
            SteamNetworkingSockets.CloseConnection(HostConnection, 0, "Disconnected", false);
        }

        private void SetConnectedComplete()
        {
            connectedComplete.SetResult(connectedComplete.Task);
        }

        private void OnConnectionFailed()
        {
            OnDisconnected.Invoke();
        }
    }
}
#endif // !DISABLESTEAMWORKS