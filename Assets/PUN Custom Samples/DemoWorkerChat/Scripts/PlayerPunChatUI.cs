using UnityEngine;
using UnityEngine.UI;

using System.Collections;



	public class PlayerPunChatUI : MonoBehaviour {

		#region Public Properties

		[Tooltip("Pixel offset from the player target")]
		public Vector3 ScreenOffset = new Vector3(0f,30f,0f);

		[Tooltip("UI Text to display Player's Name")]
		public Text PlayerNameText;

		[Tooltip("UI Slider to display Player's Health")]
		public Text messageText;

		#endregion


		#region Private Properties

		PlayerPunChat _target;

		float _characterControllerHeight = 0f;

		Transform _targetTransform;

		Renderer _targetRenderer;

		Vector3 _targetPosition;

		#endregion

		#region MonoBehaviour Messages
		
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase
		/// </summary>
		void Awake(){

			this.GetComponent<Transform>().SetParent (GameObject.Find("Canvas").GetComponent<Transform>());
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// update the health slider to reflect the Player's health
		/// </summary>
		void Update()
		{
			// Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
			if (_target == null) {
				Destroy(this.gameObject);
				return;
			}


		}

		/// <summary>
		/// MonoBehaviour method called after all Update functions have been called. This is useful to order script execution.
		/// In our case since we are following a moving GameObject, we need to proceed after the player was moved during a particular frame.
		/// </summary>
		void LateUpdate () {

			// Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
			if (_targetRenderer!=null) {
				this.gameObject.SetActive(_targetRenderer.isVisible);
			}
			
			// #Critical
			// Follow the Target GameObject on screen.
			if (_targetTransform!=null)
			{
				_targetPosition = _targetTransform.position;
				_targetPosition.y += _characterControllerHeight/2f;
				
				this.transform.position = Camera.main.WorldToScreenPoint (_targetPosition) + ScreenOffset;
			}

		}
	

		#endregion

		#region Public Methods

		public void SetTarget(PlayerPunChat target){

			if (target == null) {
			Debug.LogError("<Color=Red><b>Missing</b></Color> PlayerPunChat target for PlayerUI.SetTarget.",this);
				return;
			}

			// Cache references for efficiency because we are going to reuse them.
			_target = target;
			_targetTransform = _target.GetComponent<Transform>();
			_targetRenderer = _target.GetComponent<Renderer>();


			CharacterController _characterController = _target.GetComponent<CharacterController> ();

			// Get data from the Player that won't change during the lifetime of this Component
			if (_characterController != null){
				_characterControllerHeight = _characterController.height;
			}

			if (PlayerNameText != null) {
				PlayerNameText.text = _target.photonView.owner.NickName;
			}
		}

		public void SetMessage(string message)
		{
			messageText.text = message;
		}
		#endregion

	}
