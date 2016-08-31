using UnityEngine;
using UnityEditor;
using com.ootii.Helpers;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(UnityInputSource))]
public class UnityInputSourceEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private UnityInputSource mTarget;
    private SerializedObject mTargetSO;

    // Activators that can be selected
    private string[] mActivators = new string[] { "None", "Left Mouse Button", "Right Mouse Button", "Left or Right Mouse Button" };

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (UnityInputSource)target;
        mTargetSO = new SerializedObject(target);
    }

    /// <summary>
    /// This function is called when the scriptable object goes out of scope.
    /// </summary>
    private void OnDisable()
    {
    }

    /// <summary>
    /// Called when the inspector needs to draw
    /// </summary>
    public override void OnInspectorGUI()
    {
        // Pulls variables from runtime so we have the latest values.
        mTargetSO.Update();

        GUILayout.Space(5);

        EditorHelper.DrawInspectorTitle("ootii Input Source");

        EditorHelper.DrawInspectorDescription("Simple input solution that grabs input directly from Unity's native input solution. It supports the Xbox controller.", MessageType.None);

        GUILayout.Space(5);

        bool lNewIsPlayerInputEnabled = EditorGUILayout.Toggle(new GUIContent("Is Input Enabled", "Determines if we'll get input from the mouse, keyboard, and gamepad."), mTarget.IsEnabled);
        if (lNewIsPlayerInputEnabled != mTarget.IsEnabled)
        {
            mIsDirty = true;
            mTarget.IsEnabled = lNewIsPlayerInputEnabled;
        }

        bool lNewIsXboxControllerEnabled = EditorGUILayout.Toggle(new GUIContent("Is Xbox Enabled", "Determines we can use the Xbox controller for input."), mTarget.IsXboxControllerEnabled);
        if (lNewIsXboxControllerEnabled != mTarget.IsXboxControllerEnabled)
        {
            mIsDirty = true;
            mTarget._IsXboxControllerEnabled = lNewIsXboxControllerEnabled;

            // Ensure our input manager entries exist
            if (mTarget.IsXboxControllerEnabled)
            {
                CreateInputManagerEntries();
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("View Activator", "Determines what button enables viewing."), GUILayout.Width(EditorGUIUtility.labelWidth));
        int lNewViewActivator = EditorGUILayout.Popup(mTarget.ViewActivator, mActivators);
        if (lNewViewActivator != mTarget.ViewActivator)
        {
            mIsDirty = true;
            mTarget.ViewActivator = lNewViewActivator;
        }
        EditorGUILayout.EndHorizontal();

        //bool lNewRotateOnRightMouse = EditorGUILayout.Toggle(new GUIContent("   On Right Mouse", "Determines if we must hold the right mouse button to rotate."), mTarget.RotateOnRightMouse);
        //if (lNewRotateOnRightMouse != mTarget.RotateOnRightMouse)
        //{
        //    mIsDirty = true;
        //    mTarget.RotateOnRightMouse = lNewRotateOnRightMouse;
        //}

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

    /// <summary>
    /// If the input manager entries don't exist, create them
    /// </summary>
    private void CreateInputManagerEntries()
    {
        if (!InputManagerHelper.IsDefined("WXButton0"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "WXButton0";
            lEntry.PositiveButton = "joystick button 0";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("WXButton1"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "WXButton1";
            lEntry.PositiveButton = "joystick button 1";
            lEntry.Gravity = 1000;
            lEntry.Dead = 0.001f;
            lEntry.Sensitivity = 1000;
            lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
            lEntry.Axis = 0;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("WXRightStickX"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "WXRightStickX";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 4;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("WXRightStickY"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "WXRightStickY";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Invert = true;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 5;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("MXRightStickX"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "MXRightStickX";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 3;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }

        if (!InputManagerHelper.IsDefined("MXRightStickY"))
        {
            InputManagerEntry lEntry = new InputManagerEntry();
            lEntry.Name = "MXRightStickY";
            lEntry.Gravity = 1;
            lEntry.Dead = 0.3f;
            lEntry.Sensitivity = 1;
            lEntry.Invert = true;
            lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
            lEntry.Axis = 4;
            lEntry.JoyNum = 0;

            InputManagerHelper.AddEntry(lEntry);
        }
    }
}
