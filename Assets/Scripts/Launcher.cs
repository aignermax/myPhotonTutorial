using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MaxAigner.PhotonTest
{
    // this is part of the Photon Tutorial:
    // https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/lobby
    public class Launcher : Photon.PunBehaviour {

        #region Public Variables
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        public GameObject progressLabel;
        public delegate void ConnectionChangedEventHandler(object sender, ConnectionChangedEventArgs e);
        public delegate void RoomListUpdateEventHandler(object sender, RoomListUpdateEventArgs e);
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public event ConnectionChangedEventHandler OnConnectionProgressChanged;
        public event RoomListUpdateEventHandler OnRoomListUpdated;
        public event ErrorEventHandler OnError;

        /// <summary>
        /// The LogLevel of the whole PUN
        /// </summary>
        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>   
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so a new room will be created")]
        public byte MaxPlayersPerRoom = 4;

        #endregion

        #region Private Variables

        /// <summary>
        /// This client's version number. Users are separated from each other by gameversion (which allows us to make breaking changes)
        /// </summary>
        string _gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back ba Photon
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;
        /// <summary>
        /// keeps track if we want to create a room or just join one. 
        /// </summary>
        bool isCreatingRoom;
        /// <summary>
        /// roomname to join or create -> has to be set in the Menu normally 
        /// either by typing the name or by selecting from a roomlist from the lobby
        /// </summary>
        string JoinCreateRoomName = "";

        #endregion


        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        private void Awake()
        {

            PhotonNetwork.logLevel = Loglevel;
            // Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms
            PhotonNetwork.autoJoinLobby = true ;
            

            // Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and 
            // all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;
        }
        
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start() {
            SwitchProgressLabelVisibility(false);
            // connect network to join lobby and get Info about available Rooms
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }

        #endregion 


        #region Public Methods
        public void BTNCreate()
        {
            Connect(true , "test");
        }
        public void BTNJoin()
        {
            Connect(false, "test");
        }
        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - If not yet connected, Connect this application instance to Photon Cloud Network
        /// will be called by a button
        /// </summary>
        void Connect(bool isCreateRoom, string RoomName)
        {
            isCreatingRoom = isCreateRoom;
            JoinCreateRoomName = RoomName;
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;
            // notify everyone that connection has changed
            SwitchProgressLabelVisibility(true);

            // we check if we are connected or not, we join if we are, else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. 
                // If it fails, we will get notified in OnPhotonRandomJoinFailed() and we'll create one.
                JoinCreateRoom(RoomName, isCreateRoom);
            } else
            {
                // Critical, we must first and foremost connect to Photon Online Server
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }
        /// <summary>
        ///  joins or creates a room
        /// </summary>
        /// <param name="RoomName"></param>
        /// <param name="IsCreate"></param>
        void JoinCreateRoom(string RoomName, bool IsCreate)
        {
            if (IsCreate)
            {
                PhotonNetwork.CreateRoom(RoomName, new RoomOptions() { MaxPlayers = 100 }, null);
            }
            else
            {
                PhotonNetwork.JoinRoom(RoomName);
            }
        }
        
        #endregion
        
        /// <summary>
        /// displays or hides the progressbar visibility
        /// </summary>
        /// <param name="isProgressVisible"></param>
        void SwitchProgressLabelVisibility(bool isProgressVisible)
        {
            if (OnConnectionProgressChanged != null)
            {
                OnConnectionProgressChanged.Invoke(this, new ConnectionChangedEventArgs(false));
            }
        }

        // Update is called once per frame
        void Update() {

        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            if (OnError != null)
            {
                OnError.Invoke(this, new ErrorEventArgs(ErrorType.ErrorJoiningRoom , "Error Joining Room. "  + codeAndMsg[1]));
            }
            SwitchProgressLabelVisibility(false);
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonCreateRoomFailed(codeAndMsg);
            if (OnError != null)
            {
                OnError.Invoke(this, new ErrorEventArgs(ErrorType.ErrorJoiningRoom, "Error Creating Room. "  + codeAndMsg[1]));
            }
            SwitchProgressLabelVisibility(false);
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            // #Critical: we only load if we are the first player , else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene

            if (PhotonNetwork.room.PlayerCount== 1)
            {
                Debug.Log("We load the 'Room for 1'");
                // #Critical
                // Load the Room Level.
                PhotonNetwork.LoadLevel("Room for 1");
            }
        }

        public override void OnReceivedRoomListUpdate()
        {
            base.OnReceivedRoomListUpdate();
            // roomlist has been updated
            if (OnRoomListUpdated != null)
            {
                OnRoomListUpdated.Invoke(this, new RoomListUpdateEventArgs(PhotonNetwork.GetRoomList()));
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called");
            base.OnConnectedToMaster();
            // we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()  
                JoinCreateRoom(JoinCreateRoomName, isCreatingRoom);
            }
        }

        public override void OnDisconnectedFromPhoton()
        {
            Debug.Log("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called");
            base.OnDisconnectedFromPhoton();
            SwitchProgressLabelVisibility(false);
        }
    }

    public class ConnectionChangedEventArgs
    {
        public ConnectionChangedEventArgs(bool isConnecting)
        {
            this.IsConnecting = isConnecting;
        }
        /// <summary>
        /// if true, the connecting process has started -> if false, hide the connecting  screen and show the control panel
        /// </summary>
        public bool IsConnecting;
    }

    public class ErrorEventArgs
    {
        public ErrorEventArgs(ErrorType errorType , string error)
        {
            this.ErrorType = errorType;
            this.errorMsg = error;
        }

        public ErrorType ErrorType;
        public string errorMsg;
    }

    public enum ErrorType
    {
        ErrorCreatingRoom,
        ErrorJoiningRoom
    }

    public class RoomListUpdateEventArgs
    {
        public RoomListUpdateEventArgs(RoomInfo[] roomList)
        {
            this.RoomList = roomList;
        }
        /// <summary>
        /// get one string with RoomName (Players \\ MaxPlayers) \\n
        /// </summary>
        /// <param name="myRoomList"></param>
        /// <returns></returns>
        public string GetRoomListInfo()
        {
            string roomlistdebug = "";
            foreach (RoomInfo i in RoomList)
            {
                roomlistdebug += i.Name + " (" + i.PlayerCount + " \\ " + i.MaxPlayers + ")\n";
            }
            return roomlistdebug;
        }
        /// <summary>
        /// list of all rooms in the game. will be updated in a fixed interval controlled by Photon
        /// </summary>
        public RoomInfo[] RoomList;
    }

}