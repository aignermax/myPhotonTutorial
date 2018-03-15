﻿using System.Collections;
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
            PhotonNetwork.autoJoinLobby = false;

            // Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and 
            // all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;
        }
        
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start() {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        #endregion 


        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - If not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            // we check if we are connected or not, we join if we are, else we initiate the connection to the server.
            if (PhotonNetwork.connected)
            {
                // #Critical we need at this point to attempt joining a Random Room. 
                // If it fails, we will get notified in OnPhotonRandomJoinFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            } else
            {
                // Critical, we must first and foremost connect to Photon Online Server
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }
        #endregion

        // Update is called once per frame
        void Update() {

        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 100 }, null);
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
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnectedFromPhoton()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.Log("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called");
            base.OnDisconnectedFromPhoton();
        }
    }
}