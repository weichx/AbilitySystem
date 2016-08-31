using UnityEngine;
using UnityEditor;
using com.ootii.Actors;
using com.ootii.Helpers;
using com.ootii.Input;

[CanEditMultipleObjects]
[CustomEditor(typeof(ActorDriver))]
public class ActorDriverEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private ActorDriver mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (ActorDriver)target;
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

        EditorHelper.DrawInspectorTitle("ootii Actor Driver");

        EditorHelper.DrawInspectorDescription("Basic driver that takes input and tells the Actor Controller what to do.", MessageType.None);

        GUILayout.Space(5);

        bool lNewIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Determines if the driver is actively controlling the actor."), mTarget.IsEnabled);
        if (lNewIsEnabled != mTarget.IsEnabled)
        {
            mIsDirty = true;
            mTarget.IsEnabled = lNewIsEnabled;
        }

        GameObject lNewInputSourceOwner = EditorHelper.InterfaceOwnerField<IInputSource>(new GUIContent("Input Source", ""), mTarget.InputSourceOwner, true);
        if (lNewInputSourceOwner != mTarget.InputSourceOwner)
        {
            mIsDirty = true;
            mTarget.InputSourceOwner = lNewInputSourceOwner;
        }

        GUILayout.Space(5);

        float lNewMovementSpeed = EditorGUILayout.FloatField(new GUIContent("Movement Speed", "Meters per second the actor moves."), mTarget.MovementSpeed);
        if (lNewMovementSpeed != mTarget.MovementSpeed)
        {
            mIsDirty = true;
            mTarget.MovementSpeed = lNewMovementSpeed;
        }

        float lNewRotationSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second the actor rotates."), mTarget.RotationSpeed);
        if (lNewRotationSpeed != mTarget.RotationSpeed)
        {
            mIsDirty = true;
            mTarget.RotationSpeed = lNewRotationSpeed;
        }

        GUILayout.Space(5);

        float lNewJumpForce = EditorGUILayout.FloatField(new GUIContent("Jump Force", "Amount of upwards force to apply when a jump occurs."), mTarget.JumpForce);
        if (lNewJumpForce != mTarget.JumpForce)
        {
            mIsDirty = true;
            mTarget.JumpForce = lNewJumpForce;
        }

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
