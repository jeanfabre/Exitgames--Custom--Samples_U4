using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

[System.Serializable]
public class OnSubmit : UnityEvent<string> {}


public class NamePickUiForm : MonoBehaviour
{



    private const string UserNamePlayerPref = "NamePickUserName";

    public InputField UserNameInput;

	public OnSubmit OnSubmit;

    public void Start()
    {
        string prefsName = PlayerPrefs.GetString(UserNamePlayerPref);
        if (!string.IsNullOrEmpty(prefsName))
        {
            this.UserNameInput.text = prefsName;
        }
    }

    // new UI will fire "EndEdit" event also when loosing focus. So check "enter" key and only then StartChat.
    public void EndEditOnEnter()
    {
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {

            this.Submit();
        }
    }

	public void Submit()
    {
       // ChatGui chatNewComponent = FindObjectOfType<ChatGui>();
        //chatNewComponent.UserName = this.idInput.text.Trim();
		//chatNewComponent.Connect();
        enabled = false;

		PlayerPrefs.SetString(UserNamePlayerPref, UserNameInput.text);

		this.OnSubmit.Invoke(UserNameInput.text);
    }
}