using System.Collections.Generic;
using ExitGames.Client.Photon;
using Microsoft.MixedReality.Toolkit.UX;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;
using DeviceType = Utils.DeviceType;

namespace Manager
{
    /// <summary>
    /// Connection script for the phone app (DO NOT PUSH)
    /// </summary>
    public class PhoneLauncher : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMPro.TMP_InputField usernameInput;
        [SerializeField] private TMPro.TMP_Text       waitingText;
        [SerializeField] private UGUIInputAdapter     waitingButton;

        [FormerlySerializedAs("_menuSystem")] [SerializeField]
        private MenuSystem menuSystem;

        private const string GameVersion = "1";
        private       bool   _connecting;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a lobby
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            if (usernameInput.text.Length == 0)
                return;

            PhotonNetwork.NickName = usernameInput.text;

            _connecting = true;

            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = GameVersion;

                // _menuSystem.PrintError("Connection failed/lost");
            }

            PhotonNetwork.SetPlayerCustomProperties(new Hashtable { { "Device", DeviceType.AR } });

            menuSystem.SwitchPanel(MenuSystem.MenuIndex.Joining);
        }

        public void Cancel()
        {
            CancelConnection();

            menuSystem.SwitchPanel(MenuSystem.MenuIndex.Join);
        }

        private void CancelConnection()
        {
            Debug.Log("Launcher: CancelCollection was called");

            _connecting = false;

            if (PhotonNetwork.InRoom)
                PhotonNetwork.LeaveRoom();
            if (PhotonNetwork.InLobby)
                PhotonNetwork.LeaveLobby();
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();

            waitingText.text = "Waiting for host to connect";
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Launcher: OnConnectedToMaster() was called by PUN");
            PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("PUN Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
            menuSystem.Reset();
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Debug.Log("PUN Launcher: OnJoinedLobby() called by PUN. Now this client is in a lobby.");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            _connecting = false;
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            Debug.Log("PUN Launcher: OnLeftLobby() called by PUN.");
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("PUN Launcher: OnLeftRoom() called by PUN.");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("PUN Launcher: OnRoomListUpdate() was called by PUN");

            if (roomList.Count == 0)
            {
                waitingText.text = "Waiting for host to connect";

                Debug.Log("There is no room available");
            }
            else
            {
                waitingText.text           = "Ready to join host";
                waitingButton.interactable = true;

                if (!_connecting)
                    return;

                Debug.Log("Joining a random room");

                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log("PUN Launcher: OnMasterClientSwitched() was called by PUN");

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen       = false;
                PhotonNetwork.CurrentRoom.EmptyRoomTtl = 0;
                PhotonNetwork.CurrentRoom.PlayerTtl    = 0;
            }

            CancelConnection();

            menuSystem.SwitchPanel(MenuSystem.MenuIndex.Disconnected);
        }
    }
}