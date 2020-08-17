using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkedGrabbing : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    public void OnSelectEnter()
    {
        TransferOwnership();
    }
    public void OnSelectExit()
    {

    }
    void TransferOwnership()
    {
        photonView.RequestOwnership();
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        print("OnOwnership Requested for: " + targetView.name + " from " + requestingPlayer.NickName);
        photonView.TransferOwnership(requestingPlayer);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        print("Transfer is complete. New owner: " + targetView.Owner.NickName);
    }

}
