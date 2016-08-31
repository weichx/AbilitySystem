using System;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace com.ootii.Geometry
{
    /// <summary>
    /// View class used to help with debugging the MeshOctree
    /// associated to the mesh.
    /// </summary>
    [AddComponentMenu("ootii/Mesh Partitioner")]
    public class MeshPartitioner : MonoBehaviour
    {
        /// <summary>
        /// Octree that represent the partition
        /// </summary>
        public MeshOctree MeshOctree = null;

        /// <summary>
        /// Determines if we parse the mesh on start vs. when requested
        /// </summary>
        public bool ParseOnStart = false;

        /// <summary>
        /// Time it took in seconds to build the octree
        /// </summary>
        public float ParseTime = 0f;

        /// <summary>
        /// Vertices in the mesh that was parsed
        /// </summary>
        public int ParseVertexCount = 0;

        // Test properties used by the editor
        public bool RenderOctree = false;
        public bool RenderMesh = false;
        public bool RenderTestNode = false;
        public bool RenderTestTriangle = false;
        public Vector3 TestPosition = Vector3.zero;
        public Transform TestTransform = null;

        /// <summary>
        /// Run before the game starts updating
        /// </summary>
        public void Start()
        {
            if (ParseOnStart)
            {
                Stopwatch lTimer = new Stopwatch();
                lTimer.Start();

                Mesh lMesh = ExtractMesh();

                if (lMesh != null)
                {
                    int lID = lMesh.GetInstanceID();

                    if (lMesh.isReadable)
                    {
                        if (!MeshExt.MeshOctrees.ContainsKey(lID))
                        {
                            MeshOctree lMeshOctree = new MeshOctree(lMesh);
                            MeshExt.MeshOctrees.Add(lID, lMeshOctree);
                            MeshExt.MeshParseTime.Add(lID, 0f);
                        }
                    }
                }

                lTimer.Stop();
                ParseTime = lTimer.ElapsedTicks / (float)TimeSpan.TicksPerSecond;
            }
        }

        /// <summary>
        /// Extract the mesh preferably from the mesh collider, but the original mesh if needed
        /// </summary>
        /// <returns></returns>
        public Mesh ExtractMesh()
        {
            Mesh lMesh = null;

            // First, check if we can use a mesh collider mesh
            MeshCollider lMeshCollider = gameObject.GetComponent<MeshCollider>();
            if (lMeshCollider != null) { lMesh = lMeshCollider.sharedMesh; }

            // Check if there's a child with a mesh collider
            lMeshCollider = gameObject.GetComponentInChildren<MeshCollider>();
            if (lMeshCollider != null) { lMesh = lMeshCollider.sharedMesh; }

            // If not, check if we can grab the original mesh
            if (lMesh == null)
            {
                MeshFilter lMeshFilter = gameObject.GetComponent<MeshFilter>();
                lMesh = lMeshFilter.sharedMesh;
            }

            // Ensure the mesh is usable
            if (lMesh != null && !lMesh.isReadable)
            {
                lMesh = null;
            }

            return lMesh;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Renders the octree to the scene
        /// </summary>
        public void OnSceneGUI()
        {
            if (!RenderOctree && !RenderMesh && !RenderTestNode && !RenderTestTriangle) { return; }

            Color lHandlesColor = Handles.color;

            if (MeshOctree == null || MeshOctree.Root == null)
            {
                Stopwatch lTimer = new Stopwatch();
                lTimer.Start();

                Mesh lMesh = ExtractMesh();
                if (lMesh != null) { MeshOctree = new MeshOctree(lMesh); }

                lTimer.Stop();
                ParseTime = lTimer.ElapsedTicks / (float)TimeSpan.TicksPerSecond;
            }

            if (MeshOctree != null && MeshOctree.Root != null)
            {
                if (RenderOctree)
                {
                    MeshOctree.OnSceneGUI(transform);
                }

                if (RenderMesh)
                {
                    Handles.color = new Color(0f, 1f, 0f, 0.5f);

                    int[] lTriangles = MeshOctree.Root.MeshTriangles;
                    Vector3[] lVertices = MeshOctree.Root.MeshVertices;

                    int lTriangleCount = lTriangles.Length / 3;
                    for (int i = 0; i < lTriangleCount; i++)
                    {
                        int lIndex = i * 3;
                        Vector3 lVertex1 = transform.TransformPoint(lVertices[lTriangles[lIndex]]);
                        Vector3 lVertex2 = transform.TransformPoint(lVertices[lTriangles[lIndex + 1]]);
                        Vector3 lVertex3 = transform.TransformPoint(lVertices[lTriangles[lIndex + 2]]);

                        Handles.DrawLine(lVertex1, lVertex2);
                        Handles.DrawLine(lVertex2, lVertex3);
                        Handles.DrawLine(lVertex3, lVertex1);
                    }
                }

                if (RenderTestTriangle)
                {
                    Handles.color = Color.magenta;

                    Vector3 lLocalPoint = transform.InverseTransformPoint(TestTransform != null ? TestTransform.position : TestPosition);

                    // First, let's see if our point is inside the root node. If not, make it so.
                    if (!MeshOctree.ContainsPoint(lLocalPoint))
                    {
                        if (lLocalPoint.x < MeshOctree.Root.Min.x) { lLocalPoint.x = MeshOctree.Root.Min.x; }
                        else if (lLocalPoint.x > MeshOctree.Root.Max.x) { lLocalPoint.x = MeshOctree.Root.Max.x; }

                        if (lLocalPoint.y < MeshOctree.Root.Min.y) { lLocalPoint.y = MeshOctree.Root.Min.y; }
                        else if (lLocalPoint.y > MeshOctree.Root.Max.y) { lLocalPoint.y = MeshOctree.Root.Max.y; }

                        if (lLocalPoint.z < MeshOctree.Root.Min.z) { lLocalPoint.z = MeshOctree.Root.Min.z; }
                        else if (lLocalPoint.z > MeshOctree.Root.Max.z) { lLocalPoint.z = MeshOctree.Root.Max.z; }
                    }

                    // Grab the closest triangle
                    int lTriangleIndex = MeshOctree.Root.ClosestTriangle(lLocalPoint);
                    if (lTriangleIndex >= 0)
                    {
                        Vector3 lVertex1 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex]]);
                        Vector3 lVertex2 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex + 1]]);
                        Vector3 lVertex3 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex + 2]]);

                        Handles.DrawLine(lVertex1, lVertex2);
                        Handles.DrawLine(lVertex2, lVertex3);
                        Handles.DrawLine(lVertex3, lVertex1);

                        Handles.SphereCap(0, transform.TransformPoint(lLocalPoint), Quaternion.identity, 0.05f);
                    }
                }

                if (RenderTestNode)
                {
                    Vector3 lLocalPoint = transform.InverseTransformPoint(TestTransform != null ? TestTransform.position : TestPosition);

                    // Grab the closest node
                    MeshOctreeNode lNode = MeshOctree.Root.ClosestNode(lLocalPoint);
                    if (lNode != null)
                    {
                        Handles.color = Color.white;
                        lNode.OnSceneGUI(transform);

                        if (lNode.TriangleIndexes != null)
                        {
                            Handles.color = Color.magenta;
                            for (int i = 0; i < lNode.TriangleIndexes.Count; i++)
                            {
                                int lTriangleIndex = lNode.TriangleIndexes[i];
                                Vector3 lVertex1 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex]]);
                                Vector3 lVertex2 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex + 1]]);
                                Vector3 lVertex3 = transform.TransformPoint(MeshOctree.Root.MeshVertices[MeshOctree.Root.MeshTriangles[lTriangleIndex + 2]]);

                                Handles.DrawLine(lVertex1, lVertex2);
                                Handles.DrawLine(lVertex2, lVertex3);
                                Handles.DrawLine(lVertex3, lVertex1);
                            }
                        }
                    }
                }
            }

            Handles.color = lHandlesColor;
        }

#endif

    }
}
