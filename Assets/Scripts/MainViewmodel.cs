using Com.MaxAigner.PhotonTest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainViewmodel : MonoBehaviour {

    public Text myText;
    public GameObject ProgressPanel;
    public GameObject ControlPanel;
    public Text TXT_ErrorJoiningRoom;
    public Text TXT_ErrorCreatingRoom;

    Launcher myLauncher;

    // Use this for initialization
    void Start () {
        myLauncher = GetComponent<Launcher>();

        // update roomlist Info when Photon sends updates.
        myLauncher.OnRoomListUpdated += (object sender, RoomListUpdateEventArgs e)=> {
            myText.text = "Rooms:\n" + e.GetRoomListInfo();
        };
        // Ein- und Ausblenden des Ladebildschirms
        myLauncher.OnConnectionProgressChanged += (object sender, ConnectionChangedEventArgs e) =>
        {
            ProgressPanel.SetActive(e.IsConnecting);
            ControlPanel.SetActive(!e.IsConnecting);
        };
        // handle and display errors
        myLauncher.OnError += (object sender, ErrorEventArgs e) => {
            if (e.ErrorType == ErrorType.ErrorJoiningRoom)
            {
                TXT_ErrorJoiningRoom.text = e.errorMsg;
            } else if(e.ErrorType == ErrorType.ErrorCreatingRoom)
            {
                TXT_ErrorCreatingRoom.text = e.errorMsg;
            }
            Debug.LogError(e.errorMsg);
        };
    }

    // Update is called once per frame
    void Update () {
        
    }
}
