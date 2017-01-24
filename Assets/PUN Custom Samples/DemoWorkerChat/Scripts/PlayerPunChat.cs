using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;

using ExitGames.Client.Photon.Chat;

public class PlayerPunChat : Photon.PunBehaviour, IPunChatUser, IPunChatChannel {

	public GameObject UiPrefab;

	PlayerPunChatUI instance;

	public string UserId;
	public string channelId;

	ChatChannel _channel;

	// Use this for initialization
	void Start () {
	
		GameObject go  = Instantiate(UiPrefab) as GameObject;
		instance = go.GetComponent<PlayerPunChatUI>();
		instance.SetTarget(this);

		UserId  = this.photonView.owner.UserId;
		channelId = PhotonNetwork.room.Name;

		PunChatClientBroker.Instance.Register((IPunChatUser)this);
		PunChatClientBroker.Instance.Register((IPunChatChannel)this);

		PunChatClientBroker.ChatClient.TryGetChannel(PhotonNetwork.room.Name,out _channel);
	}

	#region IPunChatChannel implementation
	void IPunChatChannel.OnSubscribed (bool result)
	{
		//throw new System.NotImplementedException ();
	}
	void IPunChatChannel.OnUnsubscribed ()
	{
		//throw new System.NotImplementedException ();
	}
	void IPunChatChannel.OnGetMessages (string[] senders, object[] messages)
	{
		//throw new System.NotImplementedException ();
		if (senders[senders.Length-1] == UserId)
		{
			instance.SetMessage(
				(string)messages[messages.Length-1])
				;
		}

	}
	string IPunChatChannel.Channel {
		get {
			return channelId;
		}
		set {
			channelId = value;
		}
	}
	#endregion	

	#region IPunChatUser implementation
	void IPunChatUser.OnStatusUpdate (int status, bool gotMessage, object message)
	{
		//throw new System.NotImplementedException ();
	}
	void IPunChatUser.OnGetMessages (string channelName, string[] senders, object[] messages)
	{
	}

	void IPunChatUser.OnPrivateMessage (string sender, object message, string channelName)
	{
		//throw new System.NotImplementedException ();
	}
	void IPunChatUser.OnSubscribed (string[] channels, bool[] results)
	{
		Debug.Log("OnSubscribed for"+UserId+" for channels"+channels.ToStringFull());
		//throw new System.NotImplementedException ();
	}
	void IPunChatUser.OnUnsubscribed (string[] channels)
	{
		//throw new System.NotImplementedException ();
	}
	string IPunChatUser.User {
		get {
			return UserId;
		}
		set {
			UserId = value;
		}
	}
	#endregion
}
