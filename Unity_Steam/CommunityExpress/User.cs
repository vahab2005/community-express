﻿/*
 * Community Express SDK
 * http://www.communityexpresssdk.com/
 *
 * Copyright (c) 2011-2014, Zimmdot, LLC
 * All rights reserved.
 *
 * Subject to terms and condition provided in LICENSE.txt
 * Dual licensed under a Commercial Development and LGPL licenses.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace CommunityExpressNS
{
	using HSteamUser = Int32;
	using AppId_t = UInt32;

    /// <summary>
    /// Information about a user
    /// </summary>
	public class User
	{
		[DllImport("CommunityExpressSW")]
		private static extern IntPtr SteamUnityAPI_SteamUser();
        [DllImport("CommunityExpressSW")]
        [return: MarshalAs(UnmanagedType.I1)]
		private static extern Boolean SteamUnityAPI_SteamUser_BLoggedOn(IntPtr user);
		[DllImport("CommunityExpressSW")]
		private static extern HSteamUser SteamUnityAPI_SteamUser_GetHSteamUser(IntPtr user);
		[DllImport("CommunityExpressSW")]
		private static extern UInt64 SteamUnityAPI_SteamUser_GetSteamID(IntPtr user);
		[DllImport("CommunityExpressSW")]
		private static extern Int32 SteamUnityAPI_SteamUser_InitiateGameConnection(IntPtr user,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)] Byte[] authTicket, Int32 authTicketMaxSize, UInt64 serverSteamID,
			UInt32 serverIP, UInt16 serverPort, Boolean isSecure);
		[DllImport("CommunityExpressSW")]
		private static extern void SteamUnityAPI_SteamUser_TerminateGameConnection(IntPtr user, UInt32 serverIP, UInt16 serverPort);
		[DllImport("CommunityExpressSW")]
        private static extern EUserHasLicenseResult SteamUnityAPI_SteamUser_UserHasLicenseForApp(IntPtr user, UInt64 steamID, AppId_t appID);
		[DllImport("CommunityExpressSW")]
        [return: MarshalAs(UnmanagedType.I1)]
		private static extern Boolean SteamUnityAPI_SteamUser_BIsBehindNAT(IntPtr user);
		[DllImport("CommunityExpressSW")]
		private static extern void SteamUnityAPI_SteamUser_AdvertiseGame(IntPtr user, UInt64 gameServerSteamID, UInt32 serverIP, UInt16 port);
		[DllImport("CommunityExpressSW")]
		private static extern IntPtr SteamUnityAPI_GetPersonaNameByID(UInt64 steamID);
        [DllImport("CommunityExpressSW")]
		private static extern Int32 SteamUnityAPI_SteamUser_GetPlayerSteamLevel(IntPtr user);
        [DllImport("CommunityExpressSW")]
		private static extern Int32 SteamUnityAPI_SteamUser_GetGameBadgeLevel(IntPtr user, Int32 nSeries, Boolean bFoil);
        [DllImport("CommunityExpressSW")]
        private static extern void SteamUnityAPI_SteamUser_StartVoiceRecording(IntPtr user);
        [DllImport("CommunityExpressSW")]
        private static extern void SteamUnityAPI_SteamUser_StopVoiceRecording(IntPtr user);
        [DllImport("CommunityExpressSW")]
        private static extern int SteamUnityAPI_SteamUser_GetAvailableVoice(IntPtr user, [Out] UInt32 pcbCompressed, [Out] UInt32 pcbUncompressed, UInt32 nUncompressedVoiceDesiredSampleRate);
        [DllImport("CommunityExpressSW")]
        private static extern int SteamUnityAPI_SteamUser_GetVoice(IntPtr user, bool bWantCompressed, byte[] pDestBuffer, UInt32 cbDestBufferSize, [Out] UInt32 nBytesWritten, bool bWantUncompressed, byte[] pUncompressedDestBuffer, UInt32 cbUncompressedDestBufferSize, [Out] UInt32 nUncompressBytesWritten, UInt32 nUncompressedVoiceDesiredSampleRate);
        [DllImport("CommunityExpressSW")]
        private static extern int SteamUnityAPI_SteamUser_DecompressVoice(IntPtr user, byte[] pCompressed, UInt32 cbCompressed, byte[] pDestBuffer, UInt32 cbDestBufferSize, [Out] UInt32 nBytesWritten, UInt32 nDesiredSampleRate);
        
        
		private IntPtr _user;
		private Friends _friends; // for user avatar
        private UserAuthentication _auth;
		private UInt32 _serverIP = 0;
		private UInt16 _serverPort;

		internal User()
		{
			_user = SteamUnityAPI_SteamUser();
			_friends = new Friends(CommunityExpress.Instance);
            _auth = new UserAuthentication(this);
		}
        /// <summary>
        /// Finalize user
        /// </summary>
		~User()
		{
			OnDisconnect();
		}

        internal IntPtr UserPointer
        {
            get { return _user; }
        }
        /// <summary>
        /// Begins client authentication
        /// </summary>
        /// <param name="authTicket">User's authentication ticket</param>
        /// <param name="steamIDServer">Server's ID</param>
        /// <param name="ipServer">Server IP</param>
        /// <param name="serverPort">Server port</param>
        /// <param name="isSecure">If the server is secure</param>
        /// <returns>true if user is allowed to access</returns>
		public Boolean InitiateClientAuthentication(out Byte[] authTicket, SteamID steamIDServer, IPAddress ipServer, UInt16 serverPort, Boolean isSecure)
		{
			Byte[] serverIPBytes = ipServer.GetAddressBytes();
			_serverIP = (UInt32)serverIPBytes[0] << 24 | (UInt32)serverIPBytes[1] << 16 | (UInt32)serverIPBytes[2] << 8 | (UInt32)serverIPBytes[3];
			_serverPort = serverPort;

            Byte[] internalAuthTicket = new Byte[UserAuthentication.AuthTicketSizeMax];
			Int32 authTicketSize;

            authTicketSize = SteamUnityAPI_SteamUser_InitiateGameConnection(_user, internalAuthTicket, UserAuthentication.AuthTicketSizeMax, steamIDServer.ToUInt64(), _serverIP, _serverPort, isSecure);

			if (authTicketSize > 0)
			{
				authTicket = new Byte[authTicketSize];

				for (Int32 i = 0; i < authTicketSize; i++)
				{
					authTicket[i] = internalAuthTicket[i];
				}

				return true;
			}

			// Failed to generate Auth Ticket
			authTicket = null;
			return false;
		}
        /// <summary>
        /// When the user disconnects from a server
        /// </summary>
		public void OnDisconnect()
		{
			if (_serverIP != 0)
			{
				SteamUnityAPI_SteamUser_TerminateGameConnection(_user, _serverIP, _serverPort);
			}
		}

        public void StartVoiceRecording()
        {
            SteamUnityAPI_SteamUser_StartVoiceRecording(_user);
        }

        public void StopVoiceRecording()
        {
            SteamUnityAPI_SteamUser_StopVoiceRecording(_user);
        }

        public EVoiceResult GetAvailableVoice(out UInt32 cbCompressed, out UInt32 cbUncompressed, UInt32 nUncompressedVoiceDesiredSampleRate)
        {
            UInt32 pcbCompressed = 0;
            UInt32 pcbUncompressed = 0;

            EVoiceResult ret = (EVoiceResult)SteamUnityAPI_SteamUser_GetAvailableVoice(_user, pcbCompressed, pcbUncompressed, nUncompressedVoiceDesiredSampleRate);
            cbCompressed = pcbCompressed;
            cbUncompressed = pcbUncompressed;
            return ret;
        }

        public EVoiceResult GetVoice(bool bWantCompressed, byte[] pDestBuffer, out UInt32 nBytesWritten, bool bWantUncompressed, byte[] pUncompressedDestBuffer, out UInt32 nUncompressBytesWritten, UInt32 nUncompressedVoiceDesiredSampleRate)
        {
            UInt32 pnBytesWritten = 0;
            UInt32 pnUncompressBytesWritten = 0;

            EVoiceResult ret = (EVoiceResult)SteamUnityAPI_SteamUser_GetVoice(_user, bWantCompressed, pDestBuffer, (uint)pDestBuffer.Length, pnBytesWritten, bWantUncompressed, pUncompressedDestBuffer, (uint)pUncompressedDestBuffer.Length, pnUncompressBytesWritten, nUncompressedVoiceDesiredSampleRate);
            nBytesWritten = pnBytesWritten;
            nUncompressBytesWritten = pnUncompressBytesWritten;
            return ret;
        }

        public EVoiceResult DecompressVoice(byte[] pCompressed, UInt32 cbCompressed, byte[] pDestBuffer, UInt32 cbDestBufferSize, out UInt32 nBytesWritten, UInt32 nDesiredSampleRate)
        {
            UInt32 pnBytesWritten = 0;
            EVoiceResult ret = (EVoiceResult)SteamUnityAPI_SteamUser_DecompressVoice(_user, pCompressed, cbCompressed, pDestBuffer, cbDestBufferSize, pnBytesWritten, nDesiredSampleRate);
            nBytesWritten = pnBytesWritten;
            return ret;
        }

        /// <summary>
        /// After receiving a user's authentication data, and passing it to BeginAuthSession, use this function
        /// to determine if the user owns downloadable content specified by the provided AppID.
        /// </summary>
        /// <param name="appID">App ID</param>
        /// <returns>Argument for result</returns>
        public EUserHasLicenseResult UserHasLicenseForApp(AppId_t appID)
		{
			return SteamUnityAPI_SteamUser_UserHasLicenseForApp(_user, SteamID.ToUInt64(), appID);
		}
        /// <summary>
        /// Checks if the user has an app license
        /// </summary>
        /// <param name="steamID">User iD</param>
        /// <param name="appID">App ID</param>
        /// <returns>Argument for result</returns>
        public EUserHasLicenseResult UserHasLicenseForApp(SteamID steamID, AppId_t appID)
		{
			return SteamUnityAPI_SteamUser_UserHasLicenseForApp(_user, steamID.ToUInt64(), appID);
		}

        /// <summary>
        /// Set data to be replicated to friends so that they can join your game
        /// </summary>
        /// <param name="gameServerSteamID">the steamID of the game server, received from the game server by the client</param>
        /// <param name="ip">the IP address of the game server</param>
        /// <param name="port">the port of the game server</param>
		public void AdvertiseGame(SteamID gameServerSteamID, IPAddress ip, UInt16 port)
		{
			Byte[] serverIPBytes = ip.GetAddressBytes();
			UInt32 serverIP = (UInt32)serverIPBytes[0] << 24 | (UInt32)serverIPBytes[1] << 16 | (UInt32)serverIPBytes[2] << 8 | (UInt32)serverIPBytes[3];

			SteamUnityAPI_SteamUser_AdvertiseGame(_user, gameServerSteamID.ToUInt64(), serverIP, port);
		}

        /// <summary>
        /// Trading Card badges data access
        /// if you only have one set of cards, the series will be 1
        /// the user has can have two different badges for a series; the regular (max level 5) and the foil (max level 1)
        /// </summary>
        /// <param name="nSeries">Series of card</param>
        /// <param name="bFoil">If card is foil</param>
        /// <returns>User's badge level</returns>
        public int GetGameBadgeLevel(int nSeries, bool bFoil)
        {
            return SteamUnityAPI_SteamUser_GetGameBadgeLevel(_user, nSeries, bFoil);
        }

        /// <summary>
        /// Gets the large (184x184) avatar of the user
        /// </summary>
        /// <returns>large (184x184) avatar of the user</returns>
		public Image GetLargeAvatar()
		{
			return _friends.GetLargeFriendAvatar(SteamID);
		}

        /// <summary>
        /// Returns true if this users looks like they are behind a NAT device. Only valid once the user has connected to steam 
        /// (i.e a SteamServersConnected_t has been issued) and may not catch all forms of NAT.
        /// </summary>
		public Boolean IsBehindNAT
		{
			get { return SteamUnityAPI_SteamUser_BIsBehindNAT(_user); }
		}

        /// <summary>
        /// Returns true if the Steam client current has a live connection to the Steam servers. 
        /// If false, it means there is no active connection due to either a networking issue on the local machine, or the Steam server is down/busy.
        /// The Steam client will automatically be trying to recreate the connection as often as possible.
        /// </summary>
		public Boolean LoggedOn
		{
			get { return SteamUnityAPI_SteamUser_BLoggedOn(_user); }
		}

        /// <summary>
        /// Returns the HSteamUser this interface represents
        /// this is only used internally by the API, and by a few select interfaces that support multi-user
        /// </summary>
		public HSteamUser HSteamUser
		{
			get { return SteamUnityAPI_SteamUser_GetHSteamUser(_user); }
		}

        /// <summary>
        /// Returns the SteamID of the user
        /// A SteamID is a unique identifier for an account, and used to differentiate users in all parts of the Steamworks API
        /// </summary>
		public SteamID SteamID
		{
			get { return new SteamID(SteamUnityAPI_SteamUser_GetSteamID(_user)); }
		}
        /// <summary>
        /// Name of the user
        /// </summary>
		public String PersonaName
		{
            get { return Marshal.PtrToStringAnsi(SteamUnityAPI_GetPersonaNameByID(SteamID.ToUInt64())); }
		}

        /// <summary>
	    /// The Steam Level of the user, as shown on their profile
        /// </summary>
		public int SteamLevel
		{
			get { return SteamUnityAPI_SteamUser_GetPlayerSteamLevel(_user); }
		}

        /// <summary>
        /// Gets the small (32x32) avatar of the current user
        /// </summary>
		public Image SmallAvatar
		{
			get { return _friends.GetSmallFriendAvatar(SteamID); }
		}

        /// <summary>
        /// Gets the medium (64x64) avatar of the user
        /// </summary>
		public Image MediumAvatar
		{
			get { return _friends.GetMediumFriendAvatar(SteamID); }
		}

        /// <summary>
        /// Gets the large (184x184) avatar of the user
        /// </summary>
		public Image LargeAvatar
		{
			get { return _friends.GetLargeFriendAvatar(SteamID); }
		}
        /// <summary>
        /// The user's authentication status
        /// </summary>
        public UserAuthentication Authentication
        {
            get { return _auth; }
        }
	}
}