using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby lobby;

    public GameObject findRoomButton;
    public GameObject cancelButton;

    private void Awake()
    {
        lobby = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //Connects to Master photon server;
    }

    public override void OnConnectedToMaster()
    {
        print("Player has connected to Photon master server");
        PhotonNetwork.AutomaticallySyncScene = true;
        findRoomButton.SetActive(true);
    }

    public void OnFindRoomButtonClicked()
    {
        findRoomButton.SetActive(false);
        cancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

   

    //if failed then create a new room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("Tried to join game but failed");
        CreateRoom();

    }

    void CreateRoom()
    {
        print("Creating Room...");
        int RandomRoomName = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 10
        };
        PhotonNetwork.CreateRoom("Room" + RandomRoomName, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("Failed to create new room");
        CreateRoom();
    }

    public void OnCancelButtonClicked()
    {
        cancelButton.SetActive(false);
        findRoomButton.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }
}
