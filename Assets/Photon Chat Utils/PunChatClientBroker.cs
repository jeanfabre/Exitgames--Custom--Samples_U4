
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExitGames.Client.Photon.Chat;

/// <summary>
/// Implement this interface to receive connection information from ChatClient.
/// It's mandatory to register/Unregister your interfaces instance PunChatClientBroker.instance.Register() and PunChatClientBroker.instance.Unregister()
/// </summary>
public interface IPunChatConnection
{
	/// <summary>
	/// Called when ChatClient is connected
	/// </summary>
	void OnConnected();

	/// <summary>
	/// Called when ChatClient is disconnected
	/// </summary>
	void OnDisconnected();

	/// <summary>
	/// Called when ChatClient state changes
	/// </summary>
	/// <param name="state">State.</param>
	void OnChatStateChange (ChatState state);
}

/// <summary>
/// Implement this interface to receive User centric informations from the ChatClient.
/// It's mandatory to register/Unregister your interfaces instance PunChatClientBroker.instance.Register() and PunChatClientBroker.instance.Unregister()
/// </summary>
public interface IPunChatUser
{
	/// <summary>
	/// The user targeted by this Interface Instance.
	/// Set this prior registering to PunChatClientBroker()
	/// </summary>
	/// <value>The user.</value>
	string User  { get; set; }

	/// <summary>
	/// Status Update for this User.
	/// </summary>
	/// <param name="status">Status.</param>
	/// <param name="gotMessage">If set to <c>true</c> got message.</param>
	/// <param name="message">Message.</param>
	void OnStatusUpdate (int status, bool gotMessage, object message);
	
	/// <summary>
	/// messages received for this User
	/// </summary>
	/// <param name="channel">Channel.</param>
	/// <param name="senders">Senders.</param>
	/// <param name="messages">Messages.</param>
	void OnGetMessages (string channel,string[] senders, object[] messages);

	/// <summary>
	/// Private Messages for this User
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="message">Message.</param>
	/// <param name="channelName">Channel name.</param>
	void OnPrivateMessage (string sender, object message, string channelName);	
}

/// <summary>
/// Implement this interface to receive Channel centric informations from the ChatClient.
/// It's mandatory to register/Unregister your interfaces instance PunChatClientBroker.instance.Register() and PunChatClientBroker.instance.Unregister()
/// </summary>
public interface IPunChatChannel
{
	/// <summary>
	/// The Channel targeted by this Interface Instance.
	/// </summary>
	/// <value>The channel.</value>
	string Channel  { get; set; }

	/// <summary>
	/// Subscription for this Channel by the local User
	/// </summary>
	/// <param name="result">If set to <c>true</c> result.</param>
	void OnSubscribed (bool result);

	/// <summary>
	/// Unsubscription for this channel by the local User
	/// </summary>
	void OnUnsubscribed ();

	/// <summary>
	/// Messages received for this Channel
	/// </summary>
	/// <param name="senders">Senders.</param>
	/// <param name="messages">Messages.</param>
	void OnGetMessages (string[] senders, object[] messages);
}

/// <summary>
/// Pun chat client broker.
/// Register your classes to be inform of particular contexts.
/// Interfaces: IPunChatConnection,IPunChatUser,IPunChatChannel
/// </summary>
public class PunChatClientBroker : MonoBehaviour, IChatClientListener {

	/// <summary>
	/// Singleton
	/// </summary>
	public static PunChatClientBroker Instance;

	/// <summary>
	/// The chat client.
	/// When using the PunChatClientBroker, this is the property to use to access the ChatClient.
	/// </summary>
	public static ChatClient ChatClient;

	/// <summary>
	/// The auth values.
	/// </summary>
	public static ExitGames.Client.Photon.Chat.AuthenticationValues AuthValues;

	/// <summary>
	/// Define if service needs to be called every Update or not.
	/// </summary>
	public bool activeService = true;

	/// <summary>
	/// Output logs to the Unity Console.
	/// </summary>
	public bool debug = false;


	static List<IPunChatConnection> PunChatConnectionList = new List<IPunChatConnection>();
	static Dictionary<string,List<IPunChatUser>> PunChatUserList = new Dictionary<string, List<IPunChatUser>>();
	static Dictionary<string,List<IPunChatChannel>> PunChatChannelList = new Dictionary<string, List<IPunChatChannel>>();
	
	#region Action Delegates

	/// <summary>
	/// Callback for Disconnection event
	/// </summary>
	public static Action OnDisconnectedAction { get; set; }

	/// <summary>
	/// Callback for connected event.
	/// </summary>
	public static Action OnConnectedAction { get; set; }

	/// <summary>
	/// Callback when chat state changed
	/// </summary>
	public static Action<ChatState> OnChatStateChangeAction { get; set; }

