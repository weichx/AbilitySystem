using System.Collections.Generic;
using UnityEngine;

namespace com.ootii.Utilities.Debug
{
    public class DebugDraw
    {
        /// <summary>
        /// Custom material we'll draw debug meshes with
        /// </summary>
        private static Material sMaterial;

        /// <summary>
        /// Custom material we'll draw debug meshes with
        /// </summary>
        private static Material sOverlayMaterial;

        /// <summary>
        /// Used to pass property info to the material
        /// </summary>
        private static MaterialPropertyBlock sMaterialBlock;

        /// <summary>
        /// Lines we can collect and draw at once
        /// </summary>
        private static List<Vector3> sLines = new List<Vector3>();

        /// <summary>
        /// Vertices used to create a 3D line
        /// </summary>
        private static Vector3[] sLineVertices = new Vector3[8];

        /// <summary>
        /// Simple meshes to render
        /// </summary>
        private static Mesh sLine;
        private static Mesh sDisk;
        private static Mesh sTetrahedron;
        private static Mesh sCube;
        private static Mesh sOctahedron;
        private static Mesh sDodecahedron;
        private static Mesh sIcosahedron;
        private static Mesh sSphere;
        private static Mesh sBone;

        /// <summary>
        /// Constructor
        /// </summary>
        static DebugDraw()
        {
            //Initialize();
        }

