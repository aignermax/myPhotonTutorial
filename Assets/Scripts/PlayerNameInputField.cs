using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour {

    #region private variables 

    // store the playerpref Key to avoid typos
    static string playerNamePrefKey = "PlayerName";

    #endregion

    #region MonBehaviour Callbacks

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase
    /// </summary>
    void Start () {
        string defaultName = "";
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.playerName = defaultName;
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    #endregion

    #region Public Methods

    public void SetPlayerName(string value)
    {
        // #Important
        PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
    #endregion
}
