using UnityEngine;
using UnityEditor;
using com.ootii.Input;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(NavMeshInputSource))]
public class NavMeshInputSourceEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private NavMeshInputSource mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (NavMeshInputSource)target;
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

        EditorHelper.DrawInspectorTitle("ootii Nav Mesh Input Source");

        EditorHelper.DrawInspectorDescription("Input source that uses a Nav Mesh Agent to tell the Motion Controller what to do.", MessageType.None);

        GUILayout.Space(5);

        bool lNewIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Determines if the source is providing input."), mTarget.IsEnabled);
        if (lNewIsEnabled != mTarget.IsEnabled)
        {
            mIsDirty = true;
            mTarget.IsEnabled = lNewIsEnabled;
        }

        float lNewMaxViewSpeed = EditorGUILayout.FloatField(new GUIContent("Max View Speed", "Max speed that we use to simulate the view changing. A speed of 1 is normal."), mTarget.MaxViewSpeed);
        if (lNewMaxViewSpeed != mTarget.MaxViewSpeed)
        {
            mIsDirty = true;
            mTarget.MaxViewSpeed = lNewMaxViewSpeed;
        }

        GUILayout.Space(5);

        Transform lNewTarget = EditorGUILayout.ObjectField(new GUIContent("Target", "Transform that we'll use the Nav Mesh Agent to follow."), mTarget.Target, typeof(Transform), true) as Transform;
        if (lNewTarget != mTarget.Target)
        {
            mIsDirty = true;
            mTarget.Target = lNewTarget;
        }

        Vector3 lNewTargetPosition = EditorGUILayout.Vector3Field(new GUIContent("Target Position", "Specific position the Nav Mesh Agent will head to."), mTarget.TargetPosition);
        if (lNewTargetPosition != mTarget.TargetPosition)
        {
            mIsDirty = true;
            mTarget.TargetPosition = lNewTargetPosition;
        }

        GUILayout.Space(5);

        float lNewStopDistance = EditorGUILayout.FloatField(new GUIContent("Stop Distance", "Range in which we consider ourselves as arriving."), mTarget.StopDistance);
        if (lNewStopDistance != mTarget.StopDistance)
        {
            mIsDirty = true;
            mTarget.StopDistance = lNewStopDistance;
        }

        EditorGUILayout.BeginHorizontal();

        float lNewSlowDistance = EditorGUILayout.FloatField(new GUIContent("Slow Distance", "Range in which start slowing down."), mTarget.SlowDistance);
        if (lNewSlowDistance != mTarget.SlowDistance)
        {
            mIsDirty = true;
            mTarget.SlowDistance = lNewSlowDistance;
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField(new GUIContent("Slow Speed Factor", "Percentage of speed we'll drop to between the Slow Distance and Stop Distance"), GUILayout.Width(50));
        float lNewSlowFactor = EditorGUILayout.FloatField(mTarget.SlowFactor, GUILayout.Width(45));
        if (lNewSlowFactor != mTarget.SlowFactor)
        {
            mIsDirty = true;
            mTarget.SlowFactor = lNewSlowFactor;
        }

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        float lNewPathHeight = EditorGUILayout.FloatField(new GUIContent("Path Height", "Nav Mesh height that is added by Unity. This allows us to fix our height."), mTarget.PathHeight);
        if (lNewPathHeight != mTarget.PathHeight)
        {
            mIsDirty = true;
            mTarget.PathHeight = lNewPathHeight;
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
