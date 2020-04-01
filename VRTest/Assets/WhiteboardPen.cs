using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteboardPen : MonoBehaviour
{
    public Color color;
	public Whiteboard whiteboard;
	public Whiteboard whiteboard2;
	public Transform penMarker;
	public Transform penMarker2;

	private RaycastHit touch;
	public Quaternion lockedAngle;
	private Quaternion lastAngle;

	int activeBoard = 0;

	private MeshRenderer rend;
	
	private bool lastTouch;

    private Transform tipObj;

	private GameObject resetBlock;

	//for transporting marker back to original position after a set time
	private Vector3 startPos;
	private Quaternion startRot;
	private bool groundTimerStart = false;
	public float groundTimer;
	public float groundTimerMax = 3f;

	void Start () 
    {
		// Get our Whiteboard component from the whiteboard object
		whiteboard = GameObject.Find("Whiteboard").GetComponent<Whiteboard>();


		//disabling whiteboard2
		whiteboard2 = GameObject.Find("Whiteboard2").GetComponent<Whiteboard>();
		whiteboard2.GetComponent<MeshRenderer>().enabled = false;
		whiteboard2.GetComponent<MeshCollider>().enabled = false;
		whiteboard2.enabled = false;

		penMarker = whiteboard.transform.GetChild(0);
		rend = transform.GetComponent<MeshRenderer>();
		startPos = transform.position;
		startRot = transform.rotation;
		lockedAngle.eulerAngles = new Vector3(0, 0, 90);
	}

	void Update () 
    {
        tipObj = transform.GetChild(0);
		penMarker = whiteboard.transform.GetChild(0);

		// float tipHeight = transform.Find ("Tip").transform.localScale.y;
		// Vector3 tip = transform.Find ("Tip").transform.position;
        
        float tipHeight = tipObj.localScale.y / 7;

		//for hover drawing
		float hoverDist = tipObj.localScale.y / 4;
		Vector3 tip = tipObj.position;

        tipObj.gameObject.GetComponent<MeshRenderer>().material.color = color;

		if (lastTouch) 
        {
			tipHeight *= 1.1f;
		}

		if(Physics.Raycast (tip, transform.up, out touch, hoverDist))
		{
			if(touch.collider.tag == "Whiteboard")
			{
				penMarker.gameObject.SetActive(true);
				penMarker.position = new Vector3 (penMarker.position.x, tipObj.position.y, tipObj.position.z);
			}
		}
		else
		{
			penMarker.gameObject.SetActive(false);
		}

		rend.material.color = Color.white;
		// Check for a Raycast from the tip of the pen
		if (Physics.Raycast (tip, transform.up, out touch, tipHeight)) 
        {
			
			if (touch.collider.tag == "Whiteboard") 
            {

    		    whiteboard = touch.collider.GetComponent<Whiteboard>();
                print("TOUCHING");
				//feedback to if it is touching
				rend.material.color = Color.cyan;

			    // Set whiteboard parameters
			    whiteboard.SetColor (color);
			    whiteboard.SetTouchPosition (touch.textureCoord.x, touch.textureCoord.y);
			    whiteboard.ToggleTouch (true);

			    // If we started touching, get the current angle of the pen
			    if (lastTouch == false) 
                {
			    	lastTouch = true;
			    	lastAngle = transform.rotation;
			    }
            }
			
			//marker-object interactions
			switch(touch.collider.tag)
			{
				case "ColorPick":
					color = touch.collider.gameObject.GetComponent<MeshRenderer>().material.color;
				break;

				case "BoardReset":
					ResetBoard();
				break;
				case "Save":
					whiteboard.SaveBoard();
				break;
				case "Switch":
					SwitchBoard();
					
				break;
			}
		} 
        else 
        {

			//rend.material.color = Color.white;
			whiteboard.ToggleTouch (false);
			lastTouch = false;
		}

		// Lock the rotation of the pen if "touching"
		if (lastTouch) 
        {
			transform.rotation = lastAngle;

			//trying to snap the marker to the board. Won't work
			//Vector3 lockedPos = new Vector3(Mathf.Clamp(transform.position.x, 0, 0), transform.position.y, transform.position.z);
			//transform.position = lockedPos
		}

		//will return marker back to start position/rotation after set time
		if(groundTimerStart)
		{
			groundTimer += Time.deltaTime;
			if(groundTimer > groundTimerMax)
			{
				transform.position = startPos;
				transform.rotation = startRot;
			}
		}


    }

	void LateUpdate()
	{
		// if(transform.eulerAngles.z > 0 && transform.eulerAngles.z < 180)
		// {
		// 	float xClamp;
		// 	float tempZ;

		// 	if(transform.eulerAngles.z > 90)
		// 		tempZ = 180 - transform.eulerAngles.z;
		// 	else
		// 		tempZ = transform.eulerAngles.z;

		// 	xClamp = (-Mathf.Cos(tempZ*Mathf.Deg2Rad) * .13f) - .04f;

		// 	float f = Mathf.Max(xClamp, transform.position.x);
		// 	transform.position = new Vector3(f, transform.position.y, transform.position.z);
		// }
	}

	public void ResetBoard()
	{
		whiteboard.ResetBoard();
		print("board reset");
	}

	public void SwitchBoard()
	{
		Whiteboard temp = whiteboard;
		whiteboard = whiteboard2;
		whiteboard2 = temp;
		penMarker = whiteboard.transform.GetChild(0);
		whiteboard2.gameObject.GetComponent<MeshRenderer>().enabled = false;
		whiteboard.gameObject.GetComponent<MeshRenderer>().enabled = true;
		whiteboard.GetComponent<MeshCollider>().enabled = true;
		whiteboard.enabled = true;
		whiteboard2.GetComponent<MeshCollider>().enabled = false;
		whiteboard2.enabled = false;
		print("switched!");
	}

	public void colorRed()
	{
		color = Color.red;
	}
	public void colorBlue()
	{
		color = Color.blue;
	}
	public void colorBlack()
	{
		color = Color.black;
	}
	public void colorGreen()
	{
		color = Color.green;
	}
	public void colorWhite()
	{
		color = Color.white;
	}

	//for checking if the marker hit the ground --- 
	void OnCollisionEnter(Collision coll)
	{
		if(coll.transform.CompareTag("Ground"))
		{
			groundTimerStart = true;
		}
	}
	void OnCollisionExit(Collision coll)
	{
		if(coll.transform.CompareTag("Ground"))
		{
			groundTimerStart = false;
			groundTimer = 0;
		}
	}
}
