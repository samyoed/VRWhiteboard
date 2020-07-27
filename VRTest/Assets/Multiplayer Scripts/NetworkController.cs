using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // connects to photon master servers
    }
    
    public override void OnConnectedToMaster()
    {
        print("We are connected to the " + PhotonNetwork.CloudRegion + " server.");
    }

    void Update()
    {
        
    }
}
