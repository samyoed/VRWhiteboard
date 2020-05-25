using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SnapshotCamera : MonoBehaviour
{
    private static SnapshotCamera instance;
    private Camera camera;
    private bool takeSnapshot;
    // Start is called before the first frame update

    void Awake()
    {
        instance = this; 
        camera = Camera.main;
    }

    void OnPostRender()
    {
        if(takeSnapshot) 
        {
            takeSnapshot = false;
            RenderTexture renderTexture = camera.targetTexture;

            //make new exture of custom size
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false); 
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);

            // read pixels from rectangle to texture
            renderResult.ReadPixels(rect, 0, 0); 
            byte[] _bytes = renderResult.EncodeToPNG(); //encode texture to png
            string pathStart = PersistentWhiteboardInfo.pathStart;

            if(!Directory.Exists(pathStart))
			    Directory.CreateDirectory(pathStart);

            string filePath = pathStart + "/snapshots" + System.DateTime.Now.ToString("MMMM dd yyy HHmmss") + ".png"; //filepath

            System.IO.File.WriteAllBytes(filePath, _bytes);
            print("SnapshotSaved!");
            RenderTexture.ReleaseTemporary(renderTexture);
            camera.targetTexture = null;
        }
    }

    void TakeSnapshot(int width, int height)
    {
        camera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeSnapshot = true;
    }

    public static void TakeSnapshot_Static(int width, int height)
    {
        instance.TakeSnapshot(width, height);
    }

}
