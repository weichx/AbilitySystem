using UnityEditor;
using UnityEngine;
using com.ootii.Graphics;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(GraphicsManager))]
public class GraphicsManagerEditor : Editor
{
    // Helps us keep track of when the target needs to be saved. This
    // is important since some chang es happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private GraphicsManager mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (GraphicsManager)target;
        mTargetSO = new SerializedObject(target);
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Graphics Manager");

        EditorHelper.DrawInspectorDescription("By adding this component to the camera, we can draw graphics to the scene and game views.", MessageType.None);

        GUILayout.Space(5);

        bool lNewDrawToSceneView = EditorGUILayout.Toggle(new GUIContent("Draw to Scene", "Determines if we render graphics to the scene view."), mTarget.DrawToSceneView);
        if (lNewDrawToSceneView != mTarget.DrawToSceneView)
        {
            mIsDirty = true;
            mTarget.DrawToSceneView = lNewDrawToSceneView;
        }

        bool lNewDrawToGameView = EditorGUILayout.Toggle(new GUIContent("Draw to Game", "Determines if we render graphics to the game view."), mTarget.DrawToGameView);
        if (lNewDrawToGameView != mTarget.DrawToGameView)
        {
            mIsDirty = true;
            mTarget.DrawToGameView = lNewDrawToGameView;
        }

        string lNewShader = EditorGUILayout.TextField("Shader", mTarget.DefaultShader);
        if (lNewShader != mTarget.DefaultShader)
        {
            mIsDirty = true;
            mTarget.DefaultShader = lNewShader;
        }

        Font lNewFont = EditorGUILayout.ObjectField("Font", mTarget.DefaultFont, typeof(Font), false) as Font;
        if (lNewFont != mTarget.DefaultFont)
        {
            mIsDirty = true;
            mTarget.DefaultFont = lNewFont;
        }

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        EditorGUILayout.LabelField(string.Format("Lines:{0}  Triangles:{1}", mTarget.LineCount, mTarget.TriangleCount));

        if (GUILayout.Button("Clear Render Lists", EditorStyles.miniButton))
        {
            GraphicsManager.Clear();
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5);

        // If there is a change... update.
        if (mIsDirty)
        {
            // Flag the object as needing to be saved
            EditorUtility.SetDirty(mTarget);

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            EditorApplication.MarkSceneDirty();
#else
            if (!EditorApplication.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
#endif

            // Pushes the values back to the runtime so it has the changes
            mTargetSO.ApplyModifiedProperties();

            // Clear out the dirty flag
            mIsDirty = false;
        }
    }
}
