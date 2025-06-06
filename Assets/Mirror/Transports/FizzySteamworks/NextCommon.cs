#if !DISABLESTEAMWORKS
using System;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public abstract class NextCommon
    {
        protected const int MAX_MESSAGES = 256;

        protected EResult SendSocket(HSteamNetConnection conn, byte[] data, int channelId)
        {
            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = (byte)channelId;

            var pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            var pData = pinnedArray.AddrOfPinnedObject();
            var sendFlag = channelId == Channels.Unreliable
                ? Constants.k_nSteamNetworkingSend_Unreliable
                : Constants.k_nSteamNetworkingSend_Reliable;
#if UNITY_SERVER
            EResult res =
 SteamGameServerNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag, out long _);
#else
            var res = SteamNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, sendFlag,
                out var _);
#endif
            if (res != EResult.k_EResultOK) Debug.LogWarning($"Send issue: {res}");

            pinnedArray.Free();
            return res;
        }

        protected (byte[], int) ProcessMessage(IntPtr ptrs)
        {
            var data = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ptrs);
            var managedArray = new byte[data.m_cbSize];
            Marshal.Copy(data.m_pData, managedArray, 0, data.m_cbSize);
            SteamNetworkingMessage_t.Release(ptrs);

            int channel = managedArray[managedArray.Length - 1];
            Array.Resize(ref managedArray, managedArray.Length - 1);
            return (managedArray, channel);
        }
    }
}
#endif // !DISABLESTEAMWORKS