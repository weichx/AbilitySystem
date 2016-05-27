using System;
using UnityEditor;
using UnityEngine;
using Texture2DExtensions;

public class DecisionSetPage : Page<DecisionSet> {

    float slope = 1; //m
    float exp = 2; //k
    float hShift = 0; //c
    float vShift = 0; //b
    int width = 256;
    int height = 256;
    private Font font;
    private Texture2D fontTexture;
    private Texture2D graphTexture;
    private MasterView<DecisionSetItem, DecisionSet> masterView;
    private DecisionSetPage_DetailView detailView;

    public override void OnEnter() {
        masterView = new MasterView<DecisionSetItem, DecisionSet>(SetActiveItem);
        detailView = new DecisionSetPage_DetailView();
        graphTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        font = AssetDatabaseExtensions.FindAsset<Font>("Roboto-Light");
        fontTexture = AssetDatabaseExtensions.FindAsset<Texture2D>("Roboto-Light_Texture");
        BuildTexture();
    }

    public override void SetActiveItem(AssetItem<DecisionSet> newItem) {
        base.SetActiveItem(newItem);
        detailView.SetTargetObject(newItem);
    }


    private float Plot(float x, float slope, float exp, float hShift, float vShift) {
        return (slope * (Mathf.Pow((x - hShift), exp)) + vShift);
    }

    public override void Update() {
        if (activeItem != null) {
            activeItem.Update();
            if (activeItem.IsDeletePending) {
                detailView.SetTargetObject(null);
                masterView.RemoveItem(activeItem as DecisionSetItem);
                activeItem.Delete();
            }
        }
    }

