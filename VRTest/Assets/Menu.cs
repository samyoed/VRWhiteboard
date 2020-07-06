using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
//using static NativeGallery;

public class Menu : MonoBehaviour
{
    RectTransform[] tempButtonList;
    public List<GameObject> buttonList;
    public GameObject whiteboardMenu;
    public GameObject whiteboardButton;
    public GameObject toolBar;
    public GameObject settingsMenu;
    public GameObject openMenuButton;
    public GameObject closeMenuButton;
    public GameObject mainMenu;
    public GameObject snapshotCameraPrefab;
    public GameObject snapshotCamera;
    public GameObject uiBG;
    public GameObject stickyMenu;
    public GameObject stickyMenuButton;
    public RectTransform canvas;
    public TextMeshProUGUI clockTime;
    public TextMeshProUGUI clockDate;
    public Camera mainCamera;

    public Vector3 startPos;
    public Quaternion startRot;
    public Vector3 prevFramePos;
    public Quaternion prevFrameRot;
    //position lock for desktop mode
    public bool positionLocked;
    public bool whiteboardButtonActive;
    public bool whiteboardActive;
    public bool toolbarActive;
    public bool settingsActive;
    public bool openMenuActive;
    public bool closeMenuActive;
    public bool uiBGActive;
    public bool timeActive;
    public bool stickyActive;
    public bool stickyButtonActive;
    

    public float distFromWhiteboard;

    void Start()
    {
        startRot = transform.localRotation;
        startPos = transform.localPosition;
    }

    void Update()
    {
        clockTime.text = System.DateTime.Now.ToString("hh:mm:ss tt"); 
        clockDate.text = System.DateTime.Now.ToString("MM/dd/yyyy");

        if(whiteboardActive)
            whiteboardMenu.SetActive(true);
        else
            whiteboardMenu.SetActive(false);
        if(toolbarActive)
            toolBar.SetActive(true);
        else
            toolBar.SetActive(false);
        if(settingsActive)
            settingsMenu.SetActive(true);
        else
            settingsMenu.SetActive(false);
        if(openMenuActive)
            openMenuButton.SetActive(true);
        else
            openMenuButton.SetActive(false);      
        if(whiteboardButtonActive)
            whiteboardButton.SetActive(true);
        else
            whiteboardButton.SetActive(false);      
        if(closeMenuActive)
            closeMenuButton.SetActive(true);
        else 
            closeMenuButton.SetActive(false);
        if(uiBGActive)
            uiBG.SetActive(true);
        else
            uiBG.SetActive(false);
        if(timeActive)
        {
            clockTime.gameObject.SetActive(true);
            clockDate.gameObject.SetActive(true);
        }
        else
        {
            clockTime.gameObject.SetActive(false);
            clockDate.gameObject.SetActive(false);
        }
        if(stickyActive)
            stickyMenu.SetActive(true);
        else
            stickyMenu.SetActive(false);


        if((Application.platform == RuntimePlatform.WindowsPlayer ||
           Application.platform == RuntimePlatform.WindowsEditor) &&
           Input.GetKeyDown(KeyCode.Escape))
        {
            if(!positionLocked)
            {
                positionLocked = true;
                OpenMenu();
            }
            else
            {
                transform.localRotation = startRot;
                transform.localPosition = startPos;
                positionLocked = false;
                CloseMenu();
            }
        }
    }
    void LateUpdate()
    {
        if(positionLocked)
        {
            transform.position = prevFramePos;
            transform.rotation = prevFrameRot;
        }
        else
        {
            prevFramePos = transform.position;
            prevFrameRot = transform.rotation;
        }
    }

    public void WhiteboardMenu()    // show whiteboard menu
    {
        whiteboardActive = true;
        whiteboardButtonActive = false;

    }
    public void hideWhiteboardMenu() // hide whiteboard menu
    {
        whiteboardActive = false;
        whiteboardButtonActive = true;
    }
    public void showToolbar() // switch to the tool toolbar
    {
        toolbarActive = true;
        settingsActive = false;
    }
    public void hideAllBars()
    {
        toolbarActive = false;
        settingsActive = false;
    }
    public void showSettings() // switch to settings toolbar
    {
        settingsActive = true;
        toolbarActive = false;
    }
    public void ShowWhiteboardButton()
    {
        whiteboardButtonActive = true;
    }
    public void HideWhiteboardbutton()
    {
        whiteboardButtonActive = false;
    }
    public void ShowSticky()
    {
        if(!stickyActive)
        {
            stickyActive = true;
            whiteboardActive = false;
            whiteboardButtonActive = false;
        }
        else
        {
            stickyActive = false;
            whiteboardButtonActive = true;
        }
    }
    public void OpenMenu()
    {
        showToolbar();
        ShowWhiteboardButton();
        openMenuActive = false;
        closeMenuActive = true;
        uiBGActive = true;
        timeActive = true;
    }
    public void CloseMenu()
    {
        hideAllBars();
        hideWhiteboardMenu();
        whiteboardButtonActive = false;
        openMenuActive = true;
        closeMenuActive = false;
        uiBGActive = false;
        timeActive = false;
    }
    public void SpawnCamera() //spawn camera or move to menu position if it already exists
    {
        if(snapshotCamera == null)
            snapshotCamera = Instantiate(snapshotCameraPrefab, transform.position, Quaternion.Euler(Vector3.zero));
        else
        {
            snapshotCamera.transform.position = transform.position;
            snapshotCamera.GetComponent<Rigidbody>().useGravity = false;
        }
        snapshotCamera.transform.GetChild(0).GetComponent<SnapshotCamera>().isSpinning = true;
    }


    private void PickImage( int maxSize )
{
	NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
	{
		Debug.Log( "Image path: " + path );
		if( path != null )
		{
			// Create Texture from selected image
			Texture2D texture = NativeGallery.LoadImageAtPath( path, maxSize );
			if( texture == null )
			{
				Debug.Log( "Couldn't load texture from " + path );
				return;
			}

			// Assign texture to a temporary quad and destroy it after 5 seconds
			GameObject quad = GameObject.CreatePrimitive( PrimitiveType.Quad );
			quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
			quad.transform.forward = Camera.main.transform.forward;
			quad.transform.localScale = new Vector3( 1f, texture.height / (float) texture.width, 1f );
			
			Material material = quad.GetComponent<Renderer>().material;
			if( !material.shader.isSupported ) // happens when Standard shader is not included in the build
				material.shader = Shader.Find( "Legacy Shaders/Diffuse" );

			material.mainTexture = texture;
				
			Destroy( quad, 5f );

			// If a procedural texture is not destroyed manually, 
			// it will only be freed after a scene change
			Destroy( texture, 5f );
		}
	}, "Select a PNG image", "image/png" );

	Debug.Log( "Permission result: " + permission );
}

}
