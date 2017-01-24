// (c) Copyright HutongGames, LLC 2010-2016. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExitGames.Client.Photon.Chat;

public interface IPunChatConnection
{
	void OnConnected();
	void OnDisconnected();
	void OnChatStateChange (ChatState state);
}


public interface IPunChatUser
{
	string User  { get; set; }

	void OnStatusUpdate (int status, bool gotMessage, object message);

	void OnGetMessages (string channelName, string[] senders, object[] messages);

	void OnPrivateMessage (string sender, object message, string channelName);

	void OnSubscribed (string[] channels, bool[] results);
	void OnUnsubscribed (string[] channels);
}

public interface IPunChatChannel
{
	string Channel  { get; set; }

	void OnSubscribed (bool result);
	void OnUnsubscribed ();

	void OnGetMessages (string[] senders, object[] messages);
}

/// <summary>
/// Pun chat client broker.
/// Register your classes to be inform of particular contexts.
/// Interfaces: IPunChatConnection,IPunChatUser,IPunChatChannel
/// </summary>
public class PunChatClientBroker : MonoBehaviour, IChatClientListener {


	public static PunChatClientBroker Instance;

	public static ChatClient ChatClient;
	
	public static ExitGames.Client.Photon.Chat.AuthenticationValues AuthValues;
	
	public bool activeService = true;
	
	public bool debug = false;

	List<IPunChatConnection> PunChatConnectionList = new List<IPunChatConnection>();
	Dictionary<string,List<IPunChatUser>> PunChatUserList = new Dictionary<string, List<IPunChatUser>>();
	Dictionary<string,List<IPunChatChannel>> PunChatChannelList = new Dictionary<string, List<IPunChatChannel>>();
	
	#region Action Delegates
	
	public static Action OnDisconnectedAction { get; set; }
	public static Action OnConnectedAction { get; set; }
	public static Action<ChatState> OnChatStateChangeAction { get; set; }
	public static Action<string[],bool[]> OnSubscribedAction { get; set; }

	#endregion Action Delegates

	void Awake() {
		Instance = this;
		ChatClient =  new ChatClient(this);
	}

	void Update() {
		
		if (PunChatClientBroker.ChatClient != null && activeService)
		{
			PunChatClientBroker.ChatClient.Service();
		}
	}
	
	#region IChatClientListener implementation
	
	public void DebugReturn (ExitGames.Client.Photon.DebugLevel level, string message)
	{
		if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		{
			UnityEngine.Debug.LogError(message);
		}
		else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		{
			UnityEngine.Debug.LogWarning(message);
		}
		else
		{
			UnityEngine.Debug.Log(message);
		}	
	}

	public void OnConnected ()
	{
		if (debug) Debug.Log("OnConnected",this);

		if (OnConnectedAction!=null) OnConnectedAction();
		
		PunChatConnectionList.ForEach(p => p.OnConnected());
	}

	public void OnDisconnected ()
	{
		if(debug) Debug.Log("OnDisconnected",this);

		OnDisconnectedAction();

		PunChatConnectionList.ForEach(p => p.OnDisconnected());
	}
	
	public void OnChatStateChange (ChatState state)
	{
		if (debug) Debug.Log("OnChatStateChange "+state,this);

		if (OnChatStateChangeAction!=null)	OnChatStateChangeAction(state);
		PunChatConnectionList.ForEach(p => p.OnChatStateChange(state));
	}
	
	public void OnGetMessages (string channelName, string[] senders, object[] messages)
	{
		if (debug) Debug.Log("OnGetMessages for "+channelName+" senders "+senders.ToStringFull(),this);

		if (PunChatChannelList.ContainsKey(channelName))
		{
			PunChatChannelList[channelName].ForEach(p => p.OnGetMessages(senders,messages));
		}


	}
	
	public void OnPrivateMessage (string sender, object message, string channelName)
	{
		if (debug) Debug.Log("OnPrivateMessage from "+sender,this);
		
		if (PunChatUserList.ContainsKey(sender))
		{
			PunChatUserList[sender].ForEach(p => p.OnPrivateMessage(sender,message,channelName));
		}
	}
	
	public void OnSubscribed (string[] channels, bool[] results)
	{
		if (debug) Debug.Log("OnSubscribed to "+channels.ToStringFull(),this);


		if (OnSubscribedAction!=null) OnSubscribedAction(channels,results);

		int i=0;
		foreach(string _channel in channels)
		{
			if (PunChatChannelList.ContainsKey(_channel))
			{
				PunChatChannelList[_channel].ForEach(p => p.OnSubscribed(results[i]));
			}
			i++;
		}

	}

	public void OnUnsubscribed (string[] channels)
	{
		if (debug) Debug.Log("OnUnsubscribed from "+channels.ToStringFull(),this);
	}
	
	public void OnStatusUpdate (string user, int status, bool gotMessage, object message)
	{
		if (debug) Debug.Log("OnStatusUpdate for "+user+" status:"+status,this);
	}
	
	#endregion IChatClientListener implementation


	#region Interfaces Registration


	public void Register(IPunChatConnection target)
	{
		if (target==null)
		{
			return;
		}
		
		if (!PunChatConnectionList.Contains(target))
		{
			PunChatConnectionList.Add(target);
		}
	}
	
	public void Unregister(IPunChatConnection target)
	{
		if (target==null)
		{
			return;
		}
		
		if (!PunChatConnectionList.Contains(target))
		{
			PunChatConnectionList.Remove(target);
		}
	}
	
	public void Register(IPunChatUser target)
	{
		if (target==null || string.IsNullOrEmpty(target.User) )
		{
			return;
		}
		
		if (!PunChatUserList.ContainsKey(target.User))
		{
			PunChatUserList[target.User] = new List<IPunChatUser>();
		}
		
		if (!PunChatUserList[target.User].Contains(target))
		{
			PunChatUserList[target.User].Add(target);
		}
	}
	
	public void Unregister(IPunChatUser target)
	{
		if (target==null || string.IsNullOrEmpty(target.User) )
		{
			return;
		}
		
		if (!PunChatUserList.ContainsKey(target.User))
		{
			return;
		}
		
		if (!PunChatUserList[target.User].Contains(target))
		{
			PunChatUserList[target.User].Remove(target);
		}
	}
	
	public void Register(IPunChatChannel target)
	{
		if (target==null || string.IsNullOrEmpty(target.Channel) )
		{
			return;
		}
		
		if (!PunChatChannelList.ContainsKey(target.Channel))
		{
			PunChatChannelList[target.Channel] = new List<IPunChatChannel>();
		}
		
		if (!PunChatChannelList[target.Channel].Contains(target))
		{
			PunChatChannelList[target.Channel].Add(target);
		}
	}
	
	public void Unregister(IPunChatChannel target)
	{
		if (target==null || string.IsNullOrEmpty(target.Channel) )
		{
			return;
		}
		
		if (!PunChatChannelList.ContainsKey(target.Channel))
		{
			return;
		}
		
		if (!PunChatChannelList[target.Channel].Contains(target))
		{
			PunChatChannelList[target.Channel].Remove(target);
		}
	}
	
	#endregion Interfaces Registration

}
