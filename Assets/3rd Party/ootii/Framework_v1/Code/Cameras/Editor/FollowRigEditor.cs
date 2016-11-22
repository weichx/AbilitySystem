using UnityEngine;
using UnityEditor;
using com.ootii.Cameras;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(FollowRig))]
public class FollowRigEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private FollowRig mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (FollowRig)target;
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

        EditorHelper.DrawInspectorTitle("ootii Follow Rig");

        EditorHelper.DrawInspectorDescription("Basic camera rig that follows the 'anchor' and is always behind it, looking in the direction of the anchor.", MessageType.None);

        GUILayout.Space(5);

        bool lNewIsInternalUpdateEnabled = EditorGUILayout.Toggle(new GUIContent("Force Update", "Determines if we allow the camera rig to update itself or if something like the Actor Controller will tell the camera when to update."), mTarget.IsInternalUpdateEnabled);
        if (lNewIsInternalUpdateEnabled != mTarget.IsInternalUpdateEnabled)
        {
            mIsDirty = true;
            mTarget.IsInternalUpdateEnabled = lNewIsInternalUpdateEnabled;
        }

        GUILayout.Space(5);

        Transform lNewAnchor = EditorGUILayout.ObjectField(new GUIContent("Anchor", "Transform the camera is meant to follow."), mTarget.Anchor, typeof(Transform), true) as Transform;
        if (lNewAnchor != mTarget.Anchor)
        {
            mIsDirty = true;
            mTarget.Anchor = lNewAnchor;
        }

        Vector3 lNewAnchorOffset = EditorGUILayout.Vector3Field(new GUIContent("Anchor Offset", "Position of the camera relative to the anchor."), mTarget.AnchorOffset);
        if (lNewAnchorOffset != mTarget.AnchorOffset)
        {
            mIsDirty = true;
            mTarget.AnchorOffset = lNewAnchorOffset;
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
