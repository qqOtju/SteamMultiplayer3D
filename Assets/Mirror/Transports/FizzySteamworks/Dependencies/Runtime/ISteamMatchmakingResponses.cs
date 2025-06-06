// This file is provided under The MIT License as part of Steamworks.NET.
// Copyright (c) 2013-2019 Riley Labrecque
// Please see the included LICENSE.txt for additional information.

// This file is automatically generated.
// Changes to this file will be reverted when you update Steamworks.NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS

// Unity 32bit Mono on Windows crashes with ThisCall for some reason, StdCall without the 'this' ptr is the only thing that works..?
#if (UNITY_EDITOR_WIN && !UNITY_EDITOR_64) || (!UNITY_EDITOR && UNITY_STANDALONE_WIN && !UNITY_64)
	#define NOTHISPTR
#endif

using System;
using System.Runtime.InteropServices;

namespace Steamworks
{
    //-----------------------------------------------------------------------------
    // Purpose: Callback interface for receiving responses after a server list refresh
    // or an individual server update.
    //
    // Since you get these callbacks after requesting full list refreshes you will
    // usually implement this interface inside an object like CServerBrowser.  If that
    // object is getting destructed you should use ISteamMatchMakingServers()->CancelQuery()
    // to cancel any in-progress queries so you don't get a callback into the destructed
    // object and crash.
    //-----------------------------------------------------------------------------
    public class ISteamMatchmakingServerListResponse
    {
        // Server has responded ok with updated data
        public delegate void ServerResponded(HServerListRequest hRequest, int iServer);

        // Server has failed to respond
        public delegate void ServerFailedToRespond(HServerListRequest hRequest, int iServer);

        // A list refresh you had initiated is now 100% completed
        public delegate void RefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response);

        private readonly VTable m_VTable;
        private readonly IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private readonly ServerResponded m_ServerResponded;
        private readonly ServerFailedToRespond m_ServerFailedToRespond;
        private readonly RefreshComplete m_RefreshComplete;

        public ISteamMatchmakingServerListResponse(ServerResponded onServerResponded,
            ServerFailedToRespond onServerFailedToRespond, RefreshComplete onRefreshComplete)
        {
            if (onServerResponded == null || onServerFailedToRespond == null || onRefreshComplete == null)
                throw new ArgumentNullException();
            m_ServerResponded = onServerResponded;
            m_ServerFailedToRespond = onServerFailedToRespond;
            m_RefreshComplete = onRefreshComplete;

            m_VTable = new VTable
            {
                m_VTServerResponded = InternalOnServerResponded,
                m_VTServerFailedToRespond = InternalOnServerFailedToRespond,
                m_VTRefreshComplete = InternalOnRefreshComplete
            };
            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);

            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
        }

        ~ISteamMatchmakingServerListResponse()
        {
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);

            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

