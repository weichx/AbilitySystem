using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Utilities.Debug;

namespace com.ootii.Geometry
{
    /// <summary>
    /// High level container for the MeshOctreeNode. It's not
    /// required, but provides some convienience.
    /// </summary>
    public class MeshOctree
    {
        /// <summary>
        /// Name of the mesh being represented
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Root node for the octree
        /// </summary>
        public MeshOctreeNode Root = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MeshOctree()
        {
        }

        /// <summary>
        /// Mesh constructor
        /// </summary>
        /// <param name="rMesh"></param>
        public MeshOctree(Mesh rMesh)
        {
            Initialize(rMesh);
        }

        /// <summary>
        /// Initializes the octree with the specific mesh
        /// </summary>
        /// <param name="rMesh"></param>
        public void Initialize(Mesh rMesh)
        {
            if (rMesh == null) { return; }

            Name = rMesh.name;

            float lSize = Mathf.Max(rMesh.bounds.size.x, Mathf.Max(rMesh.bounds.size.y, rMesh.bounds.size.z));

            Root = new MeshOctreeNode(rMesh.bounds.center, new Vector3(lSize, lSize, lSize));
            Root.MeshVertices = rMesh.vertices;
            Root.MeshTriangles = rMesh.triangles;

            // Build the octree with the triangles
            int lTriangleCount = Root.MeshTriangles.Length / 3;
            for (int i = 0; i < lTriangleCount; i++)
            {
                Root.Insert(i * 3);
            }
        }

        /// <summary>
        /// Determines if the point (in local space) is within the bounds of 
        /// the octree.
        /// </summary>
        /// <param name="rPoint"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 rPoint)
        {
            if (Root == null) { return false; }
            return Root.ContainsPoint(rPoint);
        }

        /// <summary>
        /// Returns the closest point to the specified point
        /// </summary>
        /// <param name="rPoint">Point (in local space) we are wanting to find a close point too </param>
        /// <returns></returns>
        public Vector3 ClosestPoint(Vector3 rPoint)
        {
            if (Root == null) { return Vector3.zero; }

            Vector3 lMin = Root.Min;
            Vector3 lMax = Root.Max;

            // First, let's see if our point is inside the root node. If not, make it so.
            if (rPoint.x < lMin.x) { rPoint.x = lMin.x; }
            else if (rPoint.x > lMax.x) { rPoint.x = lMax.x; }

            if (rPoint.y < lMin.y) { rPoint.y = lMin.y; }
            else if (rPoint.y > lMax.y) { rPoint.y = lMax.y; }

            if (rPoint.z < lMin.z) { rPoint.z = lMin.z; }
            else if (rPoint.z > lMax.z) { rPoint.z = lMax.z; }

            DebugDraw.DrawSphereOverlay(MeshExt.DebugTransform.TransformPoint(rPoint), 0.1f, Color.magenta, 1f);

            // Now grab the closest point
            return Root.ClosestPoint(rPoint);
        }

        /// <summary>
        /// Finds the closest point on a mesh given the specified sphere. We can use a sphere
        /// in order to test for triangles that are in-range of the point.
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <returns></returns>
        public Vector3 ClosestPoint(Vector3 rPoint, float rRadius)
        {
            if (Root == null) { return Vector3.zero; }

            Vector3 lMin = Root.Min;
            Vector3 lMax = Root.Max;

            // First, let's see if our point is inside the root node. If not, make it so.
            if (rPoint.x < lMin.x) { rPoint.x = lMin.x; }
            else if (rPoint.x > lMax.x) { rPoint.x = lMax.x; }

            if (rPoint.y < lMin.y) { rPoint.y = lMin.y; }
            else if (rPoint.y > lMax.y) { rPoint.y = lMax.y; }

            if (rPoint.z < lMin.z) { rPoint.z = lMin.z; }
            else if (rPoint.z > lMax.z) { rPoint.z = lMax.z; }

            // Clear the triangle 
            return Root.ClosestPoint(rPoint, rRadius);
        }

        /// <summary>
        /// Renders the octree to the scene
        /// </summary>
        public void OnSceneGUI(Transform rTransform)
        {
#if UNITY_EDITOR
            if (Root != null) { Root.OnSceneGUI(rTransform); }
#endif
        }
    }
}
