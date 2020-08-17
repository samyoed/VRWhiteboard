using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom room;
    private PhotonView pv;

    //public bool isGameLoaded;
    public int currentScene;
    public int multiplayerScene;

    private void Awake()
    {
        if(PhotonRoom.room == null)
        {
            PhotonRoom.room = this;
        }
        else
        {
            if(PhotonRoom.room != this)
            {
                Destroy(PhotonRoom.room.gameObject);
                PhotonRoom.room = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
        pv = GetComponent<PhotonView>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("Joined Room");
        StartGame();
    }
    void StartGame()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.LoadLevel(multiplayerScene);
    }
    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if(currentScene == multiplayerScene)
        {
            CreatePlayer();
        }
    }
    private void CreatePlayer()
    {
        PhotonNetwork.Instantiate("PhotonPrefabs/PhotonNetworkPlayer", transform.position, Quaternion.identity, 0);
        print("Character has spawned");
    }
    
}
