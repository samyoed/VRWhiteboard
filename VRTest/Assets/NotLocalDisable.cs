﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NotLocalDisable : MonoBehaviour
{
    private PhotonView PV;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject cam;
    public GameObject ground;
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

        ground = GameObject.Find("Ground");
        //ground.GetComponent<TeleportationArea>().TeleportationProvider = GetComponent<;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
