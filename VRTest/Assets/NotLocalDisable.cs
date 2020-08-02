using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotLocalDisable : MonoBehaviour
{
    private PhotonView PV;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject cam;
    // Start is called before the first frame update
    void Start()
    {
        
        PV = GetComponent<PhotonView>();
            if(!PV.IsMine)
            {
                leftHand.SetActive(false);
                rightHand.SetActive(false);
                cam.SetActive(false);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
