using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigExt : MonoBehaviour
{
    Whiteboard whiteboard;
    float whiteboardDist;
    public float menuActiveDistance = 30;

    // Start is called before the first frame update
    void Start()
    {   
        whiteboard = GameObject.FindWithTag("Whiteboard").GetComponent<Whiteboard>();
    }

    // Update is called once per frame
    void Update()
    {
        whiteboardDist = Vector3.Distance(transform.position, whiteboard.transform.position);

        if(whiteboardDist < menuActiveDistance )
            PersistentWhiteboardInfo.whiteboardActive = true;
    }
}
