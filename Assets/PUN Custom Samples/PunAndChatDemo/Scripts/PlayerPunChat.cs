using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;

using ExitGames.Client.Photon.Chat;

public class PlayerPunChat : Photon.PunBehaviour, IPunChatChannel {

	public GameObject UiPrefab;

	PlayerPunChatUI instance;

	public string UserId;
	public string channelId;
	
	// Use this for initialization
	void Start () {
	
		GameObject go  = Instantiate(UiPrefab) as GameObject;
		instance = go.GetComponent<PlayerPunChatUI>();
		instance.SetTarget(this);

		UserId  = this.photonView.owner.UserId;
		channelId = PhotonNetwork.room.Name;

		PunChatClientBroker.Register((IPunChatChannel)this);
	}

	#region IPunChatChannel implementation
	void IPunChatChannel.OnSubscribed (bool result){}

	void IPunChatChannel.OnUnsubscribed (){}
	
	void IPunChatChannel.OnGetMessages (string[] senders, object[] messages)
	{
		Debug.Log("Player <"+UserId+"> getting messages from "+senders.ToStringFull());

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

}
