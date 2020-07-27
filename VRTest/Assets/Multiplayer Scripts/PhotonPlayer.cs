using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;
    public GameObject myAvatar;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        if(PV.IsMine)
            myAvatar = PhotonNetwork.Instantiate("PhotonPrefabs/PhotonNetworkPlayer", 
                                      GameSetup.GS.spawnPoints[0].position,
                                      GameSetup.GS.spawnPoints[1].rotation, 0); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
