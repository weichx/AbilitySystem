using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Texture2DExtensions {
    public static class Texture2D_Extended {
        public struct Point {
            public short x;
            public short y;
            public Point(short aX, short aY) { x = aX; y = aY; }
            public Point(int aX, int aY) : this((short)aX, (short)aY) { }
        }


        private static void Colors_Composite(ref Color[] baseImage, Color[] compositeImage, int w, int h) {
            // composite image with alpha channel onto base image

            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    Color compositeColor = Colors_GetColor(compositeImage, x, y, w, h);
                    Color baseColor = Colors_GetColor(baseImage, x, y, w, h);
                    float alpha = compositeColor.a;
                    //				float newRed = (1-alpha) * baseColor.r + alpha * compositeColor.r;
                    //				float newGreen = (1-alpha) * baseColor.g + alpha * compositeColor.g;
                    //				float newBlue = (1-alpha) * baseColor.b + alpha * compositeColor.b;
                    //				float newAlpha = 1.0f;
                    //				Color newColor = new Color(newRed,newGreen,newBlue,newAlpha);
                    Color newColor = Color.Lerp(baseColor, compositeColor, alpha);
                    Colors_SetColor(ref baseImage, x, y, w, h, newColor);
                }
            }
        }


        public static void DrawText(this Texture2D tx, string sText, int offsetX, int offsetY, Font font1, Texture2D font1Tx, int fontSize) {
            CharacterInfo ci;
            char[] cText = sText.ToCharArray();

            //Material fontMat = font1.material;
            //Texture2D fontTx = (Texture2D) fontMat.mainTexture;
            Texture2D fontTx = font1Tx;

            int x, y, w, h;
            int posX = 0;

            font1.GetCharacterInfo('I', out ci);

            //int IHeight = (int) (ci.flipped ? (float) fontTx.width * (ci.uv.width) : (float) fontTx.height * (-ci.uv.height));

            for (int i = 0; i < cText.Length; i++) {
                font1.GetCharacterInfo(cText[i], out ci);

                x = (int)((float)fontTx.width * ci.uv.x);
                y = (int)((float)fontTx.height * (ci.uv.y + ci.uv.height));
                w = (int)((float)fontTx.width * ci.uv.width);
                h = (int)((float)fontTx.height * (-ci.uv.height));

                Color[] charPix = fontTx.GetPixels(x, y, w, h);

                if (ci.flipped) {
                    charPix = Colors_FlipRight(charPix, w, h);

                    x = posX;
                    //y = (IHeight - w) + w/2;
                    y = (int)-ci.vert.y;

                    int tmp = w;
                    w = h;
                    h = tmp;
                }
                else {
                    x = posX + (int)ci.vert.x;
                    y = (int)-ci.vert.y;
                }

                Color[] basePix = tx.GetPixels(offsetX + x, offsetY + y, w, h);
                Colors_Composite(ref basePix, charPix, w, h);


                tx.SetPixels(offsetX + x, offsetY + y, w, h, basePix);

                posX += (int)ci.width;
            }
        }

        public static Color[] Colors_FlipRight(Color[] pix, int width, int height) {
            Color cX;

            Color[] a = new Color[pix.Length];

            for (int y = 0; y < height; y++) {

                for (int x = 0; x < width; x++) {
                    cX = Colors_GetColor(pix, x, y, width, height);

                    Colors_SetColor(ref a, y, width - 1 - x, height, width, cX);
                }

            }

            return a;
        }

        public static Color[] Colors_FlipVertically(Color[] pix, int width, int height) {
            int row, targetRow, targetRowStart;
            Color[] a = new Color[pix.Length];

            for (int i = 0; i < pix.Length;) {
                row = i / width;
                targetRow = height - row;
                targetRowStart = (targetRow - 1) * width;

                for (int j = targetRowStart; j < targetRowStart + width; j++, i++) {
                    a[j] = pix[i];
                }
            }

            return a;
        }

        public static void FlipVertically(this Texture2D tex) {
            tex.SetPixels(Colors_FlipVertically(tex.GetPixels(), tex.width, tex.height));
        }

        // Untested.
        public static void DrawComposite(this Texture2D baseImage, Texture2D compositeImage, int cx, int cy) {
            // composite image with alpha channel onto base image
            // cx and cy define the location of composite image relative to base image

            for (int y = 0; y < compositeImage.height; ++y) {
                for (int x = 0; x < compositeImage.width; ++x) {
                    Color compositeColor = compositeImage.GetPixel(x, y);
                    Color baseColor = baseImage.GetPixel(cx + x, cy + y);
                    float alpha = compositeColor.a;
                    float newRed = (1 - alpha) * baseColor.r + alpha * compositeColor.r;
                    float newGreen = (1 - alpha) * baseColor.g + alpha * compositeColor.g;
                    float newBlue = (1 - alpha) * baseColor.b + alpha * compositeColor.b;
                    float newAlpha = 1.0f;
                    Color newColor = new Color(newRed, newGreen, newBlue, newAlpha);
                    //newImage.SetPixel (cx+x, cx+y, newColor);
                    baseImage.SetPixel(cx + x, cx + y, newColor);
                }
            }
        }

        public static void DrawCircle(this Texture2D tex, int cx, int cy, int r, Color col) {
            int y = r;
            int d = 1 / 4 - r;
            int end = (int)Mathf.Ceil(r / Mathf.Sqrt(2));

            for (int x = 0; x <= end; x++) {
                tex.SetPixel(cx + x, cy + y, col);
                tex.SetPixel(cx + x, cy - y, col);
                tex.SetPixel(cx - x, cy + y, col);
                tex.SetPixel(cx - x, cy - y, col);
                tex.SetPixel(cx + y, cy + x, col);
                tex.SetPixel(cx - y, cy + x, col);
                tex.SetPixel(cx + y, cy - x, col);
                tex.SetPixel(cx - y, cy - x, col);

                d += (2 * x) + 1;

                if (d > 0) {
                    d += 2 - 2 * y--;
                }
            }
        }

        public static void DrawCircleFill(this Texture2D tex, int cx, int cy, int r, Color col) {
            int x, y, px, nx, py, ny, d;

            for (x = 0; x <= r; x++) {
                d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
                for (y = 0; y <= d; y++) {
                    px = cx + x;
                    nx = cx - x;
                    py = cy + y;
                    ny = cy - y;

                    tex.SetPixel(px, py, col);
                    tex.SetPixel(nx, py, col);

                    tex.SetPixel(px, ny, col);
                    tex.SetPixel(nx, ny, col);
                }
            }
        }

        public static void DrawLine(this Texture2D tex, int x1, int y1, int x2, int y2, Color col) {
            int dy = (int)(y2 - y1);
            int dx = (int)(x2 - x1);
            int stepx, stepy;

            if (dy < 0) { dy = -dy; stepy = -1; }
            else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; }
            else { stepx = 1; }
            dy <<= 1;
            dx <<= 1;

            float fraction = 0;

            tex.SetPixel(x1, y1, col);
            if (dx > dy) {
                fraction = dy - (dx >> 1);
                while (Mathf.Abs(x1 - x2) > 1) {
                    if (fraction >= 0) {
                        y1 += stepy;
                        fraction -= dx;
                    }
                    x1 += stepx;
                    fraction += dy;
                    tex.SetPixel(x1, y1, col);
                }
            }
            else {
                fraction = dx - (dy >> 1);
                while (Mathf.Abs(y1 - y2) > 1) {
                    if (fraction >= 0) {
                        x1 += stepx;
                        fraction -= dy;
                    }
                    y1 += stepy;
                    fraction += dx;
                    tex.SetPixel(x1, y1, col);
                }
            }
        }

        public static void DrawGradient_Linear(this Texture2D tex, Rect rArea, int nSteps, Color col1, Color col2) {
            int fromX = (int)rArea.x;
            int toX = (int)(rArea.x + rArea.width);
            int fromY = (int)rArea.y;
            int toY = (int)(rArea.y + rArea.height);
            float fStepSize = (float)rArea.width / (float)nSteps;
            float fCurStepPos = fromX;

            for (int x = fromX; x < toX; x++) {
                if (x >= fStepSize + fCurStepPos)
                    fCurStepPos += fStepSize;

                float fPercent = (fCurStepPos - fromX) / (rArea.width - fStepSize);

                Color c = Color.Lerp(col1, col2, fPercent);

                for (int y = fromY; y < toY; y++)
                    tex.SetPixel(x, y, c);
            }
        }


        // Untested.
        public static void DrawGradient_Radial(this Texture2D mask, int texWidth, int texHeight, float maskThreshold) {

            mask = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, true);
            Vector2 maskCenter = new Vector2(texWidth * 0.5f, texHeight * 0.5f);

            for (int y = 0; y < texHeight; ++y) {
                for (int x = 0; x < texWidth; ++x) {
                    float distFromCenter = Vector2.Distance(maskCenter, new Vector2(x, y));
                    float maskPixel = (0.5f - (distFromCenter / texWidth)) * maskThreshold;
                    mask.SetPixel(x, y, new Color(maskPixel, maskPixel, maskPixel, 1.0f));
                }
            }
        }


        public static void DrawTexture(this Texture2D tex, Rect rectDrawArea, Texture2D txSrc, Rect rectSrc) {
            // todo:  check width and height 1st
            Color[] pix = txSrc.GetPixels((int)rectSrc.x, (int)rectSrc.y, (int)rectSrc.width, (int)rectSrc.height);

            tex.SetPixels((int)rectDrawArea.x, (int)rectDrawArea.y, (int)rectDrawArea.width, (int)rectDrawArea.height, pix);

        }

        public static void DrawTextureTiled(this Texture2D tex, Rect rectArea, Texture2D txTile, Rect rectTile) {
            Rect rectDrawArea = new Rect(0, 0, 0, 0);

            for (int y = (int)rectTile.y; y <= (int)rectArea.height - rectTile.height; y += (int)rectTile.height) {
                for (int x = (int)rectArea.x; x <= (int)rectArea.width - rectTile.width; x += (int)rectTile.width) {
                    rectDrawArea.x = x;
                    rectDrawArea.y = y;
                    rectDrawArea.width = rectTile.width;
                    rectDrawArea.height = rectTile.height;

                    tex.DrawTexture(rectDrawArea, txTile, rectTile);
                }
            }
        }

        public static void DrawCheckers(this Texture2D tex, int texSize, int checkerSize, Color color1, Color color2) {
            for (int j = 0; j < texSize; j += checkerSize) {
                for (int i = 0; i < texSize - checkerSize; i += (checkerSize * 2)) {
                    tex.DrawRectangle(i, j, i + checkerSize, j + checkerSize, color1);
                    tex.DrawRectangle(i + checkerSize, j, i + (checkerSize * 2), j + checkerSize, color2);
                }

                Color cTmp = color1;
                color1 = color2;
                color2 = cTmp;
            }

        }

        public static void DrawRectangle(this Texture2D tex, int nX, int nY, int nX2, int nY2, Color col) {
            for (int y = nY; y < nY2; y++) {
                for (int x = nX; x < nX2; x++) {
                    tex.SetPixel(x, y, col);
                }
            }
        }

        public static void FloodFillArea(this Texture2D tex, int nSizeX, int nSizeY, Color col) {
            for (int y = 0; y < nSizeY; y++) {
                for (int x = 0; x < nSizeX; x++) {
                    tex.SetPixel(x, y, col);
                }
            }
        }


        public static void FlipTexture(this Texture2D tex) {
            Color[] c = tex.GetPixels();
            int x, y;

            for (x = 0; x < tex.width; x++) {
                for (y = 0; y < tex.height; y++) {
                    tex.SetPixel(y, x, GetPixelFromColors(c, x, y, tex.width, tex.height));
                }
            }
        }


        public static void CompositeRotatedTexture(this Texture2D tex, Texture2D rot, float angle) {

            //rotImage = new Texture2D(tex.width, tex.height);

            int x, y;

            float x1, y1, x2, y2;



            int w = tex.width;

            int h = tex.height;

            float x0 = rot_x(angle, -w / 2.0f, -h / 2.0f) + w / 2.0f;

            float y0 = rot_y(angle, -w / 2.0f, -h / 2.0f) + h / 2.0f;



            float dx_x = rot_x(angle, 1.0f, 0.0f);

            float dx_y = rot_y(angle, 1.0f, 0.0f);

            float dy_x = rot_x(angle, 0.0f, 1.0f);

            float dy_y = rot_y(angle, 0.0f, 1.0f);





            x1 = x0;

            y1 = y0;



            for (x = 0; x < rot.width; x++) {

                x2 = x1;

                y2 = y1;

                for (y = 0; y < rot.height; y++) {

                    //tex.SetPixel (x1, y1, Color.clear);           



                    x2 += dx_x;//rot_x(angle, x1, y1); 

                    y2 += dx_y;//rot_y(angle, x1, y1); 

                    tex.SetPixel((int)Mathf.Floor(x), (int)Mathf.Floor(y), getPixel(rot, x2, y2));

                }



                x1 += dy_x;

                y1 += dy_y;



            }



            //rotImage.Apply();

            //return rotImage; 

        }



        private static Color getPixel(Texture2D tex, float x, float y) {

            Color pix;

            int x1 = (int)Mathf.Floor(x);

            int y1 = (int)Mathf.Floor(y);



            if (x1 > tex.width || x1 < 0 ||

               y1 > tex.height || y1 < 0) {

                pix = Color.clear;

            }
            else {

                pix = tex.GetPixel(x1, y1);

            }



            return pix;

        }


        private static Color GetPixelFromColors(Color[] c, float x, float y, float w, float h) {
            Color pix;
            int x1 = (int)Mathf.Floor(x);
            int y1 = (int)Mathf.Floor(y);
            int nPos = ((int)y - 0) * (int)w + (int)x;

            if (x1 > w || x1 < 0 || y1 > h || y1 < 0)
                pix = Color.clear;
            else
                pix = c[nPos];

            return pix;
        }


        private static Color Colors_GetColor(Color[] pix, int x, int y, int w, int h) {
            return pix[y * w + x];
        }

        private static void Colors_SetColor(ref Color[] pix, int x, int y, int w, int h, Color c) {
            pix[y * w + x] = c;
        }


        private static float rot_x(float angle, float x, float y) {

            float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);

            float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);

            return (x * cos + y * (-sin));

        }

        private static float rot_y(float angle, float x, float y) {

            float cos = Mathf.Cos(angle / 180.0f * Mathf.PI);

            float sin = Mathf.Sin(angle / 180.0f * Mathf.PI);

            return (x * sin + y * cos);

        }


        //	public static void FloodFillArea(this Texture2D aTex, int aX, int aY, Color aFillColor)
        //	{
        //		int w = aTex.width;
        //		int h = aTex.height;
        //		Color[] colors = aTex.GetPixels();
        //		Color refCol = colors[aX + aY * w];
        //		Queue<Point> nodes = new Queue<Point>();
        //		nodes.Enqueue(new Point(aX, aY));
        //		while (nodes.Count > 0)
        //		{
        //			Point current = nodes.Dequeue();
        //			for (int i = current.x; i < w; i++)
        //			{
        //				Color C = colors[i + current.y * w];
        //				if (C != refCol || C == aFillColor)
        //					break;
        //				colors[i + current.y * w] = aFillColor;
        //				if (current.y + 1 < h)
        //				{
        //					C = colors[i + current.y * w + w];
        //					if (C == refCol && C != aFillColor)
        //						nodes.Enqueue(new Point(i, current.y + 1));
        //				}
        //				if (current.y - 1 >= 0)
        //				{
        //					C = colors[i + current.y * w - w];
        //					if (C == refCol && C != aFillColor)
        //						nodes.Enqueue(new Point(i, current.y - 1));
        //				}
        //			}
        //			for (int i = current.x - 1; i >= 0; i--)
        //			{
        //				Color C = colors[i + current.y * w];
        //				if (C != refCol || C == aFillColor)
        //					break;
        //				colors[i + current.y * w] = aFillColor;
        //				if (current.y + 1 < h)
        //				{
        //					C = colors[i + current.y * w + w];
        //					if (C == refCol && C != aFillColor)
        //						nodes.Enqueue(new Point(i, current.y + 1));
        //				}
        //				if (current.y - 1 >= 0)
        //				{
        //					C = colors[i + current.y * w - w];
        //					if (C == refCol && C != aFillColor)
        //						nodes.Enqueue(new Point(i, current.y - 1));
        //				}
        //			}
        //		}
        //		aTex.SetPixels(colors);
        //	}


        //	public static void FloodFillBorder(this Texture2D aTex, int aX, int aY, Color aFillColor, Color aBorderColor)
        //	{
        //		int w = aTex.width;
        //		int h = aTex.height;
        //		Color[] colors = aTex.GetPixels();
        //		byte[] checkedPixels = new byte[colors.Length];
        //		Color refCol = aBorderColor;
        //		Queue<Point> nodes = new Queue<Point>();
        //		nodes.Enqueue(new Point(aX, aY));
        //		while (nodes.Count > 0)
        //		{
        //			Point current = nodes.Dequeue();
        //			
        //			for (int i = current.x; i < w; i++)
        //			{
        //				if (checkedPixels[i + current.y * w] > 0 || colors[i + current.y * w] == refCol)
        //					break;
        //				colors[i + current.y * w] = aFillColor;
        //				checkedPixels[i + current.y * w] = 1;
        //				if (current.y + 1 < h)
        //				{
        //					if (checkedPixels[i + current.y * w + w] == 0 && colors[i + current.y * w + w] != refCol)
        //						nodes.Enqueue(new Point(i, current.y + 1));
        //				}
        //				if (current.y - 1 >= 0)
        //				{
        //					if (checkedPixels[i + current.y * w - w] == 0 && colors[i + current.y * w - w] != refCol)
        //						nodes.Enqueue(new Point(i, current.y - 1));
        //				}
        //			}
        //			for (int i = current.x - 1; i >= 0; i--)
        //			{
        //				if (checkedPixels[i + current.y * w] > 0 || colors[i + current.y * w] == refCol)
        //					break;
        //				colors[i + current.y * w] = aFillColor;
        //				checkedPixels[i + current.y * w] = 1;
        //				if (current.y + 1 < h)
        //				{
        //					if (checkedPixels[i + current.y * w + w] == 0 && colors[i + current.y * w + w] != refCol)
        //						nodes.Enqueue(new Point(i, current.y + 1));
        //				}
        //				if (current.y - 1 >= 0)
        //				{
        //					if (checkedPixels[i + current.y * w - w] == 0 && colors[i + current.y * w - w] != refCol)
        //						nodes.Enqueue(new Point(i, current.y - 1));
        //				}
        //			}
        //		}
        //		aTex.SetPixels(colors);
        //	}

    }
}