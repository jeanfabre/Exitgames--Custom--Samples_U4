using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MenuManager : MonoBehaviour {

	public GameObject PlayerPrefab;

	public PunAndChatConnection Connector;

	public GameObject background;
	public GameObject UserIdForm;
	public InputField UserIdInputField;
	public GameObject ConnectingPanel;
	public GameObject MainChatPanel;
	public InputField ChatMessage;

	public bool debug;
	
	public void Connect(string userId) {

		if (debug)	Debug.Log("MenuManager : Connect");
		background.SetActive(true);
		UserIdForm.SetActive(false);
		ConnectingPanel.SetActive(true);
		MainChatPanel.SetActive(false);

		Connector.UserId = userId;
		Connector.NickName = userId;
		Connector.Connect();
	}

	public void SendChatMessage()
	{
		if (debug)	Debug.Log("MenuManager: SendChatMessage "+ChatMessage.text);

		PunChatClientBroker.ChatClient.PublishMessage(PhotonNetwork.room.Name,ChatMessage.text);
	}


	#region MonoBehavior Callbacks

	// Use this for initialization
	void Start () {
		ConnectingPanel.SetActive(false);
		UserIdForm.SetActive(true);
		MainChatPanel.SetActive(false);
	}

	void OnEnable()
	{
		PunAndChatConnection.OnConnectedAction += OnConnected;
		PunAndChatConnection.OnDisconnectedAction += OnDisconnected;
	}

	void OnDisable()
	{
		PunAndChatConnection.OnConnectedAction -= OnConnected;
		PunAndChatConnection.OnDisconnectedAction -= OnDisconnected;
	}
	
	#endregion


	#region PUN Callbacks

	public void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.CreateRoom(null,new RoomOptions(){PublishUserId=true},TypedLobby.Default);
	}

	public void OnJoinedRoom()
	{
		if (debug)	Debug.Log("MenuManager: OnJoinedRoom : "+PhotonNetwork.room.Name);

		UserIdForm.SetActive(false);
		background.SetActive(false);
		MainChatPanel.SetActive(true);
		
		PunChatClientBroker.ChatClient.Subscribe(new string[]{PhotonNetwork.room.Name});

		PhotonNetwork.Instantiate(this.PlayerPrefab.name, transform.position, Quaternion.identity, 0);
	}

	public void OnLeftRoom()
	{
		if (debug) Debug.Log("MenuManager: OnLeftRoom");
		background.SetActive(true);
		MainChatPanel.SetActive(false);
		UserIdForm.SetActive(true);

		PunChatClientBroker.ChatClient.Unsubscribe(new string[]{PhotonNetwork.room.Name});
	}


	#endregion

	#region PunAndChatConnection Callbacks
	void OnConnected()
	{
		if (debug)	Debug.Log("MenuManager: OnConnected");
		ConnectingPanel.SetActive(false);
		PhotonNetwork.JoinRandomRoom();
	}

	void OnDisconnected()
	{
		
		if (debug)	Debug.Log("MenuManager: OnDisconnected");
		background.SetActive(true);
		ConnectingPanel.SetActive(false);
		UserIdForm.SetActive(true);
	}
	#endregion PunAndChatConnection



}
