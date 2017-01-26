
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExitGames.Client.Photon.Chat;

/// <summary>
/// Leverage connection for Pun and Chat for them to work in tandem.
/// 
/// Dependancy: PunChatClientBroker. It's likely a good idea to have both components on One GameObject for clarity.
/// 
/// The Premise is that Chat UserId is the same as Pun UserId (coming from your Online Social network system for your gamer's community during authentication).
/// 
/// Call Connect() and Disconnect() to control both PUN and Chat Connections.
/// 
/// Register to OnConnectedAction() and OnDisconnectedAction() Actions to receive callbacks on connection status for Pun+Chat Tandem. 
/// 
/// TODO: more options for connecting, and external authentication setup
/// </summary>
public class PunAndChatConnection : MonoBehaviour, IPunChatConnection {
	
	/// <summary>
	/// The game version. Use both for PUN and CHAT 
	/// </summary>
	public string GameVersion = "1.0";

	/// <summary>
	/// The user identifier. Used for both PUN and CHAT
	/// 
	/// Set this value prior calling Connect() method
	/// </summary>
	public string UserId = "";

	/// <summary>
	/// The NickName. This is a PUN only feature. Leave to empty if you don't want this script to use it.
	/// </summary>
	public string NickName = "";

	/// <summary>
	/// If true, output logs on the various states of PUN and Chat connection.
	/// </summary>
	public bool debug = false;

	/// <summary>
	/// Callback when PUN and CHAT are both connected. Use Connect() method to initiate connections for PUN and CHAT
	/// </summary>
	public static Action OnConnectedAction { get; set; }

	/// <summary>
	/// Callback when either PUN or Chat disconnected. Use Disconnect() method to disconnect both PUN and CHAT
	/// </summary>
	public static Action OnDisconnectedAction { get; set; }

	private bool IsConnected;

	#region MonoBehaviour Callbacks

	void Start()
	{
		PunChatClientBroker.Register(this);
		IsConnected = false;
	}
	
	#endregion

	/// <summary>
	/// Connect to PUN and CHAT
	/// UserId is expected to be set prior calling
	/// 
	/// Register to OnConnectedAction delegate to be informed when connection is effective for both PUN and CHAT
	/// </summary>
	public void Connect()
	{
		if (debug) Debug.Log("PunAndChatConnection: Connect() as <"+UserId+">");

		PunChatClientBroker.ChatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, GameVersion, new ExitGames.Client.Photon.Chat.AuthenticationValues(UserId));

		if (!PhotonNetwork.connected)
		{
			if (!string.IsNullOrEmpty(NickName)) PhotonNetwork.playerName = NickName;

			PhotonNetwork.AuthValues = new AuthenticationValues(){UserId=UserId};

			PhotonNetwork.ConnectUsingSettings(GameVersion);
		}
	}

	/// <summary>
	/// Disconnect from PUN and CHAT
	/// 
	/// Register to OnDisconnectedAction delegate to be informed when disconnection occured with either PUN or CHAT
	/// </summary>
	public void disconnect()
	{
		if (debug) Debug.Log("PunAndChatConnection: disconnect()");

		IsConnected = false;

		if (PunChatClientBroker.ChatClient !=null && PunChatClientBroker.ChatClient.CanChat)
		{
			PunChatClientBroker.ChatClient.Disconnect();
		}

		if (PhotonNetwork.connected)
		{
			PhotonNetwork.Disconnect();
		}
	}


	void CheckIfBothConnected()
	{
		if (IsConnected)
		{
			return ;
		}

		if (PunChatClientBroker.ChatClient == null){
			return;
		}

		IsConnected = PhotonNetwork.connected && PunChatClientBroker.ChatClient.CanChat;

		if (IsConnected)
		{
			if (debug) Debug.Log("PunAndChatConnection: PUN and CHAT connected");

			if (OnConnectedAction!=null) OnConnectedAction();
		}

	}
	
	#region Pun CallBacks

	public virtual void OnConnectedToMaster()
	{
		if (debug) Debug.Log("PunAndChatConnection: OnConnectedToMaster() was called by PUN.");
		CheckIfBothConnected();
	}
	
	public virtual void OnJoinedLobby()
	{
		if (debug) Debug.Log("PunAndChatConnection:OnJoinedLobby() was called by PUN.");
		CheckIfBothConnected();
	}

	public virtual void OnDisconnectedFromPhoton()
	{
		if (debug) Debug.Log("PunAndChatConnection: OnDisconnectedFromPhoton() was called by PUN.");
		if (OnDisconnectedAction!=null) OnDisconnectedAction();
	}

	#endregion Pun CallBacks


	#region IPunChatConnection implementation

	public void OnConnected()
	{
		if (debug) Debug.Log("PunAndChatConnection: OnConnected() was called by Chat.");
		
		PunChatClientBroker.ChatClient.SetOnlineStatus(ChatUserStatus.Online);
		
		CheckIfBothConnected();
	}
	
	public void OnDisconnected()
	{
		if (debug) Debug.Log("PunAndChatConnection: OnDisconnected() was called by Chat. cause:"+PunChatClientBroker.ChatClient.DisconnectedCause);
		
		PunChatClientBroker.ChatClient = null;

		if (OnDisconnectedAction!=null) OnDisconnectedAction();
	}


	public void OnChatStateChange (ChatState state)
	{
		if (debug) Debug.Log("PunAndChatConnection: OnChatStateChange() was called by Chat. ChatState:"+state);
	}
	#endregion
}
