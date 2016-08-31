using System.Collections.Generic;
using UnityEngine;
using com.ootii.Geometry;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Utilities.Debug
{
    /// <summary>
    /// Allow us to render waypoints and flags in the scene for debugging
    /// </summary>
    public class SceneFlags : MonoBehaviour
    {
        public static bool IsActive = true;

        public static List<SceneFlag> Flags = new List<SceneFlag>();

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation)
        {
            AddFlag(rPosition, rRotation, 1f, Color.black, "", Vector3.zero);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, Color rColor)
        {
            AddFlag(rPosition, rRotation, 1f, rColor, "", Vector3.zero);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, float rHeight, Color rColor)
        {
            AddFlag(rPosition, rRotation, rHeight, rColor, "", Vector3.zero);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, Color rColor, string rText)
        {
            AddFlag(rPosition, rRotation, 1f, rColor, rText, Vector3.zero);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, float rHeight, Color rColor, string rText)
        {
            AddFlag(rPosition, rRotation, rHeight, rColor, rText, Vector3.zero);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, Color rColor, Vector3 rVector)
        {
            AddFlag(rPosition, rRotation, 1f, rColor, "", rVector);
        }

        public static void AddFlag(Vector3 rPosition, Quaternion rRotation, float rHeight, Color rColor, string rText, Vector3 rVector)
        {
            if (!IsActive) { return; }

            SceneFlag lFlag = new SceneFlag();
            lFlag.Position = rPosition;
            lFlag.Rotation = rRotation;
            lFlag.Height = rHeight;
            lFlag.Color = rColor;
            lFlag.Text = rText;
            lFlag.Vector = rVector;

            Flags.Add(lFlag);
        }

        /// <summary>
        /// Determines if we'll add new flags or not
        /// </summary>
        public bool _IsEnabled = true;
        public bool IsEnabled
        {
            get { return _IsEnabled; }

            set
            {
                _IsEnabled = value;
                SceneFlags.IsActive = _IsEnabled;
            }
        }

        /// <summary>
        /// Clear the flags when we head into play
        /// </summary>
        private void Awake()
        {
            SceneFlags.IsActive = _IsEnabled;
            Flags.Clear();
        }

        /// <summary>
        /// Called each frame to render GUI to the editor
        /// </summary>
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR

            Handles.BeginGUI();

            for (int i = 0; i < SceneFlags.Flags.Count; i++)
            {
                SceneFlag lFlag = SceneFlags.Flags[i];
                Vector3 lFlagUp = lFlag.Rotation.Up();

                //DebugDraw.DrawSphereMesh(lFlag.Position, 0.02f, lFlag.Color, 1f);
                UnityEngine.Debug.DrawLine(lFlag.Position, lFlag.Position + (lFlagUp * lFlag.Height), lFlag.Color);

                if (lFlag.Vector != Vector3.zero)
                {
                    Vector3 lVectorStart = lFlag.Position + (lFlagUp * (lFlag.Height * 0.9f));
                    UnityEngine.Debug.DrawLine(lVectorStart, lVectorStart + (lFlag.Vector * 0.3f), lFlag.Color);
                }

                string lText = lFlag.Text;
                if (lText.Length == 0) { lText = "Flag " + i; }

                Color lHandleColor = GUI.color;

                GUI.color = lFlag.Color;
                Handles.Label(lFlag.Position + (lFlagUp * (lFlag.Height * 1.1f)), lText);

                // Reset
                GUI.color = lHandleColor;
            }

            Handles.EndGUI();

#endif
        }

        /// <summary>
        /// Single flag that is being placed
        /// </summary>
        public struct SceneFlag
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public float Height;
            public Color Color;
            public string Text;
            public Vector3 Vector;
        }
    }
}
