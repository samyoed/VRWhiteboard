using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Whiteboard : MonoBehaviour {

	public int textureSizeX = 512;
	public int textureSizeY = 512;
	public int penSize = 50;
	public Texture2D texture;
	public Color[] color;

	private bool touching, touchingLast;
	private float posX, posY;
	private float lastX, lastY;

	private GameObject sizeSlider;
	private GameObject fontNum;

	public WhiteboardPen whiteboardPen;

	// Use this for initialization
	void Start () {
		// Set whiteboard texture
		Renderer renderer = GetComponent<Renderer>();
		texture = new Texture2D(textureSizeX, textureSizeY);
		renderer.material.mainTexture = (Texture) texture;

		sizeSlider = GameObject.Find("Size Slider");
		fontNum = GameObject.Find("Font Size Number");

	}
	
	// Update is called once per frame
	void Update () {
		penSize = (int)Mathf.Round(sizeSlider.transform.position.y * 25 - 5);
		fontNum.GetComponent<TextMesh>().text = penSize + "";

		//print(posX + ", " + posY);

		// Transform textureCoords into "pixel" values
		int x = (int) (posX * textureSizeX - (penSize / 2));
		int y = (int) (posY * textureSizeY - (penSize / 2));

		// Only set the pixels if we were touching last frame
		if (touchingLast) 
		{
			// Set base touch pixels
			texture.SetPixels(x, y, penSize, penSize, color);

			// Interpolate pixels from previous touch
			for (float t = 0.01f; t < 1.00f; t += 0.01f) {
				int lerpX = (int) Mathf.Lerp (lastX, (float) x, t);
				int lerpY = (int) Mathf.Lerp (lastY, (float) y, t);
				texture.SetPixels (lerpX, lerpY, penSize, penSize, color);
			}
		}

		// If currently touching, apply the texture
		if (touching) {
			texture.Apply ();
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

	public void ResetBoard()
	{
		whiteboardPen.ResetBoard();
	}

	public void SwitchBoards()
	{
		whiteboardPen.SwitchBoard();
	}

	public void SaveBoard()
	{
		SaveTextureAsPNG(this.texture, "Assets/text.png");
	}

	void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes =_texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length/1024  + "Kb was saved as: " + _fullPath);
    }
}