	/// <summary>
	/// Callback when the local Player subscribed to channel(s)
	/// </summary>
	public static Action<string[],bool[]> OnSubscribedAction { get; set; }

	/// <summary>
	/// Callback when the local Player unsubscribed from channel(s)
	/// </summary>
	public static Action<string[]> OnUnsubscribedAction { get; set; }

	/// <summary>
	/// Callback when a User status changed
	/// </summary>
	public static Action<string,int,bool,object> OnStatusUpdateAction { get; set; }

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
			if (debug) UnityEngine.Debug.Log(message);
		}	
	}

	public void OnConnected ()
	{
		if (debug) Debug.Log("PunChatClientBroker: OnConnected",this);

		if (OnConnectedAction!=null) OnConnectedAction();
		
		PunChatConnectionList.ForEach(p => p.OnConnected());
	}

	public void OnDisconnected ()
	{
		if(debug) Debug.Log("PunChatClientBroker: OnDisconnected",this);

		OnDisconnectedAction();

		PunChatConnectionList.ForEach(p => p.OnDisconnected());
	}
	
	public void OnChatStateChange (ChatState state)
	{
		if (debug) Debug.Log("PunChatClientBroker: OnChatStateChange "+state,this);

		if (OnChatStateChangeAction!=null)	OnChatStateChangeAction(state);

		PunChatConnectionList.ForEach(p => p.OnChatStateChange(state));
	}
	
	public void OnGetMessages (string channelName, string[] senders, object[] messages)
	{
		if (debug) Debug.Log("PunChatClientBroker: OnGetMessages for "+channelName+" senders "+senders.ToStringFull(),this);

		if (PunChatChannelList.ContainsKey(channelName))
		{
			PunChatChannelList[channelName].ForEach(p => p.OnGetMessages(senders,messages));
		}

		foreach(var _ui in PunChatUserList)
		{
			_ui.Value.ForEach(p => p.OnGetMessages(channelName,senders,messages));
		}

	}
	
	public void OnPrivateMessage (string sender, object message, string channelName)
	{
		if (debug) Debug.Log("PunChatClientBroker: OnPrivateMessage from "+sender,this);
		
		if (PunChatUserList.ContainsKey(sender))
		{
			PunChatUserList[sender].ForEach(p => p.OnPrivateMessage(sender,message,channelName));
		}
	}
	
	public void OnSubscribed (string[] channels, bool[] results)
	{
		if (debug) Debug.Log("PunChatClientBroker: OnSubscribed to "+channels.ToStringFull(),this);


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
		if (debug) Debug.Log("PunChatClientBroker: OnUnsubscribed from "+channels.ToStringFull(),this);

		if (OnUnsubscribedAction!=null) OnUnsubscribedAction(channels);
		
		int i=0;
		foreach(string _channel in channels)
		{
			if (PunChatChannelList.ContainsKey(_channel))
			{
				PunChatChannelList[_channel].ForEach(p => p.OnUnsubscribed());
			}
			i++;
		}
	}
	
	public void OnStatusUpdate (string user, int status, bool gotMessage, object message)
	{
		if (debug) Debug.Log("PunChatClientBroker: OnStatusUpdate for "+user+" status:"+status,this);

		if (OnStatusUpdateAction!=null) OnStatusUpdateAction(user,status,gotMessage,message);

		if (PunChatUserList.ContainsKey(user))
		{
			PunChatUserList[user].ForEach(p => p.OnStatusUpdate(status,gotMessage,message));
		}
	}
	
	#endregion IChatClientListener implementation


	#region Interfaces Registration


	static public void Register(IPunChatConnection target)
	{
		if (target==null)
		{
			return;
		}
		
		if (!PunChatConnectionList.Contains(target))
		{
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: Register Connection ");
			PunChatConnectionList.Add(target);
		}
	}
	
	static public void Unregister(IPunChatConnection target)
	{
		if (target==null)
		{
			return;
		}
		
		if (!PunChatConnectionList.Contains(target))
		{
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: Unregister Connection ");
			PunChatConnectionList.Remove(target);
		}
	}
	
	static public void Register(IPunChatUser target)
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
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: register User "+target.User);
			PunChatUserList[target.User].Add(target);
		}
	}
	
	static public void Unregister(IPunChatUser target)
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
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: Unregister User "+target.User);
			PunChatUserList[target.User].Remove(target);
		}
	}
	
	static public void Register(IPunChatChannel target)
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
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: Register channel "+target.Channel);
			PunChatChannelList[target.Channel].Add(target);
		}
	}
	
	static public void Unregister(IPunChatChannel target)
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
			if (Instance!=null &&  Instance.debug) Debug.Log("PunChatClientBroker: Unregister channel "+target.Channel);
			PunChatChannelList[target.Channel].Remove(target);
		}
	}
	
	#endregion Interfaces Registration

}
