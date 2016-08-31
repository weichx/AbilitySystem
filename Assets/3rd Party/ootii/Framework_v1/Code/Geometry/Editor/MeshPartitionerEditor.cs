using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using com.ootii.Geometry;
using com.ootii.Helpers;

[CanEditMultipleObjects]
[CustomEditor(typeof(MeshPartitioner))]
public class MeshPartitionerEditor : Editor
{
    // Helps us keep track of when the target needs to be saved. This
    // is important since some chang es happen in scene.
    private bool mIsDirty;

    // The actual class we're storing
    private MeshPartitioner mTarget;
    private SerializedObject mTargetSO;

    /// <summary>
    /// Called when the object is selected in the editor
    /// </summary>
    private void OnEnable()
    {
        // Grab the serialized objects
        mTarget = (MeshPartitioner)target;
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

        EditorHelper.DrawInspectorTitle("ootii Mesh Partitioner");

        EditorHelper.DrawInspectorDescription("The mesh partitioner pre-process the mesh collider so it's not done at run-time.", MessageType.None);

        GUILayout.Space(10);

        bool lNewParseOnStart = GUILayout.Toggle(mTarget.ParseOnStart, new GUIContent("Parse on Start", ""));
        if (lNewParseOnStart != mTarget.ParseOnStart)
        {
            mIsDirty = true;
            mTarget.ParseOnStart = lNewParseOnStart;
        }

        GUILayout.Space(5f);

        GUILayout.Label(string.Format("Vertex Count:  {0}", mTarget.ParseVertexCount));
        GUILayout.Label(string.Format("Parse Seconds: {0:f5}", mTarget.ParseTime));

        GUILayout.Space(5f);

        EditorGUILayout.BeginVertical(EditorHelper.Box);

        bool lNewRenderOctree = GUILayout.Toggle(mTarget.RenderOctree, new GUIContent("Render Octree", ""));
        if (lNewRenderOctree != mTarget.RenderOctree)
        {
            mIsDirty = true;
            mTarget.RenderOctree = lNewRenderOctree;
        }

        bool lNewRenderMesh = GUILayout.Toggle(mTarget.RenderMesh, new GUIContent("Render Mesh", ""));
        if (lNewRenderMesh != mTarget.RenderMesh)
        {
            mIsDirty = true;
            mTarget.RenderMesh = lNewRenderMesh;
        }

        bool lNewRenderTestNode = GUILayout.Toggle(mTarget.RenderTestNode, new GUIContent("Render Test Node", ""));
        if (lNewRenderTestNode != mTarget.RenderTestNode)
        {
            mIsDirty = true;
            mTarget.RenderTestNode = lNewRenderTestNode;
        }

        bool lNewRenderTestTriangle = GUILayout.Toggle(mTarget.RenderTestTriangle, new GUIContent("Render Test Triangle", ""));
        if (lNewRenderTestTriangle != mTarget.RenderTestTriangle)
        {
            mIsDirty = true;
            mTarget.RenderTestTriangle = lNewRenderTestTriangle;
        }

        Transform lNewTestTransform = EditorGUILayout.ObjectField(new GUIContent("Test Transform"), mTarget.TestTransform, typeof(Transform), true, null) as Transform;
        if (lNewTestTransform != mTarget.TestTransform)
        {
            mIsDirty = true;
            mTarget.TestTransform = lNewTestTransform;
        }

        Vector3 lNewTestPosition = EditorGUILayout.Vector3Field(new GUIContent("Test Position"), mTarget.TestPosition);
        if (lNewTestPosition != mTarget.TestPosition)
        {
            mIsDirty = true;
            mTarget.TestPosition = lNewTestPosition;
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);

        if (GUILayout.Button("Rebuild Octree", EditorStyles.miniButton))
        {
            Mesh lMesh = mTarget.ExtractMesh();
            if (lMesh != null)
            {
                mIsDirty = true;

                Stopwatch lTimer = new Stopwatch();
                lTimer.Start();

                mTarget.MeshOctree = new MeshOctree(lMesh);

                lTimer.Stop();
                mTarget.ParseTime = lTimer.ElapsedTicks / (float)TimeSpan.TicksPerSecond;
                mTarget.ParseVertexCount = lMesh.vertexCount;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Mesh does not exist or is not readable.");
            }
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

    /// <summary>
    /// Allow the actor controller to render to the editor
    /// </summary>
    void OnSceneGUI()
    {
        if (mTarget != null)
        {
            mTarget.OnSceneGUI();
        }
    }
}
