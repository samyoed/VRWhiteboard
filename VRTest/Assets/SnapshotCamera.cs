using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SnapshotCamera : MonoBehaviour
{
    private static SnapshotCamera instance;
    private Camera newCamera;
    private bool takeSnapshot;
    public RenderTexture currTexture;
    public GameObject pictureTakenText;
    public bool isSpinning = true;
    // Start is called before the first frame update

    void Awake()
    {
        instance = this; 
        newCamera = gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        if(isSpinning)
            transform.parent.transform.Rotate(0,1,0);
    }

    void OnPostRender()
    {
        if(takeSnapshot) 
        {
            takeSnapshot = false;
            RenderTexture renderTexture = newCamera.targetTexture;

            //make new exture of custom size
            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false); 
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);

            // read pixels from rectangle to texture
            renderResult.ReadPixels(rect, 0, 0); 
            byte[] _bytes = renderResult.EncodeToPNG(); //encode texture to png
            string pathStart = PersistentWhiteboardInfo.pathStart;

            if(!Directory.Exists(pathStart))
			    Directory.CreateDirectory(pathStart);

            string filePath = pathStart + "/snapshot " + System.DateTime.Now.ToString("MMMM dd yyy HHmmss") + ".png"; //filepath

            System.IO.File.WriteAllBytes(filePath, _bytes);
            print("Snapshot Saved in: " + filePath);
            StartCoroutine(picTakenTextAppear());
            RenderTexture.ReleaseTemporary(renderTexture);
            newCamera.targetTexture = currTexture;
        }
        

    }


    public void StopSpin()
    {
        isSpinning = false;
    }

    void TakeSnapshot(int width, int height)
    {
        newCamera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        takeSnapshot = true;
    }

    public static void TakeSnapshot_Static(int width, int height)
    {
        instance.TakeSnapshot(width, height);
    }
    public IEnumerator picTakenTextAppear()
    {
        pictureTakenText.SetActive(true);
        yield return new WaitForSeconds(1);
        pictureTakenText.SetActive(false);
    }

}