        /// <summary>
        /// Function that initializes all of our static objects. This is needed especially since
        /// the objects can get destroyed as we move in and out of play mode.
        /// </summary>
        public static void Initialize()
        {
            sDisk = CreateDisk();
            sTetrahedron = CreateTetrahedron();
            sCube = CreateCube();
            sOctahedron = CreateOctahedron();
            sDodecahedron = CreateDodecahedron();
            sIcosahedron = CreateIcosahedron();
            sSphere = CreateSphere();
            sBone = CreateBone();

            for (int i = 0; i < sLineVertices.Length; i++)
            {
                sLineVertices[i] = Vector3.zero;
            }

            int[] lTriangles = { 3, 2, 0, 3, 0, 1, 3, 5, 2, 2, 5, 4, 7, 6, 4, 7, 4, 5, 1, 0, 6, 1, 6, 7, 3, 1, 5, 1, 7, 5, 2, 6, 0, 6, 2, 4 };

            sLine = new Mesh();
            sLine.vertices = sLineVertices;
            sLine.triangles = lTriangles;

#if UNITY_4_6
            sOverlayMaterial = new Material(
            @"Shader ""Custom/Draw"" 
            {
                Properties 
                {
                    _Color (""Main Color"", COLOR) = (1, 1, 1, 1)
                    _Emission (""Emmisive Color"", Color) = (0.5, 0.5, 0.5, 0)
                }

                SubShader 
                {
                    Pass 
                    {
                        Material 
                        {
                            Diffuse [_Color]
                            Emission [_Emission]
                        }

                        Lighting On
                        Blend SrcAlpha OneMinusSrcAlpha
                        ZTest Always
                        ZWrite Off
                    }
                }
            }");
#else
            sOverlayMaterial = new Material(Shader.Find("Standard"));
#endif

            // Don't set the hide flags or we get odd behavior like multiple
            // material renders at the same time for the same object
            //sOverlayMaterial.hideFlags = HideFlags.HideAndDontSave;

#if UNITY_4_6
            sMaterial = new Material(
                        @"Shader ""Custom/Draw"" 
            {
                Properties 
                {
                    _Color (""Main Color"", COLOR) = (1, 1, 1, 1)
                    _Emission (""Emmisive Color"", Color) = (0.5, 0.5, 0.5, 0)
                }

                SubShader 
                {
                    Pass 
                    {
                        Material 
                        {
                            Diffuse [_Color]
                            Emission [_Emission]
                        }

                        Lighting On
                        Blend SrcAlpha OneMinusSrcAlpha
                    }
                }
            }");
#else
            sMaterial = new Material(Shader.Find("Standard"));
#endif

            // Don't set the hide flags or we get odd behavior like multiple
            // material renders at the same time for the same object
            //sMaterial.hideFlags = HideFlags.HideAndDontSave;

            sMaterialBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Invalidate the draw elements so they are recreated
        /// </summary>
        public static void Invalidate()
        {
            sMaterial = null;
            sOverlayMaterial = null;
        }

        /// <summary>
        /// Draws a sphere
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        //public static void DrawSphere(Vector3 rCenter, float rRadius, Color rColor, bool rWireframe)
        //{
        //    Color lOldColor = Gizmos.color;
        //    Matrix4x4 lOldMatrix = Gizmos.matrix;

        //    Gizmos.color = rColor;
        //    Gizmos.matrix = Matrix4x4.identity;

        //    if (rWireframe)
        //    {
        //        Gizmos.DrawWireSphere(rCenter, rRadius);
        //    }
        //    else
        //    {
        //        Gizmos.DrawSphere(rCenter, rRadius);
        //    }

        //    Gizmos.color = lOldColor;
        //    Gizmos.matrix = lOldMatrix;
        //}

        /// <summary>
        /// Draws a cube
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        /// </summary>
        public static void DrawCube(Vector3 rCenter, Vector3 rSize, Color rColor, bool rWireframe)
        {
            Vector3 lFrom = rCenter;
            Vector3 lTo = rCenter;
            Vector3 lHalfSize = rSize * 0.5f;

            // bottom
            DrawLine(lFrom + new Vector3(-lHalfSize.x, -lHalfSize.y, -lHalfSize.z), lTo + new Vector3( lHalfSize.x, -lHalfSize.y, -lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3(-lHalfSize.x, -lHalfSize.y, -lHalfSize.z), lTo + new Vector3(-lHalfSize.x, -lHalfSize.y,  lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3( lHalfSize.x, -lHalfSize.y, -lHalfSize.z), lTo + new Vector3( lHalfSize.x, -lHalfSize.y,  lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3(-lHalfSize.x, -lHalfSize.y,  lHalfSize.z), lTo + new Vector3( lHalfSize.x, -lHalfSize.y,  lHalfSize.z), rColor);

            // top
            DrawLine(lFrom + new Vector3(-lHalfSize.x,  lHalfSize.y, -lHalfSize.z), lTo + new Vector3( lHalfSize.x,  lHalfSize.y, -lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3(-lHalfSize.x,  lHalfSize.y, -lHalfSize.z), lTo + new Vector3(-lHalfSize.x,  lHalfSize.y,  lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3( lHalfSize.x,  lHalfSize.y, -lHalfSize.z), lTo + new Vector3( lHalfSize.x,  lHalfSize.y,  lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3(-lHalfSize.x,  lHalfSize.y,  lHalfSize.z), lTo + new Vector3( lHalfSize.x,  lHalfSize.y,  lHalfSize.z), rColor);

            // sides
            DrawLine(lFrom + new Vector3(-lHalfSize.x, -lHalfSize.y, -lHalfSize.z), lTo + new Vector3(-lHalfSize.x,  lHalfSize.y, -lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3(-lHalfSize.x, -lHalfSize.y,  lHalfSize.z), lTo + new Vector3(-lHalfSize.x,  lHalfSize.y,  lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3( lHalfSize.x, -lHalfSize.y, -lHalfSize.z), lTo + new Vector3( lHalfSize.x,  lHalfSize.y, -lHalfSize.z), rColor);
            DrawLine(lFrom + new Vector3( lHalfSize.x, -lHalfSize.y,  lHalfSize.z), lTo + new Vector3( lHalfSize.x,  lHalfSize.y,  lHalfSize.z), rColor);
        }

        //public static void DrawCube(Vector3 rCenter, Vector3 rSize, Color rColor, bool rWireframe)
        //{
        //    Color lOldColor = Gizmos.color;
        //    Matrix4x4 lOldMatrix = Gizmos.matrix;

        //    Gizmos.color = rColor;
        //    Gizmos.matrix = Matrix4x4.identity;

        //    if (rWireframe)
        //    {
        //        Gizmos.DrawWireCube(rCenter, rSize);
        //    }
        //    else
        //    {
        //        Gizmos.DrawCube(rCenter, rSize);
        //    }

        //    Gizmos.color = lOldColor;
        //    Gizmos.matrix = lOldMatrix;
        //}

        /// <summary>
        /// Draws a horizontal circle
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawCircle(Vector3 rCenter, float rRadius, Color rColor)
        {
            DrawArc(rCenter, Quaternion.identity, 0, 360, rRadius, rColor);
        }

        /// <summary>
        /// Draws a sphere with 3 arcs
        /// </summary>
        public static void DrawWireSphere(Vector3 rCenter, float rRadius, Color rColor)
        {
            DrawArc(rCenter, Quaternion.identity, 0, 360, rRadius, rColor);
            DrawArc(rCenter, Quaternion.AngleAxis(90, Vector3.right), 0, 360, rRadius, rColor);
            DrawArc(rCenter, Quaternion.AngleAxis(90, Vector3.forward), 0, 360, rRadius, rColor);
        }

        /// <summary>
        /// Draws a horizontal circle
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rColor"></param>
        public static void DrawArc(Vector3 rCenter, Quaternion rRotation, float rMinAngle, float rMaxAngle, float rRadius, Color rColor)
        {
            sLines.Clear();

            float lStep = 10f;
            Vector3 lPoint = Vector3.zero;

            // The circle wants to go from the +X direction and
            // rotate counter-clockwise as the theta increase.
            // Logically, I think +Z should be forward and 
            // it should rotate clockwise as theta increases. Hence the 'TrueTheta'.
            for (float lTheta = rMinAngle; lTheta <= rMaxAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = rRadius * Mathf.Cos(lTrueTheta);
                lPoint.y = 0;
                lPoint.z = rRadius * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            Matrix4x4 lMatrix = Matrix4x4.TRS(rCenter, rRotation, Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            // Draw out the lines
            DrawLines(sLines, rColor);
        }

        /// <summary>
        /// Draw a simple frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        //public static void DrawFrustum(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rDistance, Color rColor)
        //{
        //    Color lOldColor = Gizmos.color;
        //    Matrix4x4 lOldMatrix = Gizmos.matrix;

        //    Gizmos.color = rColor;
        //    Gizmos.matrix = Matrix4x4.TRS(rPosition, rRotation, Vector3.one);
        //    Gizmos.DrawFrustum(Vector3.zero, rVAngle, rDistance, 0.1f, rHAngle / rVAngle);

        //    Gizmos.color = lOldColor;
        //    Gizmos.matrix = lOldMatrix;
        //}

        /// <summary>
        /// Draws a frustum
        /// </summary>
        /// <param name="rPosition"></param>
        /// <param name="rRotation"></param>
        /// <param name="rHAngle"></param>
        /// <param name="rVAngle"></param>
        /// <param name="rDistance"></param>
        /// <param name="rColor"></param>
        public static void DrawFrustumArc(Vector3 rPosition, Quaternion rRotation, float rHAngle, float rVAngle, float rDistance, Color rColor)
        {
            float lStep = 10f;
            Vector3 lPoint = Vector3.zero;

            float lHalfHAngle = rHAngle * 0.5f;
            float lHalfVAngle = rVAngle * 0.5f;

            List<Vector3> lCenters = new List<Vector3>(2) { Vector3.zero, Vector3.zero };
            List<Vector3> lLeftCorners = new List<Vector3>(5) { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
            List<Vector3> lRightCorners = new List<Vector3>(5) { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

            // bottom close arc 
            sLines.Clear();
            for (float lTheta = -lHalfHAngle; lTheta <= lHalfHAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = 1.0f * Mathf.Cos(lTrueTheta);
                lPoint.y = 0;
                lPoint.z = 1.0f * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation * Quaternion.AngleAxis(lHalfVAngle, Vector3.right), Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            lLeftCorners[1] = sLines[0]; // bottom close left
            lRightCorners[1] = sLines[sLines.Count - 1]; // bottom close right

            // Draw out the lines
            DrawLines(sLines, rColor);

            // Bottom far arc 
            sLines.Clear();
            for (float lTheta = -lHalfHAngle; lTheta <= lHalfHAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = rDistance * Mathf.Cos(lTrueTheta);
                lPoint.y = 0;
                lPoint.z = rDistance * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            lMatrix = Matrix4x4.TRS(rPosition, rRotation * Quaternion.AngleAxis(lHalfVAngle, Vector3.right), Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            lLeftCorners[0] = sLines[0]; // bottom far left
            lLeftCorners[4] = sLines[0]; // bottom far left

            lRightCorners[0] = sLines[sLines.Count - 1]; // bottom far right
            lRightCorners[4] = sLines[sLines.Count - 1]; // bottom far right

            lCenters[0] = sLines[sLines.Count / 2]; // bottom far center

            // Draw out the lines
            DrawLines(sLines, rColor);

            // Top close arc 
            sLines.Clear();
            for (float lTheta = -lHalfHAngle; lTheta <= lHalfHAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = 1.0f * Mathf.Cos(lTrueTheta);
                lPoint.y = 0;
                lPoint.z = 1.0f * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            lMatrix = Matrix4x4.TRS(rPosition, rRotation * Quaternion.AngleAxis(-lHalfVAngle, Vector3.right), Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            lLeftCorners[2] = sLines[0]; // top close left
            lRightCorners[2] = sLines[sLines.Count - 1]; // top close right

            // Draw out the lines
            DrawLines(sLines, rColor);

            // Top far arc 
            sLines.Clear();
            for (float lTheta = -lHalfHAngle; lTheta <= lHalfHAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = rDistance * Mathf.Cos(lTrueTheta);
                lPoint.y = 0;
                lPoint.z = rDistance * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            lMatrix = Matrix4x4.TRS(rPosition, rRotation * Quaternion.AngleAxis(-lHalfVAngle, Vector3.right), Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            lLeftCorners[3] = sLines[0]; // top far left
            lRightCorners[3] = sLines[sLines.Count - 1]; // top far right
            lCenters[1] = sLines[sLines.Count / 2]; // top far center

            // Draw out the lines
            DrawLines(sLines, rColor);

            // Center vertical close arch
            sLines.Clear();
            for (float lTheta = -lHalfVAngle; lTheta <= lHalfVAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = 0f;
                lPoint.y = 1.0f * Mathf.Cos(lTrueTheta);
                lPoint.z = 1.0f * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            lMatrix = Matrix4x4.TRS(rPosition, rRotation, Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            // Draw out the lines
            DrawLines(sLines, rColor);

            // Center vertical close arch
            sLines.Clear();
            for (float lTheta = -lHalfVAngle; lTheta <= lHalfVAngle; lTheta += lStep)
            {
                float lTrueTheta = -(lTheta * Mathf.Deg2Rad) + 1.57079f;
                lPoint.x = 0f;
                lPoint.y = rDistance * Mathf.Cos(lTrueTheta);
                lPoint.z = rDistance * Mathf.Sin(lTrueTheta);
                sLines.Add(lPoint);
            }

            // Transform the points based on the center and rotation
            lMatrix = Matrix4x4.TRS(rPosition, rRotation, Vector3.one);
            for (int i = 0; i < sLines.Count; i++) { sLines[i] = lMatrix.MultiplyPoint3x4(sLines[i]); }

            // Draw out the lines
            DrawLines(sLines, rColor);
            // Draw out the ends
            DrawLines(lLeftCorners, rColor);
            DrawLines(lRightCorners, rColor);
        }

        /// <summary>
        /// Draw a simple colored line
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rColor"></param>
        //public static void DrawLine(Vector3 rFrom, Vector3 rTo, Color rColor)
        //{
        //    Color lOldColor = Gizmos.color;
        //    Matrix4x4 lOldMatrix = Gizmos.matrix;

        //    Gizmos.color = rColor;
        //    Gizmos.matrix = Matrix4x4.identity;
        //    Gizmos.DrawLine(rFrom, rTo);

        //    Gizmos.color = lOldColor;
        //    Gizmos.matrix = lOldMatrix;
        //}

        /// <summary>
        /// Drawas a list of connected lines where i0->i1->i2 etc.
        /// </summary>
        /// <param name="rLines"></param>
        /// <param name="rColor"></param>
        public static void DrawLines(List<Vector3> rLines, Color rColor)
        {
            //Color lOldColor = Gizmos.color;
            //Matrix4x4 lOldMatrix = Gizmos.matrix;


            //Gizmos.color = rColor;
            //Gizmos.matrix = Matrix4x4.identity;

            for (int i = 1; i < rLines.Count; i++)
            {
                //Gizmos.DrawLine(rLines[i - 1], rLines[i]);
                UnityEngine.Debug.DrawLine(rLines[i - 1], rLines[i], rColor);
            }

            //Gizmos.color = lOldColor;
            //Gizmos.matrix = lOldMatrix;
        }

        /// <summary>
        /// Draw a simple colored line
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rColor"></param>
        public static void DrawLine(Vector3 rFrom, Vector3 rTo, Color rColor)
        {
            UnityEngine.Debug.DrawLine(rFrom, rTo, rColor);
        }

        public static void DrawLine(Vector3 rFrom, Vector3 rTo, float rThickness, Color rColor, float rAlpha)
        {
            if (sCube == null || sMaterial == null) { Initialize(); }

            Vector3 lPosition = new Vector3(rFrom.x + ((rTo.x - rFrom.x) / 2f), rFrom.y + ((rTo.y - rFrom.y) / 2f), rFrom.z + ((rTo.z - rFrom.z) / 2f));
            Quaternion lRotation = Quaternion.FromToRotation(Vector3.right, (rTo - rFrom).normalized);
            Vector3 lSize = new Vector3(Vector3.Distance(rFrom, rTo), rThickness, rThickness);

            Matrix4x4 lMatrix = Matrix4x4.TRS(lPosition, lRotation, lSize);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sCube, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draw a set of simple colored line
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rColor"></param>
        //public static void DrawLines(List<Vector3> rLines, float rThickness, Color rColor, float rAlpha)
        //{
        //    for (int i = 1; i < rLines.Count; i++)
        //    {
        //        DrawLine(rLines[i - 1], rLines[i], rThickness, rColor, rAlpha);
        //    }
        //}

        /// <summary>
        /// Draw a simple colored line
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rColor"></param>
        public static void DrawLineOverlay(Vector3 rFrom, Vector3 rTo, float rThickness, Color rColor, float rAlpha)
        {
            if (sCube == null || sOverlayMaterial == null) { Initialize(); }

            Vector3 lPosition = new Vector3(rFrom.x + ((rTo.x - rFrom.x) / 2f), rFrom.y + ((rTo.y - rFrom.y) / 2f), rFrom.z + ((rTo.z - rFrom.z) / 2f));
            Quaternion lRotation = Quaternion.FromToRotation(Vector3.right, (rTo - rFrom).normalized);
            Vector3 lSize = new Vector3(Vector3.Distance(rFrom, rTo), rThickness, rThickness);

            Matrix4x4 lMatrix = Matrix4x4.TRS(lPosition, lRotation, lSize);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sCube, lMatrix, sOverlayMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual cube mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawTetrahedronMesh(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sTetrahedron == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sTetrahedron, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual cube mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawCubeMesh(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sCube == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sCube, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual Octahedron mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawOctahedronMesh(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sOctahedron == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sOctahedron, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual Octahedron mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawOctahedronOverlay(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sOctahedron == null || sOverlayMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sOctahedron, lMatrix, sOverlayMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual Octahedron mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawDodecahedronMesh(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sDodecahedron == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sDodecahedron, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawIcosahedronMesh(Vector3 rPosition, Quaternion rRotation, float rSize, Color rColor, float rAlpha)
        {
            if (sIcosahedron == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, rSize * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sIcosahedron, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual sphere mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void DrawSphereMesh(Vector3 rPosition, float rRadius, Color rColor, float rAlpha)
        {
            if (sSphere == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, Quaternion.identity, rRadius * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sSphere, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual Octahedron mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawSphereOverlay(Vector3 rPosition, float rRadius, Color rColor, float rAlpha)
        {
            if (sSphere == null || sOverlayMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, Quaternion.identity, rRadius * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sSphere, lMatrix, sOverlayMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual cube mesh
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public static void DrawDiskMesh(Vector3 rPosition, Quaternion rRotation, float rRadius, Color rColor, float rAlpha)
        {
            if (sDisk == null || sMaterial == null) { Initialize(); }

            Matrix4x4 lMatrix = Matrix4x4.TRS(rPosition, rRotation, (rRadius * 2) * Vector3.one);

            Color lColor = rColor;
            lColor.a = rAlpha;

            Color lEmission = rColor * 0.5f;
            lEmission.a = rAlpha;

            sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            sMaterialBlock.AddColor("_Color", lColor);
            sMaterialBlock.AddColor("_Emission", lEmission);
#else
            sMaterialBlock.SetColor("_Color", lColor);
            sMaterialBlock.SetColor("_Emission", lEmission);
#endif

            UnityEngine.Graphics.DrawMesh(sDisk, lMatrix, sMaterial, 0, null, 0, sMaterialBlock);
        }

        /// <summary>
        /// Draws an actual bone mesh
        /// </summary>
        /// <param name="rBoneTransform"></param>
        /// <param name="rColor"></param>
        /// <param name="rAlpha"></param>
        public static void DrawBoneMesh(Transform rBoneTransform, Color rColor, float rAlpha)
        {
            if (rBoneTransform == null) { return; }
            if (sBone == null || sOverlayMaterial == null) { Initialize(); }

            float lSize = 0.02f;

            // Always draw at least one bone
            int lChildCount = Mathf.Max(rBoneTransform.childCount, 1);
            for (int i = 0; i < lChildCount; i++)
            {
                Quaternion lRotation = rBoneTransform.rotation;

                // If we have a child, we can use it to determine the length
                if (rBoneTransform.childCount > i)
                {
                    Transform lChildBone = rBoneTransform.GetChild(i);
                    lSize = Vector3.Distance(rBoneTransform.position, lChildBone.position);
                    lRotation = Quaternion.FromToRotation(Vector3.up, lChildBone.position - rBoneTransform.position);
                }

                // Render the bone position
                Matrix4x4 lMatrix = Matrix4x4.TRS(rBoneTransform.position, lRotation, lSize * Vector3.one);

                Color lColor = rColor;
                lColor.a = rAlpha;

                Color lEmission = rColor * 0.5f;
                lEmission.a = rAlpha;

                sMaterialBlock.Clear();

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
                sMaterialBlock.AddColor("_Color", lColor);
                sMaterialBlock.AddColor("_Emission", lEmission);
#else
                sMaterialBlock.SetColor("_Color", lColor);
                sMaterialBlock.SetColor("_Emission", lEmission);
#endif

                UnityEngine.Graphics.DrawMesh(sBone, lMatrix, sOverlayMaterial, 0, null, 0, sMaterialBlock);
            }
        }

        /// <summary>
        /// Draws the full skeleton
        /// </summary>
        /// <param name="rRootTransform"></param>
        /// <param name="rColor"></param>
        /// <param name="rAlpha"></param>
        public static void DrawSkeleton(Transform rRootTransform, Color rColor, float rAlpha)
        {
            if (rRootTransform == null) { return; }

            DrawBoneMesh(rRootTransform, rColor, rAlpha);

            for (int i = 0; i < rRootTransform.childCount; i++)
            {
                DrawSkeleton(rRootTransform.GetChild(i), rColor, rAlpha);
            }
        }

        /// <summary>
        /// Draws the full skeleton with some bones being colored a specific color
        /// </summary>
        /// <param name="rRootTransform"></param>
        /// <param name="rColor"></param>
        /// <param name="rAlpha"></param>
        /// <param name="rDrawAxis"></param>
        /// <param name="rSelectedBones"></param>
        /// <param name="rSelectedColor"></param>
        public static void DrawSkeleton(Transform rRootTransform, Color rColor, float rAlpha, bool rDrawAxis, List<Transform> rSelectedBones, Color rSelectedColor)
        {
            if (rRootTransform == null) { return; }

            if (rSelectedBones != null && rSelectedBones.IndexOf(rRootTransform) >= 0)
            {
                DrawBoneMesh(rRootTransform, rSelectedColor, 1f);
            }
            else
            {
                DrawBoneMesh(rRootTransform, rColor, rAlpha);
            }

            for (int i = 0; i < rRootTransform.childCount; i++)
            {
                DrawSkeleton(rRootTransform.GetChild(i), rColor, rAlpha, rDrawAxis, rSelectedBones, rSelectedColor);
            }
        }

        /// <summary>
        /// Draws the full skeleton
        /// </summary>
        /// <param name="rRootTransform"></param>
        /// <param name="rColor"></param>
        /// <param name="rAlpha"></param>
        public static void DrawHumanoidSkeleton(GameObject rObject, Color rColor, float rAlpha)
        {
            Animator lAnimator = rObject.GetComponent<Animator>();
            if (lAnimator == null) { return; }

            string[] lUnityBones = System.Enum.GetNames(typeof(HumanBodyBones));

            for (int i = 0; i < lUnityBones.Length; i++)
            {
                Transform lBone = lAnimator.GetBoneTransform((HumanBodyBones)i);
                if (lBone != null) { DrawBoneMesh(lBone, rColor, rAlpha); }
            }
        }

        /// <summary>
        /// Renders out a transform object so we can see where an object is positioned an how it's oriented.
        /// </summary>
        /// <param name="rTransform"></param>
        /// <param name="rSize"></param>
        public static void DrawTransform(Transform rTransform, float rSize)
        {
            Vector3 lPosition = rTransform.position;
            Quaternion lRotation = rTransform.rotation;
            DebugDraw.DrawLineOverlay(lPosition, lPosition + (lRotation * Vector3.right * rSize), 0.002f, Color.red, 1f);
            DebugDraw.DrawLineOverlay(lPosition, lPosition + (lRotation * Vector3.up * rSize), 0.002f, Color.green, 1f);
            DebugDraw.DrawLineOverlay(lPosition, lPosition + (lRotation * Vector3.forward * rSize), 0.002f, Color.blue, 1f);
        }

        /// <summary>
        /// Renders out a transform object so we can see where an object is positioned an how it's oriented.
        /// </summary>
        /// <param name="rTransform"></param>
        /// <param name="rSize"></param>
        public static void DrawTransform(Vector3 rPosition, Quaternion rRotation, float rSize)
        {
            DebugDraw.DrawLineOverlay(rPosition, rPosition + (rRotation * Vector3.right * rSize), 0.002f, Color.red, 1f);
            DebugDraw.DrawLineOverlay(rPosition, rPosition + (rRotation * Vector3.up * rSize), 0.002f, Color.green, 1f);
            DebugDraw.DrawLineOverlay(rPosition, rPosition + (rRotation * Vector3.forward * rSize), 0.002f, Color.blue, 1f);
        }

        /// <summary>
        /// Creates a 4 sided polygon
        /// </summary>
        /// <returns>Mesh that is the Icosahedron</returns>
        public static Mesh CreateTetrahedron()
        {
            Tetrahedron lShape = new Tetrahedron();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Create a 6 sided polygon
        /// </summary>
        /// <returns>Mesh that is the cube</returns>
        public static Mesh CreateCube()
        {
            Cube lShape = new Cube();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Create an 8 sided polygon
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateOctahedron()
        {
            Octahedron lShape = new Octahedron();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Create an 12 sided polygon
        /// </summary>
        /// <returns></returns>
        public static Mesh CreateDodecahedron()
        {
            Dodecahedron lShape = new Dodecahedron();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Creates a 20 sided polygon
        /// </summary>
        /// <returns>Mesh that is the Icosahedron</returns>
        public static Mesh CreateIcosahedron()
        {
            Icosahedron lShape = new Icosahedron();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Create a simple cube
        /// </summary>
        /// <returns>Mesh that is the cube</returns>
        public static Mesh CreateSphere()
        {
            return IcoSphere.CreateSphere(4);
        }

        /// <summary>
        /// Creates a flat circle
        /// </summary>
        /// <returns>Mesh that is the Icosahedron</returns>
        public static Mesh CreateDisk()
        {
            Disk lShape = new Disk();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        /// <summary>
        /// Creates a six sided bone
        /// </summary>
        /// <returns>Mesh that is the Icosahedron</returns>
        public static Mesh CreateBone()
        {
            Bone lShape = new Bone();

            Mesh lMesh = new Mesh();
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = lShape.Vertices;
            lMesh.triangles = lShape.Triangles;
            lMesh.RecalculateNormals();

            return lMesh;
        }
    }

    /// <summary>
    /// Support class for a 4 sided polygon
    /// </summary>
    public class Tetrahedron
    {
        public Vector3[] Vertices;
        public int[] Triangles;

        public Tetrahedron()
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

            float[] lVerticesFloat = new float[] { -0.352500f, -0.498510f, -0.610548f, -0.352500f, -0.498510f, 0.610548f, 0.705000f, -0.498510f, -0.000000f, 0.000000f, 0.498510f, 0.000000f };

            Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
            for (int i = 0; i < lVerticesFloat.Length; i += lStride)
            {
                lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
            }

            return lVertices;
        }

        private int[] CreateTriangles()
        {
            int[] lIndexes = { 2, 1, 0, 2, 3, 1, 3, 2, 0, 1, 3, 0 };
            return lIndexes;
        }
    }

    /// <summary>
    /// Support class for a 6 sided polygon
    /// </summary>
    public class Cube
    {
        public Vector3[] Vertices;
        public int[] Triangles;

        public Cube()
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

            float[] lVerticesFloat = new float[] { -0.500000f, -0.500000f, 0.500000f, 0.500000f, -0.500000f, 0.500000f, -0.500000f, 0.500000f, 0.500000f, 0.500000f, 0.500000f, 0.500000f, -0.500000f, 0.500000f, -0.500000f, 0.500000f, 0.500000f, -0.500000f, -0.500000f, -0.500000f, -0.500000f, 0.500000f, -0.500000f, -0.500000f };

            Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
            for (int i = 0; i < lVerticesFloat.Length; i += lStride)
            {
                lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
            }

            return lVertices;
        }

        private int[] CreateTriangles()
        {
            int[] lIndexes = { 3, 2, 0, 3, 0, 1, 3, 5, 2, 2, 5, 4, 7, 6, 4, 7, 4, 5, 1, 0, 6, 1, 6, 7, 3, 1, 5, 1, 7, 5, 2, 6, 0, 6, 2, 4 };
            return lIndexes;
        }
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
    /// Support class for a 12 sided polygon
    /// </summary>
    public class Dodecahedron
    {
        public Vector3[] Vertices;
        public int[] Triangles;

        public Dodecahedron()
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

            float[] lVerticesFloat = new float[] { 0.351283f, -0.499921f, -0.000000f, 0.595112f, -0.138430f, 0.000000f, 0.180745f, -0.121914f, -0.570779f, 0.489714f, 0.095191f, -0.352761f, 0.095191f, -0.489714f, -0.352761f, 0.180745f, -0.121914f, 0.570779f, 0.095191f, -0.489714f, 0.352761f, 0.489714f, 0.095191f, 0.352761f, -0.595112f, 0.138430f, -0.000000f, -0.351283f, 0.499921f, 0.000000f, -0.180745f, 0.121914f, 0.570779f, -0.489714f, -0.095191f, 0.352761f, -0.095191f, 0.489714f, 0.352761f, -0.180745f, 0.121914f, -0.570779f, -0.095191f, 0.489714f, -0.352761f, -0.489714f, -0.095191f, -0.352761f, -0.319176f, -0.473197f, 0.218018f, 0.319176f, 0.473197f, 0.218018f, 0.319176f, 0.473197f, -0.218018f, -0.319176f, -0.473197f, -0.218018f };

            Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
            for (int i = 0; i < lVerticesFloat.Length; i += lStride)
            {
                lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
            }

            return lVertices;
        }

        private int[] CreateTriangles()
        {
            int[] lIndexes = { 2, 1, 4, 1, 2, 3, 4, 1, 0, 1, 5, 6, 1, 6, 0, 5, 1, 7, 0, 16, 19, 16, 0, 6, 0, 19, 4, 16, 10, 11, 10, 16, 5, 5, 16, 6, 8, 19, 16, 19, 8, 15, 16, 11, 8, 19, 2, 4, 2, 19, 13, 13, 19, 15, 13, 18, 2, 18, 13, 14, 18, 3, 2, 18, 1, 3, 1, 17, 7, 17, 1, 18, 10, 17, 12, 17, 5, 7, 5, 17, 10, 12, 8, 10, 8, 12, 9, 8, 11, 10, 9, 17, 18, 17, 9, 12, 9, 18, 14, 13, 8, 14, 8, 13, 15, 14, 8, 9 };
            return lIndexes;
        }
    }

    /// <summary>
    /// Support class for a 20 sided polygon
    /// </summary>
    public class Icosahedron
    {
        public Vector3[] Vertices;
        public int[] Triangles;

        public Icosahedron()
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

            float[] lVerticesFloat = new float[] { 0.500001f, 0.000000f, -0.309017f, 0.500001f, -0.000000f, 0.309017f, -0.500001f, -0.000000f, 0.309017f, -0.500001f, 0.000000f, -0.309017f, 0.000000f, -0.309017f, 0.500001f, 0.000000f, 0.309017f, 0.500001f, 0.000000f, 0.309017f, -0.500001f, 0.000000f, -0.309017f, -0.500001f, -0.309017f, -0.500001f, -0.000000f, 0.309017f, -0.500001f, -0.000000f, 0.309017f, 0.500001f, 0.000000f, -0.309017f, 0.500001f, 0.000000f };

            Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
            for (int i = 0; i < lVerticesFloat.Length; i += lStride)
            {
                lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
            }

            return lVertices;
        }

        private int[] CreateTriangles()
        {
            int[] lIndexes = { 1, 9, 0, 0, 10, 1, 0, 7, 6, 0, 6, 10, 0, 9, 7, 4, 1, 5, 9, 1, 4, 1, 10, 5, 3, 8, 2, 2, 11, 3, 4, 5, 2, 2, 8, 4, 5, 11, 2, 6, 7, 3, 3, 11, 6, 3, 7, 8, 4, 8, 9, 5, 10, 11, 6, 11, 10, 7, 9, 8 };
            return lIndexes;
        }
    }

    /// <summary>
    /// Support class for a flat circle
    /// </summary>
    public class Disk
    {
        public Vector3[] Vertices;
        public int[] Triangles;

        public Disk()
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

            float[] lVerticesFloat = new float[] { 0.482963f, -0.001076f, -0.129409f, 0.433012f, -0.001076f, -0.250000f, 0.353553f, -0.001076f, -0.353553f, 0.250000f, -0.001076f, -0.433012f, 0.129410f, -0.001076f, -0.482963f, 0.000000f, -0.001076f, -0.500000f, -0.129409f, -0.001076f, -0.482963f, -0.250000f, -0.001076f, -0.433013f, -0.353553f, -0.001076f, -0.353553f, -0.433013f, -0.001076f, -0.250000f, -0.482963f, -0.001076f, -0.129410f, -0.500000f, -0.001076f, -0.000000f, -0.482963f, -0.001076f, 0.129409f, -0.433013f, -0.001076f, 0.250000f, -0.353553f, -0.001076f, 0.353553f, -0.250000f, -0.001076f, 0.433013f, -0.129410f, -0.001076f, 0.482963f, -0.000000f, -0.001076f, 0.500000f, 0.129409f, -0.001076f, 0.482963f, 0.250000f, -0.001076f, 0.433013f, 0.353553f, -0.001076f, 0.353553f, 0.433013f, -0.001076f, 0.250000f, 0.482963f, -0.001076f, 0.129410f, 0.500000f, -0.001076f, 0.000000f, 0.482963f, 0.001076f, -0.129409f, 0.433012f, 0.001076f, -0.250000f, 0.353553f, 0.001076f, -0.353553f, 0.250000f, 0.001076f, -0.433012f, 0.129410f, 0.001076f, -0.482963f, 0.000000f, 0.001076f, -0.500000f, -0.129409f, 0.001076f, -0.482963f, -0.250000f, 0.001076f, -0.433013f, -0.353553f, 0.001076f, -0.353553f, -0.433013f, 0.001076f, -0.250000f, -0.482963f, 0.001076f, -0.129410f, -0.500000f, 0.001076f, -0.000000f, -0.482963f, 0.001076f, 0.129409f, -0.433013f, 0.001076f, 0.250000f, -0.353553f, 0.001076f, 0.353553f, -0.250000f, 0.001076f, 0.433013f, -0.129410f, 0.001076f, 0.482963f, -0.000000f, 0.001076f, 0.500000f, 0.129409f, 0.001076f, 0.482963f, 0.250000f, 0.001076f, 0.433013f, 0.353553f, 0.001076f, 0.353553f, 0.433013f, 0.001076f, 0.250000f, 0.482963f, 0.001076f, 0.129410f, 0.500000f, 0.001076f, 0.000000f, 0.000000f, -0.001076f, 0.000000f, 0.000000f, 0.001076f, 0.000000f };

            Vector3[] lVertices = new Vector3[lVerticesFloat.Length / lStride];
            for (int i = 0; i < lVerticesFloat.Length; i += lStride)
            {
                lVertices[i / lStride] = new Vector3(lVerticesFloat[i], lVerticesFloat[i + 1], lVerticesFloat[i + 2]);
            }

            return lVertices;
        }

        private int[] CreateTriangles()
        {
            int[] lIndexes = { 25, 0, 24, 1, 0, 2, 0, 3, 1, 4, 25, 5, 26, 6, 25, 7, 1, 8, 1, 9, 2, 10, 26, 11, 2, 12, 3, 13, 27, 14, 2, 15, 27, 16, 26, 17, 28, 18, 3, 19, 4, 20, 3, 21, 28, 22, 27, 23, 4, 24, 5, 25, 29, 26, 4, 27, 29, 28, 28, 29, 30, 30, 5, 31, 6, 32, 5, 33, 30, 34, 29, 35, 31, 36, 6, 37, 7, 38, 6, 39, 31, 40, 30, 41, 7, 42, 8, 43, 32, 44, 7, 45, 32, 46, 31, 47, 33, 48, 8, 49, 9, 50, 8, 51, 33, 52, 32, 53, 34, 54, 9, 55, 10, 56, 9, 57, 34, 58, 33, 59, 35, 60, 10, 61, 11, 62, 10, 63, 35, 64, 34, 65, 36, 66, 11, 67, 12, 68, 11, 69, 36, 70, 35, 71, 37, 72, 12, 73, 13, 74, 12, 75, 37, 76, 36, 77, 38, 78, 13, 79, 14, 80, 13, 81, 38, 82, 37, 83, 39, 84, 38, 85, 14, 86, 39, 87, 14, 88, 15, 89, 40, 90, 39, 91, 15, 92, 40, 93, 15, 94, 16, 95, 41, 96, 40, 97, 16, 98, 41, 99, 16, 100, 17, 101, 42, 102, 41, 103, 17, 104, 42, 105, 17, 106, 18, 107, 43, 108, 42, 109, 18, 110, 43, 111, 18, 112, 19, 113, 44, 114, 43, 115, 19, 116, 44, 117, 19, 118, 20, 119, 45, 120, 44, 121, 20, 122, 20, 123, 21, 124, 45, 125, 46, 126, 45, 127, 21, 128, 21, 129, 22, 130, 46, 131, 47, 132, 46, 133, 22, 134, 22, 135, 23, 136, 47, 137, 24, 138, 47, 139, 23, 140, 23, 141, 0, 142, 24, 143, 1, 144, 0, 145, 48, 146, 2, 147, 1, 148, 48, 149, 3, 150, 2, 151, 48, 152, 4, 153, 3, 154, 48, 155, 5, 156, 4, 157, 48, 158, 6, 159, 5, 160, 48, 161, 7, 162, 6, 163, 48, 164, 8, 165, 7, 166, 48, 167, 9, 168, 8, 169, 48, 170, 10, 171, 9, 172, 48, 173, 11, 174, 10, 175, 48, 176, 12, 177, 11, 178, 48, 179, 13, 180, 12, 181, 48, 182, 14, 183, 13, 184, 48, 185, 15, 186, 14, 187, 48, 188, 16, 189, 15, 190, 48, 191, 17, 192, 16, 193, 48, 194, 18, 195, 17, 196, 48, 197, 19, 198, 18, 199, 48, 200, 20, 201, 19, 202, 48, 203, 21, 204, 20, 205, 48, 206, 22, 207, 21, 208, 48, 209, 23, 210, 22, 211, 48, 212, 0, 213, 23, 214, 48, 215, 24, 216, 25, 217, 49, 218, 25, 219, 26, 220, 49, 221, 26, 222, 27, 223, 49, 224, 27, 225, 28, 226, 49, 227, 28, 228, 29, 229, 49, 230, 29, 231, 30, 232, 49, 233, 30, 234, 31, 235, 49, 236, 31, 237, 32, 238, 49, 239, 32, 240, 33, 241, 49, 242, 33, 243, 34, 244, 49, 245, 34, 246, 35, 247, 49, 248, 35, 249, 36, 250, 49, 251, 36, 252, 37, 253, 49, 254, 37, 255, 38, 256, 49, 257, 38, 258, 39, 259, 49, 260, 39, 261, 40, 262, 49, 263, 40, 264, 41, 265, 49, 266, 41, 267, 42, 268, 49, 269, 42, 270, 43, 271, 49, 272, 43, 273, 44, 274, 49, 275, 44, 276, 45, 277, 49, 278, 45, 279, 46, 280, 49, 281, 46, 282, 47, 283, 49, 284, 47, 285, 24, 286, 49, 287 };

            int[] lTriangles = new int[lIndexes.Length / 2];
            for (int i = 0; i < lTriangles.Length; i++)
            {
                lTriangles[i] = lIndexes[i * 2];
            }

            return lTriangles;
        }
    }

    /// <summary>
    /// Support class for a 6 sided bone
    /// </summary>
    public class Bone
    {
        public static Vector3[] BoneVertices = new Vector3[] { new Vector3(0.000000f, 1.000000f,  0.000000f), // Top
                                                      new Vector3(0.100000f, 0.100000f,  0.000000f), // Mid-Right
                                                      new Vector3(0.000000f, 0.100000f, -0.100000f), // Mid-Back
                                                      new Vector3(-0.100000f, 0.100000f,  0.000000f), // Mid-Left
                                                      new Vector3(0.000000f, 0.100000f,  0.100000f), // Mid-Forward
                                                      new Vector3(0.000000f, 0.000000f,  0.000000f)  // Bottom
                                                 };

        public Vector3[] Vertices;
        public int[] Triangles;

        public Bone()
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

            float[] lVerticesFloat = new float[] {   0.000000f, 1.000000f,  0.000000f, // Top
                                                     0.100000f, 0.100000f,  0.000000f, // Mid-Right
                                                     0.000000f, 0.100000f, -0.100000f, // Mid-Back
                                                    -0.100000f, 0.100000f,  0.000000f, // Mid-Left
                                                     0.000000f, 0.100000f,  0.100000f, // Mid-Forward
                                                     0.000000f, 0.000000f,  0.000000f  // Bottom
                                                 };

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

    // Author: Kevin Tritz (tritz at yahoo *spamfilter* com)
    // http://codescrib.blogspot.com/
    // copyright (c) 2014  
    // license: BSD style  
    // derived from python version: Icosphere.py  
    //  
    //         Author: William G.K. Martin (wgm2111@cu where cu=columbia.edu)  
    //         copyright (c) 2010  
    //         license: BSD style  
    //        https://code.google.com/p/mesh2d-mpl/source/browse/icosphere.py  
    public class IcoSphere
    {
        public static Vector3[] vertices;           // Vector3[M] array of verticies, M = 10*(num+1)^2 + 2  
        public static int[] triangleIndices;        // int[3*N] flat triangle index list for mesh, N = 20*(num+1)^2  
        static int[,] triangles;                    // int[N,3] triangle verticies index list, N = 20*(num+1)^2  

        public static Mesh CreateSphere(int rSubdivisions)
        {
            IcoSphere.Icosahedron lIcosahedron = new IcoSphere.Icosahedron();       // initialize base Icosahedron, 20 faces, 12 vertices, radius = 1  
            get_triangulation(rSubdivisions, lIcosahedron);     // main function to subdivide and triangulate Icosahedron  

            Mesh lMesh = new Mesh();                 // mesh initialization and display   
            lMesh.hideFlags = HideFlags.HideAndDontSave;
            lMesh.vertices = vertices;
            //mesh.normals = vertices;  
            //mesh.uv = getUV(vertices);              // UV mapping is messed up near poles and longitude boundary  
            lMesh.triangles = triangleIndices;
            lMesh.RecalculateNormals();

            return lMesh;
        }

        static void get_triangulation(int num, Icosahedron ico)
        {
            Dictionary<Vector3, int> vertDict = new Dictionary<Vector3, int>();    // dict lookup to speed up vertex indexing  
            float[,] subdivision = getSubMatrix(num + 2);                            // vertex subdivision matrix calculation  
            Vector3 p1, p2, p3;
            int index = 0;
            int vertIndex;
            int len = subdivision.GetLength(0);
            int triNum = (num + 1) * (num + 1) * 20;            // number of triangle faces  
            vertices = new Vector3[triNum / 2 + 2];        // allocate verticies, triangles, etc...  
            triangleIndices = new int[triNum * 3];
            triangles = new int[triNum, 3];
            Vector3[] tempVerts = new Vector3[len];        // temporary structures for subdividing each Icosahedron face  
            int[] tempIndices = new int[len];
            int[,] triIndices = triangulate(num);        // precalculate generic subdivided triangle indices  
            int triLength = triIndices.GetLength(0);
            for (int i = 0; i < 20; i++)                    // calculate subdivided vertices and triangles for each face  
            {
                p1 = ico.Vertices[ico.Triangles[i * 3]];    // get 3 original vertex locations for each face  
                p2 = ico.Vertices[ico.Triangles[i * 3 + 1]];
                p3 = ico.Vertices[ico.Triangles[i * 3 + 2]];
                for (int j = 0; j < len; j++)                // calculate new subdivided vertex locations  
                {
                    tempVerts[j].x = subdivision[j, 0] * p1.x + subdivision[j, 1] * p2.x + subdivision[j, 2] * p3.x;
                    tempVerts[j].y = subdivision[j, 0] * p1.y + subdivision[j, 1] * p2.y + subdivision[j, 2] * p3.y;
                    tempVerts[j].z = subdivision[j, 0] * p1.z + subdivision[j, 1] * p2.z + subdivision[j, 2] * p3.z;
                    tempVerts[j].Normalize();
                    if (!vertDict.TryGetValue(tempVerts[j], out vertIndex))    // quick lookup to avoid vertex duplication  
                    {
                        vertDict[tempVerts[j]] = index;    // if vertex not in dict, add it to dictionary and final array  
                        vertIndex = index;
                        vertices[index] = tempVerts[j];
                        index += 1;
                    }
                    tempIndices[j] = vertIndex;            // assemble vertex indices for triangle assignment  
                }
                for (int j = 0; j < triLength; j++)        // map precalculated subdivided triangle indices to vertex indices  
                {
                    triangles[triLength * i + j, 0] = tempIndices[triIndices[j, 0]];
                    triangles[triLength * i + j, 1] = tempIndices[triIndices[j, 1]];
                    triangles[triLength * i + j, 2] = tempIndices[triIndices[j, 2]];
                    triangleIndices[3 * triLength * i + 3 * j] = tempIndices[triIndices[j, 0]];
                    triangleIndices[3 * triLength * i + 3 * j + 1] = tempIndices[triIndices[j, 1]];
                    triangleIndices[3 * triLength * i + 3 * j + 2] = tempIndices[triIndices[j, 2]];
                }
            }
        }
        static int[,] triangulate(int num)    // fuction to precalculate generic triangle indices for subdivided vertices  
        {
            int n = num + 2;
            int[,] triangles = new int[(n - 1) * (n - 1), 3];
            int shift = 0;
            int ind = 0;
            for (int row = 0; row < n - 1; row++)
            {
                triangles[ind, 0] = shift + 1;
                triangles[ind, 1] = shift + n - row;
                triangles[ind, 2] = shift;
                ind += 1;
                for (int col = 1; col < n - 1 - row; col++)
                {
                    triangles[ind, 0] = shift + col;
                    triangles[ind, 1] = shift + n - row + col;
                    triangles[ind, 2] = shift + n - row + col - 1;
                    ind += 1;
                    triangles[ind, 0] = shift + col + 1;
                    triangles[ind, 1] = shift + n - row + col;
                    triangles[ind, 2] = shift + col;
                    ind += 1;
                }
                shift += n - row;
            }
            return triangles;
        }
        static Vector2[] getUV(Vector3[] vertices)    // standard Longitude/Latitude mapping to (0,1)/(0,1)  
        {
            int num = vertices.Length;
            float pi = (float)System.Math.PI;
            Vector2[] UV = new Vector2[num];
            for (int i = 0; i < num; i++)
            {
                UV[i] = cartToLL(vertices[i]);
                UV[i].x = (UV[i].x + pi) / (2.0f * pi);
                UV[i].y = (UV[i].y + pi / 2.0f) / pi;
            }
            return UV;
        }
        static Vector2 cartToLL(Vector3 point)    // transform 3D cartesion coordinates to longitude, latitude  
        {
            Vector2 coord = new Vector2();
            float norm = point.magnitude;
            if (point.x != 0.0f || point.y != 0.0f)
                coord.x = -(float)System.Math.Atan2(point.y, point.x);
            else
                coord.x = 0.0f;
            if (norm > 0.0f)
                coord.y = (float)System.Math.Asin(point.z / norm);
            else
                coord.y = 0.0f;
            return coord;
        }
        static float[,] getSubMatrix(int num)    // vertex subdivision matrix, num=3 subdivides 1 triangle into 4  
        {
            int numrows = num * (num + 1) / 2;
            float[,] subdivision = new float[numrows, 3];
            float[] values = new float[num];
            int[] offsets = new int[num];
            int[] starts = new int[num];
            int[] stops = new int[num];
            int index;
            for (int i = 0; i < num; i++)
            {
                values[i] = (float)i / (float)(num - 1);
                offsets[i] = (num - i);
                if (i > 0)
                    starts[i] = starts[i - 1] + offsets[i - 1];
                else
                    starts[i] = 0;
                stops[i] = starts[i] + offsets[i];
            }
            for (int i = 0; i < num; i++)
            {
                for (int j = 0; j < offsets[i]; j++)
                {
                    index = starts[i] + j;
                    subdivision[index, 0] = values[offsets[i] - 1 - j];
                    subdivision[index, 1] = values[j];
                    subdivision[index, 2] = values[i];
                }
            }
            return subdivision;
        }

        public class Icosahedron
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