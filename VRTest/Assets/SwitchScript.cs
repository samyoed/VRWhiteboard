using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    Vector3 startPos;
    Quaternion startRot;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(startPos.x, Mathf.Clamp(transform.position.y, 0.25f, 0.75f), startPos.z);
        transform.rotation = startRot;

    }
}
