using UnityEngine;
using UnityEditor;
using com.ootii.Helpers;
using com.ootii.Input;
using com.ootii.Game;

[CanEditMultipleObjects]
[CustomEditor(typeof(GameCore))]
public class GameCoreEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're stroing
    private GameCore mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the script object is loaded
    /// </summary>
    void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (GameCore)target;
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

        EditorHelper.DrawInspectorTitle("ootii Game Core");

        EditorHelper.DrawInspectorDescription("Used to manage global aspects of the game such as the cursor, menues, etc.", MessageType.None);

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GameObject lGameObject = EditorHelper.InterfaceOwnerField<IInputSource>(new GUIContent("Input Source", "Input source we'll use to get key presses, mouse movement, etc. This GameObject should have a component implementing the IInputSource interface."), mTarget.InputSourceOwner, true);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Set Input Source");
            mTarget.InputSourceOwner = lGameObject;
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField(new GUIContent("Find", "Determines if we attempt to automatically find the input source at startup if one isn't set."), GUILayout.Width(30));

        EditorGUI.BeginChangeCheck();
        bool lBool = EditorGUILayout.Toggle(mTarget.AutoFindInputSource, GUILayout.Width(16));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Set Auto Find Input Source");
            mTarget.AutoFindInputSource = lBool;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorHelper.Box);
        EditorHelper.DrawSmallTitle("Editor Options");

        if (EditorHelper.TextField("Pause Alias", "Action alias to pause playing while in the editor.", mTarget.EditorPauseAlias))
        {
            mIsDirty = true;
            mTarget.EditorPauseAlias = EditorHelper.FieldStringValue;
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorHelper.Box);
        EditorHelper.DrawSmallTitle("Cursor Options");

        if (EditorHelper.BoolField("Is Cursor Visible", "Determines if the mouse cursor starts as visible.", mTarget.IsCursorVisible))
        {
            mIsDirty = true;
            mTarget.IsCursorVisible = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.TextField("Cursor Alias", "Action alias to determine if the cursor should be visible.", mTarget.ShowCursorAlias))
        {
            mIsDirty = true;
            mTarget.ShowCursorAlias = EditorHelper.FieldStringValue;
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