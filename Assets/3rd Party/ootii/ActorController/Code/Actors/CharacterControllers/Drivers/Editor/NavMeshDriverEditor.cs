using UnityEngine;
using UnityEditor;
using com.ootii.Actors;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(NavMeshDriver))]
public class NavMeshDriverEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private NavMeshDriver mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (NavMeshDriver)target;
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

        EditorHelper.DrawInspectorTitle("ootii Nav Mesh Driver");

        EditorHelper.DrawInspectorDescription("Actor driver that uses a Nav Mesh Agent (and Mecanim Animator) to tell the Actor Controller what to do.", MessageType.None);

        GUILayout.Space(5);

        bool lNewIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Determines if the driver is actively controlling the actor."), mTarget.IsEnabled);
        if (lNewIsEnabled != mTarget.IsEnabled)
        {
            mIsDirty = true;
            mTarget.IsEnabled = lNewIsEnabled;
        }

        bool lNewUseNavMeshPosition = EditorGUILayout.Toggle(new GUIContent("Use Nav Position", "Determines if we'll use the nav mesh position directly or calculate our own."), mTarget.UseNavMeshPosition);
        if (lNewUseNavMeshPosition != mTarget.UseNavMeshPosition)
        {
            mIsDirty = true;
            mTarget.UseNavMeshPosition = lNewUseNavMeshPosition;
        }

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

        bool lNewClearTargetOnStop = EditorGUILayout.Toggle(new GUIContent("Clear Target On Stop", "Determine if we clear the target once it's reached."), mTarget.ClearTargetOnStop);
        if (lNewClearTargetOnStop != mTarget.ClearTargetOnStop)
        {
            mIsDirty = true;
            mTarget.ClearTargetOnStop = lNewClearTargetOnStop;
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

        EditorGUILayout.LabelField(new GUIContent("Slow Speed", "Speed we'll drop to between the Slow Distance and Stop Distance"), GUILayout.Width(50));
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
