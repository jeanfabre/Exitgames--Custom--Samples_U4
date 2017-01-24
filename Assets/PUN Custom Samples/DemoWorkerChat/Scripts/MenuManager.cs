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


	public void Connect() {
		background.SetActive(true);
		UserIdForm.SetActive(false);
		ConnectingPanel.SetActive(true);
		MainChatPanel.SetActive(false);

		Connector.UserId = UserIdInputField.text;
		PhotonNetwork.playerName = Connector.UserId;
		Connector.Connect();
	}

	public void SendChatMessage()
	{
		Debug.Log("SendChatMessage "+ChatMessage.text);
		PunChatClientBroker.ChatClient.PublishMessage(PhotonNetwork.room.Name,ChatMessage.text);
	}


	#region MonoBehavior Callbacks

	// Use this for initialization
	void Start () {
		ConnectingPanel.SetActive(false);
		UserIdForm.SetActive(true);
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
		PhotonNetwork.CreateRoom(null);
	}

	public void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom : "+PhotonNetwork.room.Name);

		background.SetActive(false);
		MainChatPanel.SetActive(true);
		
		PunChatClientBroker.ChatClient.Subscribe(new string[]{PhotonNetwork.room.Name});

		PhotonNetwork.Instantiate(this.PlayerPrefab.name, transform.position, Quaternion.identity, 0);
	}

	public void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom");
		background.SetActive(true);
		MainChatPanel.SetActive(false);
		PunChatClientBroker.ChatClient.Unsubscribe(new string[]{PhotonNetwork.room.Name});
	}


	#endregion

	#region PunAndChatConnection Callbacks
	void OnConnected()
	{
		ConnectingPanel.SetActive(false);

		PhotonNetwork.JoinRandomRoom();
	}

	void OnDisconnected()
	{
		background.SetActive(true);
		ConnectingPanel.SetActive(false);
		UserIdForm.SetActive(true);
	}
	#endregion PunAndChatConnection



}
