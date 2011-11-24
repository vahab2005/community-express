﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace SteamworksUnityHost
{
	//-----------------------------------------------------------------------------
	// Purpose: list of states a friend can be in
	//-----------------------------------------------------------------------------
	public enum EPersonaState
	{
		EPersonaStateOffline = 0,		// friend is not currently logged on
		EPersonaStateOnline = 1,		// friend is logged on
		EPersonaStateBusy = 2,			// user is on, but busy
		EPersonaStateAway = 3,			// auto-away feature
		EPersonaStateSnooze = 4			// auto-away for a long time
	};

	public class Friends : ICollection<Friend>
	{
		[DllImport("SteamworksUnity.dll")]
		private static extern IntPtr SteamUnityAPI_SteamFriends();
		[DllImport("SteamworksUnity.dll")]
		private static extern int SteamUnityAPI_SteamFriends_GetFriendCount(IntPtr friend);
		[DllImport("SteamworksUnity.dll")]
		private static extern UInt64 SteamUnityAPI_SteamFriends_GetFriendByIndex(IntPtr friend, int iFriend);
		[DllImport("SteamworksUnity.dll")]
		private static extern IntPtr SteamUnityAPI_SteamFriends_GetFriendPersonaName(IntPtr friend, UInt64 steamIDFriend);
		[DllImport("SteamworksUnity.dll")]
		private static extern int SteamUnityAPI_SteamFriends_GetFriendPersonaState(IntPtr friend, UInt64 steamIDFriend);

		private IntPtr _friends;

		private class FriendEnumator : IEnumerator<Friend>
		{
			private int _index;
			private Friends _friends;

			public FriendEnumator(Friends friends)
			{
				_friends = friends;
				_index = -1;
			}

			public Friend Current 
			{
				get
				{
					SteamID id = _friends.GetFriendByIndex(_index);
					return new Friend(_friends, id);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public bool MoveNext()
			{
				_index++;
				return _index < _friends.Count;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose()
			{
			}
		}

		internal Friends()
		{
			_friends = SteamUnityAPI_SteamFriends();
		}

		private SteamID GetFriendByIndex(int iFriend)
		{
			return new SteamID(SteamUnityAPI_SteamFriends_GetFriendByIndex(_friends, iFriend));
		}

		internal String GetFriendPersonaName(SteamID steamIDFriend)
		{
			IntPtr personaName = SteamUnityAPI_SteamFriends_GetFriendPersonaName(_friends, steamIDFriend.ToUInt64());
			return Marshal.PtrToStringAnsi(personaName);
		}

		internal EPersonaState GetFriendPersonaState(SteamID steamIDFriend)
		{
			int personaState = SteamUnityAPI_SteamFriends_GetFriendPersonaState(_friends, steamIDFriend.ToUInt64());
			return (EPersonaState)personaState;
		}

		public int Count
		{
			get { return SteamUnityAPI_SteamFriends_GetFriendCount(_friends); }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public void Add(Friend item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(Friend item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Friend[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(Friend item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<Friend> GetEnumerator()
		{
			return new FriendEnumator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
