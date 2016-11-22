using UnityEngine;
using UnityEditor;
using com.ootii.Actors.Attributes;
using com.ootii.Actors.LifeCores;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(ActorCore))]
public class ActorCoreEditor : Editor
{
    // Helps us keep track of when the list needs to be saved. This
    // is important since some changes happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private ActorCore mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (ActorCore)target;
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

        EditorHelper.DrawInspectorTitle("ootii Actor Core");

        EditorHelper.DrawInspectorDescription("Very basic foundation for actors. This allows us to set some simple properties.", MessageType.None);

        GUILayout.Space(5);

        GameObject lNewAttributeSourceOwner = EditorHelper.InterfaceOwnerField<IAttributeSource>(new GUIContent("Attribute Source", "Attribute source we'll use to the actor's current health."), mTarget.AttributeSourceOwner, true);
        if (lNewAttributeSourceOwner != mTarget.AttributeSourceOwner)
        {
            mIsDirty = true;
            mTarget.AttributeSourceOwner = lNewAttributeSourceOwner;
        }

        if (EditorHelper.TextField("Health ID", "Attribute identifier that represents the health attribute", mTarget.HealthID))
        {
            mIsDirty = true;
            mTarget.HealthID = EditorHelper.FieldStringValue;
        }

        GUILayout.Space(5);

        if (EditorHelper.BoolField("Is Alive", "Determines if the actor is actually alive", mTarget.IsAlive))
        {
            mIsDirty = true;
            mTarget.IsAlive = EditorHelper.FieldBoolValue;
        }

        if (EditorHelper.TextField("Damaged Motion", "Name of motion to activate when damage occurs", mTarget.DamagedMotion))
        {
            mIsDirty = true;
            mTarget.DamagedMotion = EditorHelper.FieldStringValue;
        }

        if (EditorHelper.TextField("Death Motion", "Name of motion to activate when death occurs", mTarget.DeathMotion))
        {
            mIsDirty = true;
            mTarget.DeathMotion = EditorHelper.FieldStringValue;
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
