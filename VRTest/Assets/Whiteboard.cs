using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
//using UnityEngine.Windows;
using System.IO;
//using UnityGoogleDrive;

public class Whiteboard : MonoBehaviour {

	public int textureSizeX = 512;
	public int textureSizeY = 512;
	public int penSize = 50;
	public Texture2D texture;
	public Texture2D currentTexture;
	public Color[] color;

	private bool touching, touchingLast;
	private float posX, posY;
	private float lastX, lastY;

	private GameObject sizeSlider;
	private GameObject fontNum;

	public WhiteboardPen whiteboardPen;
	public MeshRenderer renderer;
	public MeshRenderer render;


	// Use this for initialization
	void Start () {
		// Set whiteboard texture
		renderer = GetComponent<MeshRenderer>();
		whiteboardPen = GameObject.Find("WhiteboardPen").GetComponent<WhiteboardPen>();
		texture = new Texture2D(textureSizeX, textureSizeY);
		currentTexture = Instantiate(texture);
		renderer.material.mainTexture = (Texture) currentTexture;

		

		sizeSlider = GameObject.Find("Size Slider");
		fontNum = GameObject.Find("Font Size Number");

	}
	
	// Update is called once per frame
	void Update () {
		//penSize = (int)slider.value;
		//fontNum.GetComponent<TextMesh>().text = penSize + "";

		//print(posX + ", " + posY);

		// Transform textureCoords into "pixel" values
		int x = (int) (posX * textureSizeX - (penSize / 2));
		int y = (int) (posY * textureSizeY - (penSize / 2));

		// Only set the pixels if we were touching last frame
		if (touchingLast) 
		{
			// Set base touch pixels
			currentTexture.SetPixels(x, y, penSize, penSize, color);

			// Interpolate pixels from previous touch
			for (float t = 0.01f; t < 1.00f; t += 0.01f) {
				int lerpX = (int) Mathf.Lerp (lastX, (float) x, t);
				int lerpY = (int) Mathf.Lerp (lastY, (float) y, t);
				currentTexture.SetPixels (lerpX, lerpY, penSize, penSize, color);
			}
		}

		// If currently touching, apply the texture
		if (touching) {
			currentTexture.Apply ();
		}
			
		this.lastX = (float) x;
		this.lastY = (float) y;

		this.touchingLast = this.touching;
	}

	public void ToggleTouch(bool touching) 
	{
		this.touching = touching;
	}

	public void SetTouchPosition(float x, float y) 
	{
		this.posX = x;
		this.posY = y;
	}

	public void SetColor(Color color) 
	{
		this.color = Enumerable.Repeat<Color>(color, penSize * penSize).ToArray<Color>();
	}

//button press stuff
	public void penSizeEdit(float size)
	{
		penSize = (int)size;
	}

	public void ResetBoard()
	{
		currentTexture = new Texture2D(textureSizeX, textureSizeY);
		renderer.material.mainTexture = (Texture) currentTexture;
	}

	public void SwitchBoards()
	{
		whiteboardPen.SwitchBoard();
	}

	public void SaveBoard()
	{
		string pathStart = PersistentWhiteboardInfo.pathStart;
		if(!Directory.Exists(pathStart))
			Directory.CreateDirectory(pathStart);

		//string filePath = Application.persistentDataPath + "/test.png";
		string filePath = pathStart + "/whiteboard" + System.DateTime.Now.ToString("MMMM dd yyyy HHmmss") + ".png";
		SaveTextureAsPNG(this.texture, filePath);
	}

	void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes =_texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
    }
	
	
}