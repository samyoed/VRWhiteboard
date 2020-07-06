using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostItWall : MonoBehaviour
{
    public float thickness = 0.6f; // how much the post it note be from the center of wall for it to "stick"
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PostIt>() != null)
        {
            print("post it note sticking to wall.");
            other.transform.SetParent(transform);
            other.transform.localEulerAngles = new Vector3(270, 0, 0);
            other.transform.localPosition = new Vector3(other.transform.localPosition.x, other.transform.localPosition.y, -0.6f); //-0.6f
            print(other.transform.localPosition);
            other.GetComponent<PostIt>().stuckToWall = true;
        }
    }
}
