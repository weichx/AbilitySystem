using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using com.ootii.Geometry;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Graphics
{
    /// <summary>
    /// Provides a way to render lines and overlay graphics into the run-time editor. This
    /// component needs to be attached to the camera GameObject.
    /// </summary>
    [ExecuteInEditMode]
    public class GraphicsManager : MonoBehaviour
    {
        /// <summary>
        /// Material used to render
        /// </summary>
        private static Material mSimpleMaterial = null;

        /// <summary>
        /// Support vectors for drawing complex shapes
        /// </summary>
        private static List<Vector3> mVectors1 = new List<Vector3>();
        private static List<Vector3> mVectors2 = new List<Vector3>();

        /// <summary>
        /// Timer used to render in the editor while NOT playing
        /// </summary>
        private static Stopwatch mInternalTimer = new Stopwatch();
        private static float InternalTime
        {
            get
            {
                if (Application.isPlaying) { return Time.time; }
                return mInternalTimer.ElapsedMilliseconds * 1000f;
            }
        }

        /// <summary>
        /// Lines we'll render
        /// </summary>
        private static List<Line> mLines = new List<Line>();

        /// <summary>
        /// Triangles we'll render
        /// </summary>
        private static List<Triangle> mTriangles = new List<Triangle>();

        /// <summary>
        /// Text we'll render
        /// </summary>
        private static List<Text> mText = new List<Text>();

        /// <summary>
        /// Default shader to use
        /// </summary>
        private static string mShader = "Hidden/GraphicsManagerUI";

        /// <summary>
        /// Default font to use
        /// </summary>
        private static Font mFont = null;

        /// <summary>
        /// Fonts and the extracted texture that we're using
        /// </summary>
        private static Dictionary<Font, TextFont> mFonts = new Dictionary<Font, TextFont>();

        /// <summary>
        /// Shape used over and over
        /// </summary>
        private static Octahedron mOctahedron = null;

        /// <summary>
        /// Default shader to load and use
        /// </summary>
        public string _DefaultShader = "Hidden/GraphicsManagerUI";
        public string DefaultShader
        {
            get { return _DefaultShader; }
            set { _DefaultShader = value; }
        }

        /// <summary>
        /// Default font to load and use
        /// </summary>
        public Font _DefaultFont;
        public Font DefaultFont
        {
            get { return _DefaultFont; }
            set { _DefaultFont = value; }
        }

        /// <summary>
        /// Renders the graphics to the scene view
        /// </summary>
        public bool _DrawToSceneView = true;
        public bool DrawToSceneView
        {
            get { return _DrawToSceneView; }
            set { _DrawToSceneView = value; }
        }

        /// <summary>
        /// Renders the graphics to the game view
        /// </summary>
        public bool _DrawToGameView = true;
        public bool DrawToGameView
        {
            get { return _DrawToGameView; }
            set { _DrawToGameView = value; }
        }

        /// <summary>
        /// Grabs the number of lines currently in the render list
        /// </summary>
        public int LineCount
        {
            get { return GraphicsManager.mLines.Count; }
        }

        /// <summary>
        /// Grabs the number of triangles currently in the render list
        /// </summary>
        public int TriangleCount
        {
            get { return GraphicsManager.mTriangles.Count; }
        }

        /// <summary>
        /// Grab the number of text entries currently in the render list
        /// </summary>
        public int TextCount
        {
            get { return GraphicsManager.mText.Count; }
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        GraphicsManager()
        {
            mInternalTimer.Start();
        }

        /// <summary>
        /// Coroutine that will be used to render our lines at the end of the frame
        /// </summary>
        /// <returns></returns>
        public IEnumerator Start()
        {
            // Initialize our materials
            GraphicsManager.CreateMaterials();

            // Load the shader
            mShader = _DefaultShader;

            // Load the font texture
            mFont = _DefaultFont;
            AddFont(mFont);

            // Render textures at the end since we can't render them in OnPostRender
            WaitForEndOfFrame lWait = new WaitForEndOfFrame();

            while (true)
            {
                yield return lWait;

                // Render textures at the end of the frame
                if (_DrawToGameView)
                {
                    GraphicsManager.RenderText();
                }

#if !UNITY_EDITOR
                // Clear all lists. We can't do this in the editor because OnDrawGizmos needs to run
                GraphicsManager.Clear();
#endif
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Best to move this as the first update in the custom
        /// execution order.
        /// </summary>
        protected void Update()
        {
            if (Application.isPlaying)
            {
                GraphicsManager.Clear();
            }
        }

        /// <summary>
        /// Renders the graphics to the scene view. Called AFTER WaitForEndOfFrame
        /// </summary>
        public void OnDrawGizmos()
        {
            if (Event.current.type.Equals(EventType.repaint))
            {
                if (_DrawToSceneView)
                {
                    GraphicsManager.RenderLines();
                    GraphicsManager.RenderTriangles();
                }
            }

            // When in the editor, we can clear here
            if (!Application.isPlaying)
            {
                GraphicsManager.Clear();
            }
        }

#endif

        /// <summary>
        /// Renders the graphics to the game view
        /// </summary>
        protected void OnPostRender()
        {
            if (_DrawToGameView)
            {
                GraphicsManager.RenderLines();
                GraphicsManager.RenderTriangles();
            }
        }

        /// <summary>
        /// Releases any graphics we've allocated
        /// </summary>
        public static void Clear()
        {
            float lTime = InternalTime;

            for (int i = mLines.Count - 1; i >= 0; i --)
            {
                if (mLines[i].ExpirationTime < lTime)
                {
                    Line.Release(mLines[i]);
                    mLines.RemoveAt(i);
                }
            }

            for (int i = mTriangles.Count - 1; i >= 0; i--)
            {
                if (mTriangles[i].ExpirationTime < lTime)
                {
                    Triangle.Release(mTriangles[i]);
                    mTriangles.RemoveAt(i);
                }
            }

            for (int i = mText.Count - 1; i >= 0; i--)
            {
                if (mText[i].ExpirationTime < lTime)
                {
                    Text.Release(mText[i]);
                    mText.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Draws a line at the end of the frame using Unity's GL 
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rColor"></param>
        public static void DrawLine(Vector3 rStart, Vector3 rEnd, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            Line lLine = Line.Allocate();
            lLine.Transform = rTransform;
            lLine.Start = rStart;
            lLine.End = rEnd;
            lLine.Color = rColor;
            lLine.ExpirationTime = InternalTime + rDuration;

            mLines.Add(lLine);
        }

        /// <summary>
        /// Drawas a list of connected lines where i0->i1->i2 etc.
        /// </summary>
        /// <param name="rLines"></param>
        /// <param name="rColor"></param>
        public static void DrawLines(List<Vector3> rLines, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            for (int i = 1; i < rLines.Count; i++)
            {
                Line lLine = Line.Allocate();
                lLine.Transform = rTransform;
                lLine.Start = rLines[i - 1];
                lLine.End = rLines[i];
                lLine.Color = rColor;
                lLine.ExpirationTime = InternalTime + rDuration;

                mLines.Add(lLine);
            }
        }

        /// <summary>
        /// Draws a triangle at the end of the frame using Unity's GL
        /// </summary>
        /// <param name="rPoint1"></param>
        /// <param name="rPoint2"></param>
        /// <param name="rPoint3"></param>
        /// <param name="rColor"></param>
        /// <param name="rFill"></param>
        /// <param name="rTransform"></param>
        public static void DrawTriangle(Vector3 rPoint1, Vector3 rPoint2, Vector3 rPoint3, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            Triangle lTriangle = Triangle.Allocate();
            lTriangle.Transform = rTransform;
            lTriangle.Point1 = rPoint1;
            lTriangle.Point2 = rPoint2;
            lTriangle.Point3 = rPoint3;
            lTriangle.Color = rColor;
            lTriangle.ExpirationTime = InternalTime + rDuration;

            mTriangles.Add(lTriangle);
        }

        /// <summary>
        /// Draws a box given the specified dimensions
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rHeight"></param>
        /// <param name="rWidth"></param>
        /// <param name="rDepth"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawBox(Vector3 rCenter, float rWidth, float rHeight, float rDepth, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            float lHalfWidth = rWidth * 0.5f;
            float lHalfHeight = rHeight * 0.5f;
            float lHalfDepth = rDepth * 0.5f;

            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, lHalfHeight, -lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(-lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, -lHalfDepth), rCenter + new Vector3(lHalfWidth, lHalfHeight, -lHalfDepth), rColor, rTransform, rDuration);
            DrawLine(rCenter + new Vector3(lHalfWidth, -lHalfHeight, lHalfDepth), rCenter + new Vector3(-lHalfWidth, -lHalfHeight, lHalfDepth), rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawBox(Bounds rBounds, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawBox(rBounds.center, rBounds.size.x, rBounds.size.y, rBounds.size.z, rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawCollider(BoxCollider rColldier, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            if (rColldier == null) { return; }

            mVectors1.Clear();
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);

            for (int i = 0; i < mVectors1.Count; i++)
            {
                mVectors1[i] = rColldier.transform.TransformPoint(mVectors1[i]);
            }

            DrawLine(mVectors1[0], mVectors1[1], rColor, rTransform, rDuration);
            DrawLine(mVectors1[1], mVectors1[2], rColor, rTransform, rDuration);
            DrawLine(mVectors1[2], mVectors1[3], rColor, rTransform, rDuration);
            DrawLine(mVectors1[3], mVectors1[0], rColor, rTransform, rDuration);
            DrawLine(mVectors1[4], mVectors1[5], rColor, rTransform, rDuration);
            DrawLine(mVectors1[5], mVectors1[6], rColor, rTransform, rDuration);
            DrawLine(mVectors1[6], mVectors1[7], rColor, rTransform, rDuration);
            DrawLine(mVectors1[7], mVectors1[4], rColor, rTransform, rDuration);
            DrawLine(mVectors1[0], mVectors1[4], rColor, rTransform, rDuration);
            DrawLine(mVectors1[1], mVectors1[5], rColor, rTransform, rDuration);
            DrawLine(mVectors1[2], mVectors1[6], rColor, rTransform, rDuration);
            DrawLine(mVectors1[3], mVectors1[7], rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws a bounding box given the coordinates
        /// </summary>
        /// <param name="rBounds"></param>
        /// <param name="rColor"></param>
        /// <param name="rTransform"></param>
        /// <param name="rDuration"></param>
        public static void DrawSolidCollider(BoxCollider rColldier, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            if (rColldier == null) { return; }

            mVectors1.Clear();
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, 0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, 0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(-0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);
            mVectors1.Add(new Vector3(0.5f * rColldier.size.x, -0.5f * rColldier.size.y, -0.5f * rColldier.size.z) + rColldier.center);

            for (int i = 0; i < mVectors1.Count; i++)
            {
                mVectors1[i] = rColldier.transform.TransformPoint(mVectors1[i]);
            }

            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);
            DrawTriangle(mVectors1[0], mVectors1[1], mVectors1[2], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[0], mVectors1[2], mVectors1[3], lColor, rTransform, rDuration);

            DrawTriangle(mVectors1[0], mVectors1[1], mVectors1[5], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[0], mVectors1[5], mVectors1[4], lColor, rTransform, rDuration);

            DrawTriangle(mVectors1[0], mVectors1[3], mVectors1[7], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[0], mVectors1[7], mVectors1[4], lColor, rTransform, rDuration);

            DrawTriangle(mVectors1[6], mVectors1[5], mVectors1[1], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[6], mVectors1[1], mVectors1[2], lColor, rTransform, rDuration);

            DrawTriangle(mVectors1[6], mVectors1[7], mVectors1[4], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[6], mVectors1[4], mVectors1[5], lColor, rTransform, rDuration);

            DrawTriangle(mVectors1[6], mVectors1[2], mVectors1[3], lColor, rTransform, rDuration);
            DrawTriangle(mVectors1[6], mVectors1[3], mVectors1[7], lColor, rTransform, rDuration);

            DrawLine(mVectors1[0], mVectors1[1], rColor, rTransform, rDuration);
            DrawLine(mVectors1[1], mVectors1[2], rColor, rTransform, rDuration);
            DrawLine(mVectors1[2], mVectors1[3], rColor, rTransform, rDuration);
            DrawLine(mVectors1[3], mVectors1[0], rColor, rTransform, rDuration);
            DrawLine(mVectors1[4], mVectors1[5], rColor, rTransform, rDuration);
            DrawLine(mVectors1[5], mVectors1[6], rColor, rTransform, rDuration);
            DrawLine(mVectors1[6], mVectors1[7], rColor, rTransform, rDuration);
            DrawLine(mVectors1[7], mVectors1[4], rColor, rTransform, rDuration);
            DrawLine(mVectors1[0], mVectors1[4], rColor, rTransform, rDuration);
            DrawLine(mVectors1[1], mVectors1[5], rColor, rTransform, rDuration);
            DrawLine(mVectors1[2], mVectors1[6], rColor, rTransform, rDuration);
            DrawLine(mVectors1[3], mVectors1[7], rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawCircle(Vector3 rCenter, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawCircle(rCenter, rRadius, rColor, Vector3.up, rTransform, rDuration);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawCircle(Vector3 rCenter, float rRadius, Color rColor, Vector3 rNormal, Transform rTransform = null, float rDuration = 0f)
        {
            int lCount = 36;
            Vector3[] lPositions = new Vector3[lCount];

            Quaternion lRotation = Quaternion.AngleAxis(360f / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = Vector3.forward * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                lPositions[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawLine(lPositions[i - 1], lPositions[i], rColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCircle(Vector3 rCenter, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawSolidCircle(rCenter, rRadius, rColor, Vector3.up, rTransform, rDuration);
        }

        /// <summary>
        /// Renders a simple circle whose normal is Vector3.up
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCircle(Vector3 rCenter, float rRadius, Color rColor, Vector3 rNormal, Transform rTransform = null, float rDuration = 0f)
        {
            int lCount = 36;
            mVectors1.Clear();

            Quaternion lRotation = Quaternion.AngleAxis(360f / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = Vector3.forward * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                mVectors1.Add(rCenter + lSurfacePoint);
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);
            for (int i = 1; i < lCount; i++)
            {
                DrawLine(mVectors1[i - 1], mVectors1[i], rColor, rTransform, rDuration);
                DrawTriangle(rCenter, mVectors1[i - 1], mVectors1[i], lColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            int lCount = 36;
            Vector3[] lPositions = new Vector3[lCount];

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = rFrom.normalized * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                lPositions[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawLine(lPositions[i - 1], lPositions[i], rColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawSolidArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            int lCount = 36;
            Vector3[] lPositions = new Vector3[lCount];

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);
            Vector3 lSurfacePoint = rFrom.normalized * rRadius;
            for (int i = 0; i < lCount; i++)
            {
                lPositions[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawTriangle(rCenter, lPositions[i - 1], lPositions[i], rColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCenteredArc(Vector3 rCenter, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            DrawSolidCenteredArc(rCenter, Vector3.up, rFrom, rAngle, rRadius, rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws an arc representing the angle specified
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rNormal"></param>
        /// <param name="rFrom"></param>
        /// <param name="rAngle"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidCenteredArc(Vector3 rCenter, Vector3 rNormal, Vector3 rFrom, float rAngle, float rRadius, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            int lCount = 36;
            Vector3[] lPositions = new Vector3[lCount];

            Quaternion lRotation = Quaternion.AngleAxis(rAngle / (float)(lCount - 1), rNormal);

            Vector3 lSurfacePoint = Quaternion.AngleAxis(-rAngle * 0.5f, rNormal) * (rFrom.normalized * rRadius);
            for (int i = 0; i < lCount; i++)
            {
                lPositions[i] = rCenter + lSurfacePoint;
                lSurfacePoint = lRotation * lSurfacePoint;
            }

            for (int i = 1; i < lCount; i++)
            {
                DrawTriangle(rCenter, lPositions[i - 1], lPositions[i], rColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Draws an arrow from start to end (with end being the tip) at the end of the frame using Unity's GL 
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rColor"></param>
        public static void DrawArrow(Vector3 rStart, Vector3 rEnd, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            Line lLine = Line.Allocate();
            lLine.Transform = rTransform;
            lLine.Start = rStart;
            lLine.End = rEnd;
            lLine.Color = rColor;
            lLine.ExpirationTime = InternalTime + rDuration;

            mLines.Add(lLine);

            DrawPoint(rEnd, rColor, rTransform, rDuration);
        }

        /// <summary>
        /// Draws a wire frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        /// <param name="rColor"></param>
        public static void DrawFrustum(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rMinDistance, float rMaxDistance, Color rColor, bool rIsSpherical = true)
        {
            if (rHAngle == 0 || rVAngle == 0 || rMaxDistance == 0) { return; }

            int lSteps = 10;
            int lStepsPlus1 = lSteps + 1;

            float lHalfHAngle = rHAngle * 0.5f;
            float lHalfVAngle = rVAngle * 0.5f;
            Vector3 lPoint = Vector3.zero;

            mVectors1.Clear();
            mVectors2.Clear();
            for (float lVTheta = -lHalfVAngle; lVTheta <= lHalfVAngle; lVTheta += (rVAngle / lSteps))
            {
                float lTrueVTheta = -(lVTheta * Mathf.Deg2Rad);
                float lY = Mathf.Sin(lTrueVTheta);

                for (float lHTheta = -lHalfHAngle; lHTheta <= lHalfHAngle; lHTheta += (rHAngle / lSteps))
                {
                    float lTrueHTheta = -(lHTheta * Mathf.Deg2Rad) + 1.57079f;
                    float lX = Mathf.Cos(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);
                    float lZ = Mathf.Sin(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);

                    lPoint.x = rMinDistance * lX;
                    lPoint.y = rMinDistance * lY;
                    lPoint.z = rMinDistance * lZ;
                    mVectors1.Add(rPosition + (rRotation * lPoint));

                    lPoint.x = rMaxDistance * lX;
                    lPoint.y = rMaxDistance * lY;
                    lPoint.z = rMaxDistance * lZ;
                    mVectors2.Add(rPosition + (rRotation * lPoint));
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the horizontal lines
                for (int x = 0; x < lSteps; x++)
                {
                    DrawLine(mVectors2[x], mVectors2[x + 1], rColor);
                    DrawLine(mVectors2[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], rColor);
                    DrawLine(mVectors1[x], mVectors1[x + 1], rColor);
                    DrawLine(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], rColor);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the vertical lines
                for (int y = 0; y < lSteps; y++)
                {
                    DrawLine(mVectors2[(y * lStepsPlus1) + 0], mVectors2[((y + 1) * lStepsPlus1) + 0], rColor);
                    DrawLine(mVectors2[(y * lStepsPlus1) + lSteps], mVectors2[((y + 1) * lStepsPlus1) + lSteps], rColor);
                    DrawLine(mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], rColor);
                    DrawLine(mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], rColor);

                    DrawLine(mVectors1[(y * lStepsPlus1) + (lSteps / 2)], mVectors1[((y + 1) * lStepsPlus1) + (lSteps / 2)], rColor);
                    DrawLine(mVectors2[(y * lStepsPlus1) + (lSteps / 2)], mVectors2[((y + 1) * lStepsPlus1) + (lSteps / 2)], rColor);
                }
            }

            if (rHAngle < 360f && rVAngle < 360f)
            {
                // Draw the corners
                DrawLine(mVectors1[0], mVectors2[0], rColor);
                DrawLine(mVectors1[lSteps], mVectors2[lSteps], rColor);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + 0], mVectors2[(lSteps * lStepsPlus1) + 0], rColor);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + lSteps], mVectors2[(lSteps * lStepsPlus1) + lSteps], rColor);
            }
        }

        /// <summary>
        /// Draws a solid frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        /// <param name="rColor"></param>
        public static void DrawSolidFrustum(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rMinDistance, float rMaxDistance, Color rColor, bool rIsSpherical = true)
        {
            if (rHAngle == 0 || rVAngle == 0 || rMaxDistance == 0) { return; }

            int lSteps = 10;
            int lStepsPlus1 = lSteps + 1;

            float lHalfHAngle = rHAngle * 0.5f;
            float lHalfVAngle = rVAngle * 0.5f;
            Vector3 lPoint = Vector3.zero;

            mVectors1.Clear();
            mVectors2.Clear();
            for (float lVTheta = -lHalfVAngle; lVTheta <= lHalfVAngle; lVTheta += (rVAngle / lSteps))
            {
                float lTrueVTheta = -(lVTheta * Mathf.Deg2Rad);
                float lY = Mathf.Sin(lTrueVTheta);

                for (float lHTheta = -lHalfHAngle; lHTheta <= lHalfHAngle; lHTheta += (rHAngle / lSteps))
                {
                    float lTrueHTheta = -(lHTheta * Mathf.Deg2Rad) + 1.57079f;
                    float lX = Mathf.Cos(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);
                    float lZ = Mathf.Sin(lTrueHTheta) * (rIsSpherical ? Mathf.Cos(lTrueVTheta) : 1f);

                    lPoint.x = rMinDistance * lX;
                    lPoint.y = rMinDistance * lY;
                    lPoint.z = rMinDistance * lZ;
                    mVectors1.Add(rPosition + (rRotation * lPoint));

                    lPoint.x = rMaxDistance * lX;
                    lPoint.y = rMaxDistance * lY;
                    lPoint.z = rMaxDistance * lZ;
                    mVectors2.Add(rPosition + (rRotation * lPoint));
                }
            }

            // Draw all the surfaces
            Color lColor = new Color(rColor.r, rColor.g, rColor.b, 0.1f);

            if (rMinDistance > 0f)
            {
                // Draw the near surface
                for (int y = 0; y < lSteps; y++)
                {
                    for (int x = 0; x < lSteps; x++)
                    {
                        DrawTriangle(mVectors1[((y + 1) * lStepsPlus1) + x], mVectors1[(y * lStepsPlus1) + x], mVectors1[((y + 1) * lStepsPlus1) + x + 1], lColor);
                        DrawTriangle(mVectors1[(y * lStepsPlus1) + x], mVectors1[(y * lStepsPlus1) + x + 1], mVectors1[((y + 1) * lStepsPlus1) + x + 1], lColor);
                    }
                }
            }

            // Draw the far surface
            for (int y = 0; y < lSteps; y++)
            {
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + x], mVectors2[(y * lStepsPlus1) + x], mVectors2[((y + 1) * lStepsPlus1) + x + 1], lColor);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + x], mVectors2[(y * lStepsPlus1) + x + 1], mVectors2[((y + 1) * lStepsPlus1) + x + 1], lColor);
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the top surface
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[x], mVectors1[x], mVectors2[x + 1], lColor);
                    DrawTriangle(mVectors1[x], mVectors1[x + 1], mVectors2[x + 1], lColor);
                }

                // Draw the bottom surface
                for (int x = 0; x < lSteps; x++)
                {
                    DrawTriangle(mVectors2[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], lColor);
                    DrawTriangle(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], mVectors2[(lSteps * lStepsPlus1) + x + 1], lColor);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the left surface
                for (int y = 0; y < lSteps; y++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + 0], mVectors2[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], lColor);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + 0], mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], lColor);
                }

                // Draw the right surface
                for (int y = 0; y < lSteps; y++)
                {
                    DrawTriangle(mVectors2[((y + 1) * lStepsPlus1) + lSteps], mVectors2[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], lColor);
                    DrawTriangle(mVectors2[(y * lStepsPlus1) + lSteps], mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], lColor);
                }
            }

            if (rVAngle < 360f)
            {
                // Draw the horizontal lines
                for (int x = 0; x < lSteps; x++)
                {
                    DrawLine(mVectors2[x], mVectors2[x + 1], rColor);
                    DrawLine(mVectors2[(lSteps * lStepsPlus1) + x], mVectors2[(lSteps * lStepsPlus1) + x + 1], rColor);
                    DrawLine(mVectors1[x], mVectors1[x + 1], rColor);
                    DrawLine(mVectors1[(lSteps * lStepsPlus1) + x], mVectors1[(lSteps * lStepsPlus1) + x + 1], rColor);
                }
            }

            if (rHAngle < 360f)
            {
                // Draw the vertical lines
                for (int y = 0; y < lSteps; y++)
                {
                    DrawLine(mVectors2[(y * lStepsPlus1) + 0], mVectors2[((y + 1) * lStepsPlus1) + 0], rColor);
                    DrawLine(mVectors2[(y * lStepsPlus1) + lSteps], mVectors2[((y + 1) * lStepsPlus1) + lSteps], rColor);
                    DrawLine(mVectors1[(y * lStepsPlus1) + 0], mVectors1[((y + 1) * lStepsPlus1) + 0], rColor);
                    DrawLine(mVectors1[(y * lStepsPlus1) + lSteps], mVectors1[((y + 1) * lStepsPlus1) + lSteps], rColor);
                }
            }

            if (rHAngle < 360f && rVAngle < 360f)
            {
                // Draw the corners
                DrawLine(mVectors1[0], mVectors2[0], rColor);
                DrawLine(mVectors1[lSteps], mVectors2[lSteps], rColor);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + 0], mVectors2[(lSteps * lStepsPlus1) + 0], rColor);
                DrawLine(mVectors1[(lSteps * lStepsPlus1) + lSteps], mVectors2[(lSteps * lStepsPlus1) + lSteps], rColor);
            }
        }

        /// <summary>
        /// Draws a diamond that represents a single point.
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rColor"></param>
        public static void DrawPoint(Vector3 rCenter, Color rColor, Transform rTransform = null, float rDuration = 0f)
        {
            if (mOctahedron == null) { mOctahedron = new Octahedron(); }

            float lScale = 0.075f;
            for (int i = 0; i < mOctahedron.Triangles.Length; i = i + 3)
            {
                DrawTriangle(rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i]] * lScale), rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i + 1]] * lScale), rCenter + (mOctahedron.Vertices[mOctahedron.Triangles[i + 2]] * lScale), rColor, rTransform, rDuration);
            }
        }

        /// <summary>
        /// Draws a diamond that represents a single point.
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rColor"></param>
        public static void DrawQuaternion(Vector3 rCenter, Quaternion rRotation, float rScale = 1f, float rDuration = 0f)
        {
            DrawLine(rCenter, rCenter + rRotation.Forward() * rScale, Color.blue, null, rDuration);
            DrawLine(rCenter, rCenter + rRotation.Right() * rScale, Color.red, null, rDuration);
            DrawLine(rCenter, rCenter + rRotation.Up() * rScale, Color.green, null, rDuration);
        }

        /// <summary>
        /// Draws a wire capsule
        /// </summary>
        public static void DrawCapsule(Vector3 rStart, Vector3 rEnd, float rRadius, Color rColor, float rDuration = 0f)
        {
            Vector3 lDirection = (rEnd - rStart).normalized;
            Quaternion lRotation = (lDirection.sqrMagnitude == 0f ? Quaternion.identity : Quaternion.LookRotation(lDirection, Vector3.up));

            Vector3 lForward = lRotation * Vector3.forward;
            Vector3 lRight = lRotation * Vector3.right;
            Vector3 lUp = lRotation * Vector3.up;

            DrawArc(rStart, lForward, lUp, 360f, rRadius, rColor, null, rDuration);
            DrawArc(rStart, lUp, lRight, 180f, rRadius, rColor, null, rDuration);
            DrawArc(rStart, lRight, -lUp, 180f, rRadius, rColor, null, rDuration);

            DrawArc(rEnd, lForward, lUp, 360f, rRadius, rColor, null, rDuration);
            DrawArc(rEnd, lUp, -lRight, 180f, rRadius, rColor, null, rDuration);
            DrawArc(rEnd, lRight, lUp, 180f, rRadius, rColor, null, rDuration);

            DrawLine(rStart + (lRight * rRadius), rEnd + (lRight * rRadius), rColor, null, rDuration);
            DrawLine(rStart + (-lRight * rRadius), rEnd + (-lRight * rRadius), rColor, null, rDuration);
            DrawLine(rStart + (lUp * rRadius), rEnd + (lUp * rRadius), rColor, null, rDuration);
            DrawLine(rStart + (-lUp * rRadius), rEnd + (-lUp * rRadius), rColor, null, rDuration);
        }

        /// <summary>
        /// Renders a camera facing texture to the screen given the world position and size
        /// </summary>
        /// <param name="rTexture">Texture to render</param>
        /// <param name="rPosition">Center position in world space</param>
        /// <param name="rWidth">Size of the texture in pixels</param>
        /// <param name="rHeight">Size of the texture in pixels</param>
        public static void DrawTexture(Texture rTexture, Vector3 rPosition, float rWidth, float rHeight)
        {
            Vector2 lScreenPoint = Camera.main.WorldToScreenPoint(rPosition);
            Rect lScreenRect = new Rect(lScreenPoint.x - (rWidth * 0.5f), Screen.height - lScreenPoint.y - (rHeight * 0.5f), rWidth, rHeight);

            //UnityEngine.Graphics.DrawTexture(lScreenRect, rTexture);
            UnityEngine.GUI.DrawTexture(lScreenRect, rTexture);
        }

        /// <summary>
        /// Renders a camera facing texture to the screen given the screen position and size
        /// </summary>
        /// <param name="rTexture">Texture to render</param>
        /// <param name="rPosition">Center position in screen space</param>
        /// <param name="rWidth">Size of the texture in pixels</param>
        /// <param name="rHeight">Size of the texture in pixels</param>
        public static void DrawTexture(Texture rTexture, Vector2 rPosition, float rWidth, float rHeight)
        {
            rPosition.x = rPosition.x * Screen.width;
            rPosition.y = rPosition.y * Screen.height;
            Rect lScreenRect = new Rect(rPosition.x - (rWidth * 0.5f), Screen.height - rPosition.y - (rHeight * 0.5f), rWidth, rHeight);

            //UnityEngine.Graphics.DrawTexture(lScreenRect, rTexture);
            UnityEngine.GUI.DrawTexture(lScreenRect, rTexture);
        }

        /// <summary>
        /// Draws text on the scene
        /// </summary>
        /// <param name="rText"></param>
        /// <param name="rPosition"></param>
        /// <param name="rColor"></param>
        public static void DrawText(string rText, Vector3 rPosition, Color rColor, float rDuration = 0f)
        {
            DrawText(rText, rPosition, rColor, mFont, rDuration);
        }

        /// <summary>
        /// Draws text on the screen
        /// </summary>
        /// <param name="sText"></param>
        /// <param name="rPosition"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="rFont"></param>
        /// <param name="fontSize"></param>
        public static void DrawText(string rText, Vector3 rPosition, Color rColor, Font rFont, float rDuration = 0f)
        {
            // Extract the font texture if we haven't already
            if (!mFonts.ContainsKey(rFont))
            {
                if (!AddFont(rFont)) { return; }
            }

            TextFont lTextFont = mFonts[rFont];

            // Gather information about the text
            int lWidth = Mathf.Abs(lTextFont.MinX);
            CharacterInfo lInfo;

            char[] lString = rText.ToCharArray();
            for (int i = 0; i < lString.Length; i++)
            {
                rFont.GetCharacterInfo(lString[i], out lInfo);
                lWidth += Mathf.Max(lInfo.advance, lInfo.glyphWidth);
            }

            int lHeight = lTextFont.MaxY - lTextFont.MinY;

            // Create the destination texture and clear it
            Texture2D lTexture = new Texture2D(lWidth, lHeight, TextureFormat.ARGB32, false, true);
            Color32[] lColors = new Color32[lWidth * lHeight];
            for (int i = 0; i < lColors.Length; i++) { lColors[i] = new Color32(0, 0, 0, 0); }
            lTexture.SetPixels32(lColors);

            // Process each character in the text
            int lStartX = Mathf.Abs(lTextFont.MinX);
            int lStartY = Mathf.Abs(lTextFont.MinY);

            for (int i = 0; i < lString.Length; i++)
            {
                TextCharacter lCharacter = GetCharacterPixels(rFont, lString[i]);

                // Change color and set pixels if we're not dealing with a white space
                if (lCharacter.Pixels != null)
                {
                    for (int j = 0; j < lCharacter.Pixels.Length; j++)
                    {
                        rColor.a = lCharacter.Pixels[j].a;
                        lCharacter.Pixels[j] = rColor;
                    }

                    lTexture.SetPixels(lStartX + lCharacter.MinX, lStartY + lCharacter.MinY, lCharacter.Width, lCharacter.Height, lCharacter.Pixels);
                }

                // Move our cursor forward
                lStartX += (int)lCharacter.Advance;
            }

            // Apply all the changes
            lTexture.Apply();

            // The allocation is really just so we can destroy the texture at the end of the frame
            Text lText = Text.Allocate();
            lText.Position = rPosition;
            lText.Texture = lTexture;
            lText.ExpirationTime = InternalTime + rDuration;
            mText.Add(lText);
        }

        /// <summary>
        /// Adds a font to the list of available fonts and extracts out the texture
        /// </summary>
        /// <param name="rFont"></param>
        public static bool AddFont(Font rFont)
        {
            if (rFont == null) { return false; }
            if (mFonts.ContainsKey(rFont)) { return true; }

            Texture2D lFontTexture = (Texture2D)rFont.material.mainTexture;
            byte[] lFontRawData = lFontTexture.GetRawTextureData();

            Texture2D lSourceTexture = new Texture2D(lFontTexture.width, lFontTexture.height, lFontTexture.format, false);
            lSourceTexture.LoadRawTextureData(lFontRawData);
            lSourceTexture.Apply();

            // Create the cache
            TextFont lTextFont = TextFont.Allocate();
            lTextFont.Font = rFont;
            lTextFont.Texture = lSourceTexture;

            // Find the MinY
            CharacterInfo lInfo;
            char[] lString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,?:;~!@#$%^&*()_+-=".ToCharArray();

            for (int i = 0; i < lString.Length; i++)
            {
                rFont.GetCharacterInfo(lString[i], out lInfo);
                if (lInfo.minX < lTextFont.MinX) { lTextFont.MinX = lInfo.minX; }
                if (lInfo.maxX > lTextFont.MaxX) { lTextFont.MaxX = lInfo.maxX; }
                if (lInfo.minY < lTextFont.MinY) { lTextFont.MinY = lInfo.minY; }
                if (lInfo.maxY > lTextFont.MaxY) { lTextFont.MaxY = lInfo.maxY; }
            }

            // Store the cache
            mFonts.Add(rFont, lTextFont);

            return true;
        }

        /// <summary>
        /// Draws any lines that need to be drawn
        /// </summary>
        private static void RenderLines()
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mLines.Count; i++)
            {
                Line lLine = mLines[i];

                GL.PushMatrix();

                if (lLine.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lLine.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.LINES);
                GL.Color(lLine.Color);
                GL.Vertex3(lLine.Start.x, lLine.Start.y, lLine.Start.z);
                GL.Vertex3(lLine.End.x, lLine.End.y, lLine.End.z);
                GL.End();

                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Draws any triangles that need to be drawn
        /// </summary>
        private static void RenderTriangles()
        {
            if (mSimpleMaterial == null) { CreateMaterials(); }
            mSimpleMaterial.SetPass(0);

            for (int i = 0; i < mTriangles.Count; i++)
            {
                Triangle lTriangle = mTriangles[i];

                GL.PushMatrix();

                if (lTriangle.Transform == null)
                {
                    GL.MultMatrix(Matrix4x4.identity);
                }
                else
                {
                    GL.MultMatrix(lTriangle.Transform.localToWorldMatrix);
                }

                GL.Begin(GL.TRIANGLES);
                GL.Color(lTriangle.Color);
                GL.Vertex3(lTriangle.Point1.x, lTriangle.Point1.y, lTriangle.Point1.z);
                GL.Vertex3(lTriangle.Point2.x, lTriangle.Point2.y, lTriangle.Point2.z);
                GL.Vertex3(lTriangle.Point3.x, lTriangle.Point3.y, lTriangle.Point3.z);
                GL.End();

                GL.PopMatrix();
            }
        }

        /// <summary>
        /// Draws any text that needs to be draw
        /// </summary>
        private static void RenderText()
        {
            for (int i = 0; i < mText.Count; i++)
            {
                Text lText = mText[i];

                int lWidth = lText.Texture.width;
                int lHeight = lText.Texture.height;
                Vector2 lScreenPoint = Camera.main.WorldToScreenPoint(lText.Position);
                Rect lScreenRect = new Rect(lScreenPoint.x - (lWidth * 0.5f), Screen.height - lScreenPoint.y - (lHeight * 0.5f), lWidth, lHeight);

                //UnityEngine.Graphics.DrawTexture(lScreenRect, lText.Texture);
                UnityEngine.GUI.DrawTexture(lScreenRect, lText.Texture);
            }
        }

        /// <summary>
        /// Creates the material we'll render with
        /// </summary>
        private static void CreateMaterials()
        {
            if (mSimpleMaterial != null) { return; }

            // Unity has a built-in shader that is useful for drawing simple colored things.
            //Shader lShader = Shader.Find("Hidden/Internal-Colored");
            //Shader lShader = Shader.Find("Hidden/GraphicsManagerUI");
            Shader lShader = Shader.Find(mShader);
            if (lShader == null) { lShader = Shader.Find("Hidden/GraphicsManagerUI"); }

            mSimpleMaterial = new Material(lShader);
            mSimpleMaterial.hideFlags = HideFlags.HideAndDontSave;

            // Turn on alpha blending
            mSimpleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mSimpleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

            // Turn backface culling off
            mSimpleMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            mSimpleMaterial.SetInt("_ZWrite", 0);
        }

        /// <summary>
        /// If the pixels are cached, grab them. Otherwise, create the cache
        /// </summary>
        /// <param name="rFont"></param>
        /// <param name="rCharacter"></param>
        /// <returns></returns>
        private static TextCharacter GetCharacterPixels(Font rFont, char rCharacter)
        {
            // If we already grabbed the pixesl, this is easy
            if (!mFonts.ContainsKey(rFont)) { return null; }
            if (mFonts[rFont].Characters.ContainsKey(rCharacter))
            {
                return mFonts[rFont].Characters[rCharacter];
            }

            // Grab our font's source texture
            Texture2D lFontTexture = mFonts[rFont].Texture;

            // Process each character in the text
            int x, y, w, h;
            CharacterInfo lInfo;
            Vector2 lGlyphBottomLeft = Vector2.zero;

            Color[] lPixels = null;
            rFont.GetCharacterInfo(rCharacter, out lInfo);

            // Rotate 90-counter-clockwise needed
            if (lInfo.uvBottomLeft.x == lInfo.uvBottomRight.x)
            {
                // No flip needed
                if (lInfo.uvBottomLeft.y > lInfo.uvBottomRight.y)
                {
                    lGlyphBottomLeft = lInfo.uvBottomRight;
                }
                else
                {
                    lGlyphBottomLeft = lInfo.uvBottomLeft;
                }
            }
            // No rotate needed
            else
            {
                // Flip needed
                if (lInfo.uvBottomLeft.y > lInfo.uvTopLeft.y)
                {
                    lGlyphBottomLeft = lInfo.uvTopLeft;
                }
                // No flip needed
                else
                {
                    lGlyphBottomLeft = lInfo.uvBottomLeft;
                }
            }

            x = (int)((float)lFontTexture.width * lGlyphBottomLeft.x) + 0;
            y = (int)((float)lFontTexture.height * lGlyphBottomLeft.y) + 0;
            w = lInfo.glyphWidth;
            h = lInfo.glyphHeight;

            // We need to rotate the pixels
            if (lInfo.uvBottomLeft.x == lInfo.uvBottomRight.x)
            {
                if (lInfo.uvBottomLeft.y > lInfo.uvBottomRight.y)
                {
                    lPixels = lFontTexture.GetPixels(x, y, h, w);
                    lPixels = RotatePixelsLeft(lPixels, h, w);
                }
            }

            // We need to flip the array
            if (lInfo.uvBottomLeft.y > lInfo.uvTopLeft.y)
            {
                lPixels = lFontTexture.GetPixels(x, y, w, h);
                lPixels = FlipPixelsVertically(lPixels, w, h);
            }

            // We need to mirror the array
            if (lInfo.uvTopLeft.x > lInfo.uvTopRight.x)
            {
                lPixels = lFontTexture.GetPixels(x, y, w, h);
                lPixels = FlipPixelsHorizontally(lPixels, w, h);
            }

            // Add the character pixels since they didn't exist in the beginning
            TextCharacter lTextCharacter = TextCharacter.Allocate();
            lTextCharacter.Character = rCharacter;
            lTextCharacter.Pixels = lPixels;
            lTextCharacter.MinX = lInfo.minX;
            lTextCharacter.MinY = lInfo.minY;
            lTextCharacter.Width = w;
            lTextCharacter.Height = h;
            lTextCharacter.Advance = lInfo.advance;

            mFonts[rFont].Characters.Add(rCharacter, lTextCharacter);

            // Return the pixels
            return lTextCharacter;
        }

        /// <summary>
        /// Rotates the array 90-degrees counter-clockwise
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] RotatePixelsLeft(Color[] rArray, int rWidth, int rHeight)
        {
            Color[] lNewArray = new Color[rArray.Length];

            for (int i = 0; i < rArray.Length; i++)
            {
                int lRow = i / rWidth;
                int lCol = i % rWidth;

                int lNewRow = lCol;
                int lNewCol = rHeight - lRow - 1;

                int lIndex = (lNewRow * rHeight) + lNewCol;
                lNewArray[lIndex] = rArray[i];
            }

            return lNewArray;
        }

        /// <summary>
        /// Flips the array horizontally, but not vertically
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] FlipPixelsHorizontally(Color[] rArray, int rWidth, int rHeight)
        {
            Color lColor;
            Color[] lNewArray = new Color[rArray.Length];

            for (int y = 0; y < rHeight; y++)
            {
                for (int x = 0; x < rWidth; x++)
                {
                    lColor = rArray[y * rWidth + x];
                    lNewArray[(rWidth - 1 - x) * rHeight + y] = lColor;
                }
            }

            return lNewArray;
        }

        /// <summary>
        /// Flips the array vertically, but not horizontally
        /// </summary>
        /// <param name="rArray"></param>
        /// <param name="rWidth"></param>
        /// <param name="rHeight"></param>
        /// <returns></returns>
        private static Color[] FlipPixelsVertically(Color[] rArray, int rWidth, int rHeight)
        {
            int lRow;
            int lTargetRow;
            int lTargetRowStart;

            Color[] lNewArray = new Color[rArray.Length];

            for (int i = 0; i < rArray.Length;)
            {
                lRow = i / rWidth;
                lTargetRow = rHeight - lRow;
                lTargetRowStart = (lTargetRow - 1) * rWidth;

                for (int j = lTargetRowStart; j < lTargetRowStart + rWidth; j++, i++)
                {
                    lNewArray[j] = rArray[i];
                }
            }

            return lNewArray;
        }

        /// <summary>
        /// Support class for a 8 sided polygon
        /// </summary>
        public class Octahedron
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public Octahedron()
            {
                Vertices = CreateVertices();
                Triangles = CreateTriangles();

                // We want the edges of the polygon to look crisp. So,
                // we're going to create individual vertices for each index
                Vector3[] lNewVertices = new Vector3[Triangles.Length];
                for (int i = 0; i < Triangles.Length; i++)
                {
                    lNewVertices[i] = Vertices[Triangles[i]];
                    Triangles[i] = i;
                }

                Vertices = lNewVertices;
            }

            private Vector3[] CreateVertices()
            {
                int lStride = 3;

                float[] lVerticesFloat = new float[] { 0.000000f, 0.500000f, 0.000000f, 0.500000f, 0.000000f, 0.000000f, 0.000000f, 0.000000f, -0.500000f, -0.500000f, 0.000000f, 0.000000f, 0.000000f, -0.000000f, 0.500000f, 0.000000f, -0.500000f, -0.000000f };

                Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
                for (int i = 0; i < lVerticesFloat.Length; i += lStride)
                {
                    lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
                }

                return lVertices;
            }

            private int[] CreateTriangles()
            {
                int[] lIndexes = { 1, 2, 0, 2, 3, 0, 3, 4, 0, 0, 4, 1, 5, 2, 1, 5, 3, 2, 5, 4, 3, 5, 1, 4 };
                return lIndexes;
            }
        }

        /// <summary>
        /// Class that is used to help us create spheres
        /// </summary>
        private class Icosahedron
        {
            public Vector3[] Vertices;
            public int[] Triangles;

            public Icosahedron()
            {
                Vertices = CreateVertices();
                Triangles = CreateTriangles();
            }

            private Vector3[] CreateVertices()
            {
                Vector3[] vertices = new Vector3[12];

                float lHalfSize = 0.5f;
                float a = (lHalfSize + Mathf.Sqrt(5)) / 2.0f;

                vertices[0] = new Vector3(a, 0.0f, lHalfSize);
                vertices[9] = new Vector3(-a, 0.0f, lHalfSize);
                vertices[11] = new Vector3(-a, 0.0f, -lHalfSize);
                vertices[1] = new Vector3(a, 0.0f, -lHalfSize);
                vertices[2] = new Vector3(lHalfSize, a, 0.0f);
                vertices[5] = new Vector3(lHalfSize, -a, 0.0f);
                vertices[10] = new Vector3(-lHalfSize, -a, 0.0f);
                vertices[8] = new Vector3(-lHalfSize, a, 0.0f);
                vertices[3] = new Vector3(0.0f, lHalfSize, a);
                vertices[7] = new Vector3(0.0f, lHalfSize, -a);
                vertices[6] = new Vector3(0.0f, -lHalfSize, -a);
                vertices[4] = new Vector3(0.0f, -lHalfSize, a);

                for (int i = 0; i < 12; i++)
                {
                    vertices[i].Normalize();
                }

                return vertices;
            }

            private int[] CreateTriangles()
            {
                int[] lTriangles = {
                1,2,0,
                2,3,0,
                3,4,0,
                4,5,0,
                5,1,0,
                6,7,1,
                2,1,7,
                7,8,2,
                2,8,3,
                8,9,3,
                3,9,4,
                9,10,4,
                10,5,4,
                10,6,5,
                6,1,5,
                6,11,7,
                7,11,8,
                8,11,9,
                9,11,10,
                10,11,6,
            };

                return lTriangles;
            }
        }
    }
}