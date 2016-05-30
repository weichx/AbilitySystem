using System;
using System.Collections.Generic;
using Texture2DExtensions;
using UnityEngine;

public class GraphHelper {

    public static void DrawGraphLines(Rect rect, Texture2D graphTexture, Func<float, float> Plot ) {
        int graphX = (int)rect.x;
        int graphY = (int)rect.y;
        int graphWidth = (int)rect.width;
        int graphHeight = (int)rect.height;

        Color[] pixels = graphTexture.GetPixels(graphX, graphY, graphWidth, graphHeight);
        Color32 color = new Color(0.2f, 0.2f, 0.2f);        

        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = color;
        }

        graphTexture.SetPixels(graphX, graphY, graphWidth, graphHeight, pixels);

        int lastX = graphX;
        int lastY = graphHeight;
        
        List<Vector2I> pointlist = new List<Vector2I>();
        for (int i = 0; i < graphWidth; i++) {
            float x = i / (float)graphWidth;
            float y = Plot(x);

            if (float.IsNaN(y)) continue; //not sure why this can happen but it does with non integer exponents

            //inverted y because text rendindering is updside down and i cant figure out 
            //how to flip the text correctly, so im flipping the graph instead
            
            pointlist.Add(new Vector2I(i, (int)((1 - y) * graphHeight)));
            
        }

        Color fadedGrey = new Color(0.24f, 0.24f, 0.24f, 0.5f);
        int quarterWidth = (int)(graphWidth * 0.25f);
        int quarterHeight = (int)(graphHeight * 0.25f);

        graphTexture.DrawLine(graphX, graphY + graphHeight, graphX + graphWidth, graphY, fadedGrey);

        graphTexture.DrawLine(graphX, quarterHeight * 1, graphX + graphWidth, quarterHeight * 1, fadedGrey);
        graphTexture.DrawLine(graphX, quarterHeight * 2, graphX + graphWidth, quarterHeight * 2, fadedGrey);
        graphTexture.DrawLine(graphX, quarterHeight * 3, graphX + graphWidth, quarterHeight * 3, fadedGrey);

        graphTexture.DrawLine(quarterWidth * 1, graphY, quarterWidth * 1, graphY + graphHeight, fadedGrey);
        graphTexture.DrawLine(quarterWidth * 2, graphY, quarterWidth * 2, graphY + graphHeight, fadedGrey);
        graphTexture.DrawLine(quarterWidth * 3, graphY, quarterWidth * 3, graphY + graphHeight, fadedGrey);

        if (pointlist.Count >= 2) {
            lastX = pointlist[0].x;
            lastY = pointlist[0].y;
            for (int i = 1; i < pointlist.Count; i++) {
                int y = pointlist[i].y;
                graphTexture.DrawLine(lastX, lastY, i, y, Color.green);
                lastX = i;
                lastY = y;
            }

        }
    }
   
}