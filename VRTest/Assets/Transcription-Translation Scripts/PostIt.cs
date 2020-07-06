using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;


public class PostIt : MonoBehaviour
{
    public TextMeshPro content;
    public Transform rightAttach;
    public XRController right, left;
    public bool selected = false;
    public bool stuckToWall = false;
    //public float distance = 0;
    public float sensitivity = 1, wallSensitivity = 1, scaleSensitivity;
    public string pathToSave = "";
    public SpeechToText speechToText;
    public GameObject floatingControls, stuckToWallControls;

    // Start is called before the first frame update
    void Start()
    {
        
        /*right = GameObject.Find("RightHand Controller").GetComponent<XRController>();
        left = GameObject.Find("LeftHand Controller").GetComponent<XRController>();*/
    }

    public void Edit()
    {
        speechToText.beginPostItTranscription();
        speechToText.written = content.text;
        speechToText.toDisplay = content.text;
        speechToText.cursorLocation = content.text.Length;
        Destroy(gameObject);
    }

    public void Select()
    {
        if(selected)
        {
            return;
        }
        speechToText.endButton.SetActive(false);
        speechToText.beginButton.SetActive(false);
        float distance = Vector3.Distance(right.gameObject.transform.position, gameObject.transform.position);
        if(rightAttach == null)
        {
            GameObject obj = GameObject.Find("[RightHand Controller] Attach");
            rightAttach = obj.transform;
        }
        gameObject.transform.SetParent(right.transform);
        distance = 0.2f; // change this later.
        //distance = 0;
        gameObject.transform.localPosition = new Vector3(0, 0, distance);
        gameObject.transform.localEulerAngles = new Vector3(270, 0, 0);
        selected = true;
    }

    public void Deselect()
    {
        //gameObject.transform.SetParent(Posts.transform);
        selected = false;
        speechToText.endButton.SetActive(false);
        speechToText.beginButton.SetActive(true);
    }


    public void SetText(string txt)
    {
        content.text = txt;
        Resize();
    }

    public void Resize()
    {
        if(content.text.Length >= 84)
        {
            content.fontSize = (84.0f / content.text.Length) * 12;
        } else
        {
            content.fontSize = 12;
        }
    }

    void MoveZ(GameObject rigid, float zIncr)
    {
        print("Old Pos: " + rigid.transform.position.ToString());
        rigid.transform.localPosition += new Vector3(0, 0, zIncr);
        Vector3 newPos = rigid.transform.position;
        print("New Pos: " + newPos.ToString());
        rigid.transform.localPosition -= new Vector3(0, 0, zIncr);
        //rigid.GetComponent<Rigidbody>().MovePosition(newPos);
        rigid.transform.position = newPos;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Rigidbody>().ResetInertiaTensor();
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        if (selected)
        {
            if (!stuckToWall)
            {
                stuckToWallControls.SetActive(false);
                floatingControls.SetActive(true);
                Vector2 vector;
                if (right.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vector))
                {
                    //gameObject.transform.localPosition += new Vector3(0, 0, vector.y * sensitivity);
                    MoveZ(gameObject, vector.y * sensitivity);
                    //distance += vector.y*sensitivity;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    //gameObject.GetComponent<Rigidbody>().MovePosition(gameObject.GetComponent<Rigidbody>().position + new Vector3(0, 0, Time.deltaTime * sensitivity));
                    MoveZ(gameObject, Time.deltaTime * sensitivity);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    MoveZ(gameObject, -Time.deltaTime * sensitivity);
                   // gameObject.GetComponent<Rigidbody>().MovePosition(gameObject.GetComponent<Rigidbody>().position - new Vector3(0, 0, Time.deltaTime * sensitivity));
                }

                if (left.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vector))
                {
                    Vector3 scale = gameObject.transform.localScale + (new Vector3 (1,0,1)) * vector.y * scaleSensitivity;
                    if (scale.x > 0)
                    {
                        transform.localScale = scale;
                    }
                }

                // not allowing deselection unless if stuck to a wall.

                /*bool pressing;
                if (right.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out pressing))
                {
                    if (pressing) // press trigger to deselect
                    {
                        selected = false;
                    }
                }*/ 

            } else
            {
                stuckToWallControls.SetActive(true);
                floatingControls.SetActive(false);
                Vector2 vector;
                if (right.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out vector))
                {
                    // swap for rigid body movement later
                    gameObject.transform.localPosition += new Vector3(vector.x * wallSensitivity, vector.y * sensitivity, 0);
                }


                if(Input.GetKey(KeyCode.W))
                {
                    gameObject.transform.localPosition += new Vector3(0, Time.deltaTime * sensitivity, 0);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    gameObject.transform.localPosition += new Vector3(0, -Time.deltaTime * sensitivity, 0);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    gameObject.transform.localPosition += new Vector3(Time.deltaTime * sensitivity, 0, 0);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    gameObject.transform.localPosition += new Vector3(-Time.deltaTime * sensitivity, 0, 0);
                }

                bool p;

                if (right.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out p))
                {
                    if(p)
                    {
                        gameObject.transform.localPosition += new Vector3(0, 0, 0.6f);
                        stuckToWall = false;
                        Select();
                        return;
                    }
                }


                if(Input.GetKey(KeyCode.DownArrow))
                {
                    gameObject.transform.localPosition += new Vector3(0, 0, 0.6f);
                    stuckToWall = false;
                    Select();
                    return;
                }

                bool pressing;
                if (right.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out pressing))
                {
                    if (pressing) // press trigger to deselect
                    {
                        Deselect();
                        //selected = false;
                        stuckToWallControls.SetActive(false);
                    }
                }


            }
        } else
        {
            //Deselect(); // get rid of this line after testing
        }
    }
}
