using UnityEditor;
using UnityEngine;
using com.ootii.Helpers;
using com.ootii.Utilities.Debug;

[CanEditMultipleObjects]
[CustomEditor(typeof(Log))]
public class LogEditor : Editor
{
    // Helps us keep track of when the target needs to be saved. This
    // is important since some chang es happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private Log mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (com.ootii.Utilities.Debug.Log)target;
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

        EditorHelper.DrawInspectorTitle("ootii Debug Logger");

        EditorHelper.DrawInspectorDescription("By adding this component to the camera, we can log information directly to the screen and change settings.", MessageType.None);

        GUILayout.Space(5);

        bool lNewPrefixTime = EditorGUILayout.Toggle(new GUIContent("Prefix Time", "Determines if we add the time to the front of the log entries."), mTarget._PrefixTime);
        if (lNewPrefixTime != mTarget._PrefixTime)
        {
            mIsDirty = true;
            mTarget._PrefixTime = lNewPrefixTime;
            Log.PrefixTime = mTarget._PrefixTime;
        }

        GUILayout.Space(5);

        int lNewLineCount = EditorGUILayout.IntField(new GUIContent("Line Count", "Number of lines to write to on the screen (edit-time only)."), mTarget._LineCount);
        if (lNewLineCount != mTarget._LineCount)
        {
            if (!Application.isPlaying)
            {
                mIsDirty = true;
                mTarget._LineCount = lNewLineCount;
                Log.LineCount = lNewLineCount;
            }
        }

        int lNewFontSize = EditorGUILayout.IntField(new GUIContent("Font Size", "Font size when writing to the screen."), mTarget._ScreenFontSize);
        if (lNewFontSize != mTarget._ScreenFontSize)
        {
            mIsDirty = true;
            mTarget._ScreenFontSize = lNewFontSize;
            Log.FontSize = lNewFontSize;
        }

        Color lNewFontColor = EditorGUILayout.ColorField(new GUIContent("Font Color", "Font color when writing to the screen"), mTarget._ScreenForeColor);
        if (lNewFontColor != mTarget._ScreenForeColor)
        {
            mIsDirty = true;
            mTarget._ScreenForeColor = lNewFontColor;
            Log.ForeColor = lNewFontColor;
        }

        bool lNewClearScreenEachFrame = EditorGUILayout.Toggle(new GUIContent("Clear Each Frame", "Determines if we clear the screen each frame."), mTarget._ClearScreenEachFrame);
        if (lNewPrefixTime != mTarget._ClearScreenEachFrame)
        {
            mIsDirty = true;
            mTarget._ClearScreenEachFrame = lNewClearScreenEachFrame;
            Log.ClearScreenEachFrame = mTarget._ClearScreenEachFrame;
        }

        GUILayout.Space(5);

        string lNewFilePath = EditorGUILayout.TextField(new GUIContent("File Path", "Relative path to the file. For example: '.\\Log.txt' will write to the root of the project."), mTarget._FilePath);
        if (lNewFilePath != mTarget._FilePath)
        {
            mIsDirty = true;
            mTarget._FilePath = lNewFilePath;
            Log.FilePath = mTarget._FilePath;
        }

        bool lNewFileFlushPerWrite = EditorGUILayout.Toggle(new GUIContent("Flush Per Write", "Determines if we flush to the file on ever write. This causes a performance hit, but allows for updating the file immediately."), mTarget._FileFlushPerWrite);
        if (lNewFileFlushPerWrite != mTarget._FileFlushPerWrite)
        {
            mIsDirty = true;
            mTarget._FileFlushPerWrite = lNewFileFlushPerWrite;
            Log.FileFlushPerWrite = mTarget._FileFlushPerWrite;
        }

        GUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        EditorGUILayout.LabelField("For All Write:");

        bool lNewIsConsoleEnabled = EditorGUILayout.Toggle(new GUIContent("Is Console Enabled", "Determines if we'll write to the console."), mTarget._IsConsoleEnabled);
        if (lNewIsConsoleEnabled != mTarget._IsConsoleEnabled)
        {
            mIsDirty = true;
            mTarget._IsConsoleEnabled = lNewIsConsoleEnabled;
            Log.IsConsoleEnabled = mTarget._IsConsoleEnabled;
        }

        bool lNewIsScreenEnabled = EditorGUILayout.Toggle(new GUIContent("Is Screen Enabled", "Determines if we'll write to the screen."), mTarget._IsScreenEnabled);
        if (lNewIsScreenEnabled != mTarget._IsScreenEnabled)
        {
            mIsDirty = true;
            mTarget._IsScreenEnabled = lNewIsScreenEnabled;
            Log.IsScreenEnabled = mTarget._IsScreenEnabled;
        }

        bool lNewIsFileEnabled = EditorGUILayout.Toggle(new GUIContent("Is File Enabled", "Determines if we'll write to a file."), mTarget._IsFileEnabled);
        if (lNewIsFileEnabled != mTarget._IsFileEnabled)
        {
            mIsDirty = true;
            mTarget._IsFileEnabled = lNewIsFileEnabled;
            Log.IsFileEnabled = mTarget._IsFileEnabled;
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

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