    public override void Render(Rect rect) {
        GUILayout.BeginArea(rect);
        GUILayout.BeginHorizontal();
        masterView.Render();
        GUILayout.Space(10f);
        detailView.Render();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Creates tabs from buttons, with their bottom edge removed by the magic of Haxx
    /// </summary>
    /// <remarks>
    /// The line will be misplaced if other elements is drawn before this
    /// </remarks>
    /// <returns>Selected tab</returns>
    public static int Tabs(string[] options, int selected) {
        const float DarkGray = 0.6f;
        const float LightGray = 0.9f;
        const float StartSpace = 10;

        GUILayout.Space(StartSpace);
        Color storeColor = GUI.backgroundColor;
        Color highlightCol = new Color(LightGray, LightGray, LightGray);
        Color bgCol = new Color(DarkGray, DarkGray, DarkGray);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.padding.bottom = 8;

        GUILayout.BeginHorizontal();
        {   
            for (int i = 0; i < options.Length; ++i) {
                GUI.backgroundColor = i == selected ? highlightCol : bgCol;
                if (GUILayout.Button(options[i], buttonStyle)) {
                    selected = i; //Tab click
                }
            }
        }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = storeColor;
        return selected;
    }

    private void BuildTexture() {
        DrawGraphLines();
        graphTexture.DrawText("0.0", 0, height - 1, font, fontTexture, 14);
        graphTexture.DrawText("1.0", width - 25, height - 1, font, fontTexture, 14);
        graphTexture.FlipVertically();
        graphTexture.Apply();
    }

    private void MoveMe(Rect rect) {
        GUI.DrawTexture(new Rect(0, 0, width, height), graphTexture);
        GUILayout.BeginArea(new Rect(0, width, width, rect.height));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 50;
        EditorGUI.BeginChangeCheck();
        slope = EditorGUILayout.FloatField("Slope", slope);
        exp = EditorGUILayout.FloatField("Exp", exp);
        vShift = EditorGUILayout.FloatField("vShift", vShift);
        hShift = EditorGUILayout.FloatField("hShift", hShift);
        if (EditorGUI.EndChangeCheck()) {
            BuildTexture();
        }
        EditorGUIUtility.labelWidth = labelWidth;
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawGraphLines() {
        int graphX = 16;
        int graphY = 16;
        int graphWidth = width - 32;
        int graphHeight = height - 32;
        Color[] pixels = graphTexture.GetPixels(graphX, graphY, graphWidth, graphHeight);
        int[] intpix = new int[pixels.Length];
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i] = Color.black;
            intpix[i] = GetByte(0, 3); //seems wrong to me, should be set but set doesnt work
        }

        int lastX = graphX;
        int lastY = (int)((1 - Plot(0.0001f, slope, exp, hShift, vShift)) * graphHeight);

        Color32 c32 = Color.green;

        for (int i = 0; i < graphHeight; i++) {
            float x = i / (float)graphHeight;
            float y = Plot(x, slope, exp, hShift, vShift);
            //inverted because text rendindering is updside down and i cant figure out 
            //how to flip the text correctly, so im flipping the graph instead
            int lineY = (int)((1 - y) * graphHeight);
            AALine(intpix, graphWidth, graphHeight, lastX, lastY, i, lineY, Color.green);
            lastX = i;
            lastY = lineY;
        }


        for (int i = 0; i < intpix.Length; i++) {
            int r = SetByte(intpix[i], 0);
            int g = SetByte(intpix[i], 1);
            int b = SetByte(intpix[i], 2);
            int a = SetByte(intpix[i], 3);
            pixels[i] = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

        graphTexture.SetPixels(graphX, graphY, graphWidth, graphHeight, pixels);
    }

    private static int GetByte(int input, int byteIndex) {
        return (input | (0xff << (byteIndex * 8)));
    }

    private static int SetByte(int input, int byteIndex) {
        return (input >> (8 * byteIndex)) & 0xff;
    }

    public static void AALine(int[] pixels, int pixelWidth, int pixelHeight, int x0, int y0, int x1, int y1, Color32 c32) {
        AALine(pixels, pixelWidth, pixelHeight, x0, y0, x1, y1, c32.a, c32.r, c32.g);
    }

    /// <summary> 
    /// Draws an antialiased line, using an optimized version of Gupta-Sproull algorithm 
    /// </summary> 
    /// <param name="pixels">Pixels from a WriteableBitmap to draw to (premultiplied alpha format)</param> 
    /// <param name="pixelWidth">Width of the bitmap</param> 
    /// <param name="pixelHeight">Height of the bitmap</param> 
    /// <param name="x0">Start X</param> 
    /// <param name="y0">Start Y</param> 
    /// <param name="x1">End X</param> 
    /// <param name="y1">End Y</param> 
    /// <param name="sa">Opacity of the line (0..255)</param> 
    /// <param name="srb">Non-premultiplied red and blue component in the format 0x00rr00bb</param> 
    /// <param name="sg">Green component (0..255)</param> 
    public static void AALine(int[] pixels, int pixelWidth, int pixelHeight, int x0, int y0, int x1, int y1, int sa, uint srb, uint sg) {

        if ((x0 == x1) && (y0 == y1)) return; // edge case causing invDFloat to overflow, found by Shai Rubinshtein

        if (x0 < 1) x0 = 1;
        if (x0 > pixelWidth - 2) x0 = pixelWidth - 2;
        if (y0 < 1) y0 = 1;
        if (y0 > pixelHeight - 2) y0 = pixelHeight - 2;

        if (x1 < 1) x1 = 1;
        if (x1 > pixelWidth - 2) x1 = pixelWidth - 2;
        if (y1 < 1) y1 = 1;
        if (y1 > pixelHeight - 2) y1 = pixelHeight - 2;

        int addr = y0 * pixelWidth + x0;
        int dx = x1 - x0;
        int dy = y1 - y0;

        int du;
        int dv;
        int u;
        int v;
        int uincr;
        int vincr;

        // By switching to (u,v), we combine all eight octants 
        int adx = dx, ady = dy;
        if (dx < 0) adx = -dx;
        if (dy < 0) ady = -dy;

        if (adx > ady) {
            du = adx;
            dv = ady;
            u = x1;
            v = y1;
            uincr = 1;
            vincr = pixelWidth;
            if (dx < 0) uincr = -uincr;
            if (dy < 0) vincr = -vincr;

        }
        else {
            du = ady;
            dv = adx;
            u = y1;
            v = x1;
            uincr = pixelWidth;
            vincr = 1;
            if (dy < 0) uincr = -uincr;
            if (dx < 0) vincr = -vincr;
        }

        int uend = u + du;
        int d = (dv << 1) - du;        // Initial value as in Bresenham's 
        int incrS = dv << 1;    // Δd for straight increments 
        int incrD = (dv - du) << 1;    // Δd for diagonal increments

        double invDFloat = 1.0 / (4.0 * Math.Sqrt(du * du + dv * dv));   // Precomputed inverse denominator 
        double invD2duFloat = 0.75 - 2.0 * (du * invDFloat);   // Precomputed constant

        const int PRECISION_SHIFT = 10; // result distance should be from 0 to 1 << PRECISION_SHIFT, mapping to a range of 0..1 
        const int PRECISION_MULTIPLIER = 1 << PRECISION_SHIFT;
        int invD = (int)(invDFloat * PRECISION_MULTIPLIER);
        int invD2du = (int)(invD2duFloat * PRECISION_MULTIPLIER * sa);
        int ZeroDot75 = (int)(0.75 * PRECISION_MULTIPLIER * sa);

        int invDMulAlpha = invD * sa;
        int duMulInvD = du * invDMulAlpha; // used to help optimize twovdu * invD 
        int dMulInvD = d * invDMulAlpha; // used to help optimize twovdu * invD 
                                         //int twovdu = 0;    // Numerator of distance; starts at 0 
        int twovduMulInvD = 0; // since twovdu == 0 
        int incrSMulInvD = incrS * invDMulAlpha;
        int incrDMulInvD = incrD * invDMulAlpha;

        do {
            AlphaBlendNormalOnPremultiplied(pixels, addr, (ZeroDot75 - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
            AlphaBlendNormalOnPremultiplied(pixels, addr + vincr, (invD2du + twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
            AlphaBlendNormalOnPremultiplied(pixels, addr - vincr, (invD2du - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);

            if (d < 0) {
                // choose straight (u direction) 
                twovduMulInvD = dMulInvD + duMulInvD;
                d += incrS;
                dMulInvD += incrSMulInvD;
            }
            else {
                // choose diagonal (u+v direction) 
                twovduMulInvD = dMulInvD - duMulInvD;
                d += incrD;
                dMulInvD += incrDMulInvD;
                v++;
                addr += vincr;
            }
            u++;
            addr += uincr;
        } while (u < uend);
    }

    /// <summary> 
    /// Blends a specific source color on top of a destination premultiplied color 
    /// </summary> 
    /// <param name="pixels">Array containing destination color</param> 
    /// <param name="index">Index of destination pixel</param> 
    /// <param name="sa">Source alpha (0..255)</param> 
    /// <param name="srb">Source non-premultiplied red and blue component in the format 0x00rr00bb</param> 
    /// <param name="sg">Source green component (0..255)</param> 
    private static void AlphaBlendNormalOnPremultiplied(int[] pixels, int index, int sa, uint srb, uint sg) {
        uint destPixel = (uint)pixels[index];
        uint da, dg, drb;

        da = (destPixel >> 24);
        dg = ((destPixel >> 8) & 0xff);
        drb = destPixel & 0x00FF00FF;

        // blend with high-quality alpha and lower quality but faster 1-off RGBs 
        pixels[index] = (int)(
           ((sa + ((da * (255 - sa) * 0x8081) >> 23)) << 24) | // aplha 
           (((sg - dg) * sa + (dg << 8)) & 0xFFFFFF00) | // green 
           (((((srb - drb) * sa) >> 8) + drb) & 0x00FF00FF) // red and blue 
        );

        //dr = ((destPixel >> 16) & 0xff); 
        //db = ((destPixel) & 0xff);

        //uint srb = (uint)((sr << 16) | sb);


        //pixels[index] = (int)( 
        //   ((sa + ((da * (255 - sa) * 0x8081) >> 23)) << 24) | // alpha 
        //   (((((sr - dr) * sa) >> 8) + dr) << 16) | // red 
        //   ( ((sg - dg) * sa + (dg << 8)) & 0xFFFFFF00 ) | // green 
        //   ( (((sb - db) * sa) >> 8) + db ) ); // blue 
    }
}