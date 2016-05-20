//using UnityEditor;
//using UnityEngine;
//using System.Collections;

//public class TestDrawing : MonoBehaviour
//{
//	public Font myFont;
//	public Texture2D myFontTexture;
//	public float speed;
//	public float fps = 40.0f;

//	int fontSize = 32;
//	int texSize = 256;		
//	int checkerSize = 64;		
//	//int circles = 10;
//	Color color1 = new Color( .8f, .8f, .8f, 0.6f );
//	Color color2 = new Color( .4f, .4f, .4f, 0.6f );
//	Color32[] pixCheckers;
//	private int yScrollPos = 0;
//	private float lastTime = 0.0f;


//	void Start ()
//	{
//		Texture2D texCheckers = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false, true);
//		texCheckers.FloodFillArea( texSize, texSize, new Color(0, 0, 0, 0) );
//		texCheckers.DrawCheckers( texSize, checkerSize, color1, color2 );
//		pixCheckers = texCheckers.GetPixels32();

//		string charsWanted = "`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?";		
//		myFont.RequestCharactersInTexture( charsWanted, fontSize );

//		Texture2D txDrawOn = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false, true);
//		GetComponent<Renderer>().material.mainTexture = txDrawOn;
//	}

//	void Update()
//	{
//		if( Time.time - (1.0f / fps) < lastTime )
//			return;
//		else
//			lastTime = Time.time;

//		yScrollPos += Mathf.FloorToInt( speed * Time.deltaTime );
		
//		if( yScrollPos > 210 )
//			yScrollPos = 0;

//		Texture2D tex = (Texture2D) GetComponent<Renderer>().material.mainTexture;

//		tex.SetPixels32( pixCheckers );

//		tex.DrawText( "Hello Worldy!", 20, yScrollPos, myFont, myFontTexture, fontSize );

//		tex.FlipVertically();

//		tex.Apply();

//		GetComponent<Renderer>().material.mainTexture = tex;
//	}

////	public static void DumpARGB32ToDisk(ref byte[] argb, uint width, uint height)
////	{
////		System.Drawing.Bitmap bmp = new System.Drawing.Bitmap((int)width, (int)height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
////		System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
////		System.Runtime.InteropServices.Marshal.Copy(argb, 0, bmpData.Scan0, argb.Length);
////		bmp.UnlockBits(bmpData);
////
////		//bmp.RotateFlip( System.Drawing.RotateFlipType.RotateNoneFlipY );
////
////		System.Runtime.InteropServices.Marshal.Copy( bmpData.Scan0, argb, 0, argb.Length);
////		//bmp.Save("Assets/_GeneratedImage.png", System.Drawing.Imaging.ImageFormat.Png);
////	}


////	Texture2D GetRTPixels(RenderTexture rt)
////	{
////		RenderTexture currentActiveRT = RenderTexture.active;
////
////		RenderTexture.active = rt;
////		Texture2D tex = new Texture2D(rt.width, rt.height);
////		tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
////
////		RenderTexture.active = currentActiveRT;
////
////		return tex;
////	}








//}
