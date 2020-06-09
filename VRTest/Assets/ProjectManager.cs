using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;

public class ProjectManager : MonoBehaviour
{
    public Whiteboard whiteboard;
    public WhiteboardPen whiteboardPen;

    public void SaveTextureGoogleDrive()
	{

        InAppBrowser.OpenURL("http://www.google.com");
        // Texture2D _texture = whiteboard.texture;
		// byte[] _bytes =_texture.EncodeToPNG();
		// var file = new UnityGoogleDrive.Data.File() {Name = "Image.png", Content = _bytes};
		// GoogleDriveFiles.Create(file).Send();
        // print("Saved to Drive");
	}
}
