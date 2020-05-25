using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static NativeGallery;

public class Menu : MonoBehaviour
{
    RectTransform[] tempButtonList;
    public List<GameObject> buttonList;
    public GameObject whiteboardMenu;
    public GameObject mainMenu;
    public RectTransform canvas;
    public Camera mainCamera;

    public float distFromWhiteboard;
    void Start()
    {
        // tempButtonList = GetComponentsInChildren<RectTransform>();
        // foreach(RectTransform trans in tempButtonList)
        //     buttonList.Add(trans);
        // buttonList.Remove(canvas);
        // foreach(RectTransform trans in buttonHideList)
        //     buttonList.Remove(trans);
        
        // foreach(RectTransform trans in buttonList)
        //     trans.gameObject.SetActive(false);
        
    }
    void Update()
    {
        if(PersistentWhiteboardInfo.whiteboardActive)
        {
            whiteboardMenu.SetActive(false);
        }
        else
            whiteboardMenu.SetActive(true);
    }

    public void showDebug()
    {
        GameObject.FindObjectOfType<Reporter>().doShow();
    }
    public void hideDebug()
    {
        GameObject.FindObjectOfType<Reporter>().doShow();
    }
    public void WhiteboardMenu()
    {
        whiteboardMenu.SetActive(true);
        foreach (GameObject button in buttonList)
            if(button != whiteboardMenu)
                button.SetActive(false);
        // foreach(RectTransform trans in buttonHideList)
        //     trans.gameObject.SetActive(false);
        // foreach(RectTransform trans in buttonList)
        //     trans.gameObject.SetActive(true);
    }
    public void hideMenu()
    {
        mainMenu.SetActive(true);
        foreach (GameObject button in buttonList)
            if(button != mainMenu)
                button.SetActive(false);
    }
    public void GetImage()
    {
        string path;
        string title = "";
        string mime = "image/*";
        //NativeGallery.GetImagesFromGallery(path, title = "", mime = "image/*");
        PickImage(1024);

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