#if NOTHISPTR
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InternalServerResponded(HServerListRequest hRequest, int iServer);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InternalServerFailedToRespond(HServerListRequest hRequest, int iServer);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InternalRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response);
		private void InternalOnServerResponded(HServerListRequest hRequest, int iServer) {
			try
			{
				m_ServerResponded(hRequest, iServer);
			}
			catch (Exception e)
			{
				CallbackDispatcher.ExceptionHandler(e);
			}
		}
		private void InternalOnServerFailedToRespond(HServerListRequest hRequest, int iServer) {
			try
			{
				m_ServerFailedToRespond(hRequest, iServer);
			}
			catch (Exception e)
			{
				CallbackDispatcher.ExceptionHandler(e);
			}
		}
		private void InternalOnRefreshComplete(HServerListRequest hRequest, EMatchMakingServerResponse response) {
			try
			{
				m_RefreshComplete(hRequest, response);
			}
			catch (Exception e)
			{
				CallbackDispatcher.ExceptionHandler(e);
			}
		}
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalRefreshComplete(IntPtr thisptr, HServerListRequest hRequest,
            EMatchMakingServerResponse response);

        private void InternalOnServerResponded(IntPtr thisptr, HServerListRequest hRequest, int iServer)
        {
            try
            {
                m_ServerResponded(hRequest, iServer);
            }
            catch (Exception e)
            {
                CallbackDispatcher.ExceptionHandler(e);
            }
        }

        private void InternalOnServerFailedToRespond(IntPtr thisptr, HServerListRequest hRequest, int iServer)
        {
            try
            {
                m_ServerFailedToRespond(hRequest, iServer);
            }
            catch (Exception e)
            {
                CallbackDispatcher.ExceptionHandler(e);
            }
        }

        private void InternalOnRefreshComplete(IntPtr thisptr, HServerListRequest hRequest,
            EMatchMakingServerResponse response)
        {
            try
            {
                m_RefreshComplete(hRequest, response);
            }
            catch (Exception e)
            {
                CallbackDispatcher.ExceptionHandler(e);
            }
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private class VTable
        {
            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalServerResponded m_VTServerResponded;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalServerFailedToRespond m_VTServerFailedToRespond;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalRefreshComplete m_VTRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingServerListResponse that)
        {
            return that.m_pGCHandle.AddrOfPinnedObject();
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Callback interface for receiving responses after pinging an individual server
    //
    // These callbacks all occur in response to querying an individual server
    // via the ISteamMatchmakingServers()->PingServer() call below.  If you are
    // destructing an object that implements this interface then you should call
    // ISteamMatchmakingServers()->CancelServerQuery() passing in the handle to the query
    // which is in progress.  Failure to cancel in progress queries when destructing
    // a callback handler may result in a crash when a callback later occurs.
    //-----------------------------------------------------------------------------
    public class ISteamMatchmakingPingResponse
    {
        // Server has responded successfully and has updated data
        public delegate void ServerResponded(gameserveritem_t server);

        // Server failed to respond to the ping request
        public delegate void ServerFailedToRespond();

        private readonly VTable m_VTable;
        private readonly IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private readonly ServerResponded m_ServerResponded;
        private readonly ServerFailedToRespond m_ServerFailedToRespond;

        public ISteamMatchmakingPingResponse(ServerResponded onServerResponded,
            ServerFailedToRespond onServerFailedToRespond)
        {
            if (onServerResponded == null || onServerFailedToRespond == null) throw new ArgumentNullException();
            m_ServerResponded = onServerResponded;
            m_ServerFailedToRespond = onServerFailedToRespond;

            m_VTable = new VTable
            {
                m_VTServerResponded = InternalOnServerResponded,
                m_VTServerFailedToRespond = InternalOnServerFailedToRespond
            };
            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);

            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
        }

        ~ISteamMatchmakingPingResponse()
        {
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);

            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

#if NOTHISPTR
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InternalServerResponded(gameserveritem_t server);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void InternalServerFailedToRespond();
		private void InternalOnServerResponded(gameserveritem_t server) {
			m_ServerResponded(server);
		}
		private void InternalOnServerFailedToRespond() {
			m_ServerFailedToRespond();
		}
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerResponded(IntPtr thisptr, gameserveritem_t server);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void InternalServerFailedToRespond(IntPtr thisptr);

        private void InternalOnServerResponded(IntPtr thisptr, gameserveritem_t server)
        {
            m_ServerResponded(server);
        }

        private void InternalOnServerFailedToRespond(IntPtr thisptr)
        {
            m_ServerFailedToRespond();
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private class VTable
        {
            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalServerResponded m_VTServerResponded;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalServerFailedToRespond m_VTServerFailedToRespond;
        }

        public static explicit operator IntPtr(ISteamMatchmakingPingResponse that)
        {
            return that.m_pGCHandle.AddrOfPinnedObject();
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Callback interface for receiving responses after requesting details on
    // who is playing on a particular server.
    //
    // These callbacks all occur in response to querying an individual server
    // via the ISteamMatchmakingServers()->PlayerDetails() call below.  If you are
    // destructing an object that implements this interface then you should call
    // ISteamMatchmakingServers()->CancelServerQuery() passing in the handle to the query
    // which is in progress.  Failure to cancel in progress queries when destructing
    // a callback handler may result in a crash when a callback later occurs.
    //-----------------------------------------------------------------------------
    public class ISteamMatchmakingPlayersResponse
    {
        // Got data on a new player on the server -- you'll get this callback once per player
        // on the server which you have requested player data on.
        public delegate void AddPlayerToList(string pchName, int nScore, float flTimePlayed);

        // The server failed to respond to the request for player details
        public delegate void PlayersFailedToRespond();

        // The server has finished responding to the player details request
        // (ie, you won't get anymore AddPlayerToList callbacks)
        public delegate void PlayersRefreshComplete();

        private readonly VTable m_VTable;
        private readonly IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private readonly AddPlayerToList m_AddPlayerToList;
        private readonly PlayersFailedToRespond m_PlayersFailedToRespond;
        private readonly PlayersRefreshComplete m_PlayersRefreshComplete;

        public ISteamMatchmakingPlayersResponse(AddPlayerToList onAddPlayerToList,
            PlayersFailedToRespond onPlayersFailedToRespond, PlayersRefreshComplete onPlayersRefreshComplete)
        {
            if (onAddPlayerToList == null || onPlayersFailedToRespond == null || onPlayersRefreshComplete == null)
                throw new ArgumentNullException();
            m_AddPlayerToList = onAddPlayerToList;
            m_PlayersFailedToRespond = onPlayersFailedToRespond;
            m_PlayersRefreshComplete = onPlayersRefreshComplete;

            m_VTable = new VTable
            {
                m_VTAddPlayerToList = InternalOnAddPlayerToList,
                m_VTPlayersFailedToRespond = InternalOnPlayersFailedToRespond,
                m_VTPlayersRefreshComplete = InternalOnPlayersRefreshComplete
            };
            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);

            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
        }

        ~ISteamMatchmakingPlayersResponse()
        {
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);

            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

#if NOTHISPTR
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalAddPlayerToList(IntPtr pchName, int nScore, float flTimePlayed);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalPlayersFailedToRespond();
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalPlayersRefreshComplete();
		private void InternalOnAddPlayerToList(IntPtr pchName, int nScore, float flTimePlayed) {
			m_AddPlayerToList(InteropHelp.PtrToStringUTF8(pchName), nScore, flTimePlayed);
		}
		private void InternalOnPlayersFailedToRespond() {
			m_PlayersFailedToRespond();
		}
		private void InternalOnPlayersRefreshComplete() {
			m_PlayersRefreshComplete();
		}
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalAddPlayerToList(IntPtr thisptr, IntPtr pchName, int nScore, float flTimePlayed);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalPlayersFailedToRespond(IntPtr thisptr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalPlayersRefreshComplete(IntPtr thisptr);

        private void InternalOnAddPlayerToList(IntPtr thisptr, IntPtr pchName, int nScore, float flTimePlayed)
        {
            m_AddPlayerToList(InteropHelp.PtrToStringUTF8(pchName), nScore, flTimePlayed);
        }

        private void InternalOnPlayersFailedToRespond(IntPtr thisptr)
        {
            m_PlayersFailedToRespond();
        }

        private void InternalOnPlayersRefreshComplete(IntPtr thisptr)
        {
            m_PlayersRefreshComplete();
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private class VTable
        {
            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalAddPlayerToList m_VTAddPlayerToList;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalPlayersFailedToRespond m_VTPlayersFailedToRespond;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalPlayersRefreshComplete m_VTPlayersRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingPlayersResponse that)
        {
            return that.m_pGCHandle.AddrOfPinnedObject();
        }
    }

    //-----------------------------------------------------------------------------
    // Purpose: Callback interface for receiving responses after requesting rules
    // details on a particular server.
    //
    // These callbacks all occur in response to querying an individual server
    // via the ISteamMatchmakingServers()->ServerRules() call below.  If you are
    // destructing an object that implements this interface then you should call
    // ISteamMatchmakingServers()->CancelServerQuery() passing in the handle to the query
    // which is in progress.  Failure to cancel in progress queries when destructing
    // a callback handler may result in a crash when a callback later occurs.
    //-----------------------------------------------------------------------------
    public class ISteamMatchmakingRulesResponse
    {
        // Got data on a rule on the server -- you'll get one of these per rule defined on
        // the server you are querying
        public delegate void RulesResponded(string pchRule, string pchValue);

        // The server failed to respond to the request for rule details
        public delegate void RulesFailedToRespond();

        // The server has finished responding to the rule details request
        // (ie, you won't get anymore RulesResponded callbacks)
        public delegate void RulesRefreshComplete();

        private readonly VTable m_VTable;
        private readonly IntPtr m_pVTable;
        private GCHandle m_pGCHandle;
        private readonly RulesResponded m_RulesResponded;
        private readonly RulesFailedToRespond m_RulesFailedToRespond;
        private readonly RulesRefreshComplete m_RulesRefreshComplete;

        public ISteamMatchmakingRulesResponse(RulesResponded onRulesResponded,
            RulesFailedToRespond onRulesFailedToRespond, RulesRefreshComplete onRulesRefreshComplete)
        {
            if (onRulesResponded == null || onRulesFailedToRespond == null || onRulesRefreshComplete == null)
                throw new ArgumentNullException();
            m_RulesResponded = onRulesResponded;
            m_RulesFailedToRespond = onRulesFailedToRespond;
            m_RulesRefreshComplete = onRulesRefreshComplete;

            m_VTable = new VTable
            {
                m_VTRulesResponded = InternalOnRulesResponded,
                m_VTRulesFailedToRespond = InternalOnRulesFailedToRespond,
                m_VTRulesRefreshComplete = InternalOnRulesRefreshComplete
            };
            m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
            Marshal.StructureToPtr(m_VTable, m_pVTable, false);

            m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
        }

        ~ISteamMatchmakingRulesResponse()
        {
            if (m_pVTable != IntPtr.Zero) Marshal.FreeHGlobal(m_pVTable);

            if (m_pGCHandle.IsAllocated) m_pGCHandle.Free();
        }

#if NOTHISPTR
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalRulesResponded(IntPtr pchRule, IntPtr pchValue);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalRulesFailedToRespond();
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void InternalRulesRefreshComplete();
		private void InternalOnRulesResponded(IntPtr pchRule, IntPtr pchValue) {
			m_RulesResponded(InteropHelp.PtrToStringUTF8(pchRule), InteropHelp.PtrToStringUTF8(pchValue));
		}
		private void InternalOnRulesFailedToRespond() {
			m_RulesFailedToRespond();
		}
		private void InternalOnRulesRefreshComplete() {
			m_RulesRefreshComplete();
		}
#else
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalRulesResponded(IntPtr thisptr, IntPtr pchRule, IntPtr pchValue);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalRulesFailedToRespond(IntPtr thisptr);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void InternalRulesRefreshComplete(IntPtr thisptr);

        private void InternalOnRulesResponded(IntPtr thisptr, IntPtr pchRule, IntPtr pchValue)
        {
            m_RulesResponded(InteropHelp.PtrToStringUTF8(pchRule), InteropHelp.PtrToStringUTF8(pchValue));
        }

        private void InternalOnRulesFailedToRespond(IntPtr thisptr)
        {
            m_RulesFailedToRespond();
        }

        private void InternalOnRulesRefreshComplete(IntPtr thisptr)
        {
            m_RulesRefreshComplete();
        }
#endif

        [StructLayout(LayoutKind.Sequential)]
        private class VTable
        {
            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalRulesResponded m_VTRulesResponded;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalRulesFailedToRespond m_VTRulesFailedToRespond;

            [NonSerialized] [MarshalAs(UnmanagedType.FunctionPtr)]
            public InternalRulesRefreshComplete m_VTRulesRefreshComplete;
        }

        public static explicit operator IntPtr(ISteamMatchmakingRulesResponse that)
        {
            return that.m_pGCHandle.AddrOfPinnedObject();
        }
    }
}

#endif // !DISABLESTEAMWORKS