using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteboardPen : MonoBehaviour
{
    public Color color;
	public Whiteboard whiteboard;
	private RaycastHit touch;
	public Quaternion lockedAngle;
	private Quaternion lastAngle;

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
		rend = transform.GetComponent<MeshRenderer>();
		startPos = transform.position;
		startRot = transform.rotation;
		lockedAngle.eulerAngles = new Vector3(0, 0, 90);
	}

	void Update () 
    {
        tipObj = transform.GetChild(0);

		// float tipHeight = transform.Find ("Tip").transform.localScale.y;
		// Vector3 tip = transform.Find ("Tip").transform.position;
        
        float tipHeight = tipObj.localScale.y / 4;
		Vector3 tip = tipObj.position;

        tipObj.gameObject.GetComponent<MeshRenderer>().material.color = color;

		if (lastTouch) 
        {
			tipHeight *= 1.1f;
		}

		// Check for a Raycast from the tip of the pen
		if (Physics.Raycast (tip, transform.up, out touch, tipHeight)) 
        {
			if (touch.collider.tag == "Whiteboard") 
            {
    		    whiteboard = touch.collider.GetComponent<Whiteboard>();
                print("TOUCHING");
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
					Texture2D texture = new Texture2D(whiteboard.textureSizeX, whiteboard.textureSizeY);
					whiteboard.GetComponent<Whiteboard>().SetColor(Color.white);
					print(whiteboard.GetComponent<Whiteboard>().color.Length);
					whiteboard.GetComponent<Whiteboard>().texture.SetPixels(0, 0, whiteboard.textureSizeX, whiteboard.textureSizeY, 
																		whiteboard.GetComponent<Whiteboard>().color);
				break;

				case "Save":
					SaveTextureAsPNG(whiteboard.GetComponent<Whiteboard>().texture, "Assets/test.png");
				break;
			}
			// //for colorpicking
            // if(touch.collider.tag == "ColorPick")
            // {
            //     color = touch.collider.gameObject.GetComponent<MeshRenderer>().material.color;
            // }
			// //for board reset
			// if(touch.collider.tag == "BoardReset")
			// {
			// 	Texture2D texture = new Texture2D(whiteboard.textureSize, whiteboard.textureSize);
			// 	whiteboard.GetComponent<Whiteboard>().SetColor(Color.white);
			// 	whiteboard.GetComponent<Whiteboard>().texture.SetPixels(0, 0, whiteboard.textureSize, whiteboard.textureSize, 
			// 															whiteboard.GetComponent<Whiteboard>().color);
			// }
		} 
        else 
        {
			
			rend.material.color = Color.white;
			whiteboard.ToggleTouch (false);
			lastTouch = false;
		}

		// Lock the rotation of the pen if "touching"
		if (lastTouch) 
        {
			
			transform.rotation = lockedAngle;

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

	void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
         {
             byte[] _bytes =_texture.EncodeToPNG();
             System.IO.File.WriteAllBytes(_fullPath, _bytes);
             Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
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
