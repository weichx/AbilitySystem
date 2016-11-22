using UnityEngine;
using UnityEditor;
using com.ootii.Actors;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(MoveToDriver))]
public class MoveToDriverEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private MoveToDriver mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (MoveToDriver)target;
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

        EditorHelper.DrawInspectorTitle("ootii Move To Driver");

        EditorHelper.DrawInspectorDescription("Actor driver used for simple non-Nav Mesh movement.", MessageType.None);

        GUILayout.Space(5);

        if (EditorHelper.BoolField("Is Enabled", "Determines if the driver is actively controlling the actor.", mTarget.IsEnabled, mTarget))
        {
            mIsDirty = true;
            mTarget.IsEnabled = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.ObjectField<Transform>("Target", "Transform that we'll use the Nav Mesh Agent to follow.", mTarget.Target, mTarget))
        {
            mIsDirty = true;
            mTarget.Target = EditorHelper.FieldObjectValue as Transform;
        }

        if (EditorHelper.Vector3Field("Target Position", "Specific position the Nav Mesh Agent will head to.", mTarget.TargetPosition, mTarget))
        {
            mIsDirty = true;
            mTarget.TargetPosition = EditorHelper.FieldVector3Value;
        }

        if (EditorHelper.BoolField("Clear Target On Stop", "Determine if we clear the target once it's reached.", mTarget.ClearTargetOnStop, mTarget))
        {
            mIsDirty = true;
            mTarget.ClearTargetOnStop = EditorHelper.FieldBoolValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.FloatField("Stop Distance", "Range in which we consider ourselves as arriving.", mTarget.StopDistance, mTarget))
        {
            mIsDirty = true;
            mTarget.StopDistance = EditorHelper.FieldFloatValue;
        }

        if (EditorHelper.FloatField("Slow Distance", "Range in which start slowing down.", mTarget.SlowDistance, mTarget))
        {
            mIsDirty = true;
            mTarget.SlowDistance = EditorHelper.FieldFloatValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.FloatField("Fast Speed", "Meters per second the actor moves when outside the slow distance.", mTarget.FastSpeed, mTarget))
        {
            mIsDirty = true;
            mTarget.FastSpeed = EditorHelper.FieldFloatValue;
        }

        if (EditorHelper.FloatField("Slow Speed", "Meters per second the actor moves when inside the slow distance.", mTarget.SlowSpeed, mTarget))
        {
            mIsDirty = true;
            mTarget.SlowSpeed = EditorHelper.FieldFloatValue;
        }

        if (EditorHelper.FloatField("Rotation Speed", "Degrees per second the actor rotates.", mTarget.RotationSpeed, mTarget))
        {
            mIsDirty = true;
            mTarget.RotationSpeed = EditorHelper.FieldFloatValue;
        }

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
