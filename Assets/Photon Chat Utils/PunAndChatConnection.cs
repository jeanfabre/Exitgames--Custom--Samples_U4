
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExitGames.Client.Photon.Chat;

/// <summary>
/// Synchronize connection for Pun and Chat
/// </summary>
public class PunAndChatConnection : MonoBehaviour, IPunChatConnection {
	

	public string UserId = "";
	public string NickName ="";

	public bool autoConnect = true;


	public static Action OnConnectedAction { get; set; }
	public static Action OnDisconnectedAction { get; set; }

	private bool IsConnected;

	#region MonoBehaviour Callbacks
	void Start()
	{
		PunChatClientBroker.Instance.Register(this);
		IsConnected = false;
		if (autoConnect)
		{
			Connect ();
		}
	}
	
	#endregion


	public void Connect()
	{
		Debug.Log("Connect");

		PunChatClientBroker.ChatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, "1.0", new ExitGames.Client.Photon.Chat.AuthenticationValues(UserId));


		if (!PhotonNetwork.connected)
		{
			PhotonNetwork.playerName = NickName;
			PhotonNetwork.AuthValues = new AuthenticationValues(){UserId=UserId};
			PhotonNetwork.ConnectUsingSettings("1.0");
		}
	}

	public void disconnect()
	{
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
			Debug.Log("PUN and CHAT connected");

			if (OnConnectedAction!=null) OnConnectedAction();
		}

	}
	
	#region Pun CallBacks

	public virtual void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster() was called by PUN.");
		CheckIfBothConnected();
	}
	
	public virtual void OnJoinedLobby()
	{
		Debug.Log("OnJoinedLobby() was called by PUN.");
		CheckIfBothConnected();
	}

	public virtual void OnDisconnectedFromPhoton()
	{
		Debug.Log("OnDisconnectedFromPhoton() was called by PUN.");
		if (OnDisconnectedAction!=null) OnDisconnectedAction();
	}

	#endregion Pun CallBacks


	#region IPunChatConnection implementation

	public void OnConnected()
	{
		Debug.Log("OnConnected() was called by Chat.");
		
		PunChatClientBroker.ChatClient.SetOnlineStatus(ChatUserStatus.Online);
		
		CheckIfBothConnected();
	}
	
	public void OnDisconnected()
	{
		Debug.Log("OnDisconnected() was called by Chat. cause:"+PunChatClientBroker.ChatClient.DisconnectedCause);
		
		PunChatClientBroker.ChatClient = null;

		if (OnDisconnectedAction!=null) OnDisconnectedAction();
	}


	public void OnChatStateChange (ChatState state)
	{
		Debug.Log("OnChatStateChange() was called by Chat. ChatState:"+state);

	}
	#endregion
}
