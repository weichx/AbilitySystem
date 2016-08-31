using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Utilities.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Geometry
{
    /// <summary>
    /// Octree node built specifically for partitioning and
    /// searching meshes for vertices and triangles. This works
    /// by finding the lowest node a specific point belongs to.
    /// That node then contains a list of triangles that cover the area.
    /// 
    /// Since triangles exist in three dimensions, we'll turn them
    /// into a sphere. Any node the sphere touches it will become part of
    /// </summary>
    public class MeshOctreeNode
    {
        /// <summary>
        /// Defines the max triangles before we split
        /// </summary>
        public const int MAX_TRIANGLES = 20;

        /// <summary>
        /// Minimum size for the node. We won't split smaller
        /// </summary>
        public const float MIN_NODE_SIZE = 0.05f;

        /// <summary>
        /// Floating point error to use in comparisons
        /// </summary>
        public const float EPSILON = 0.00001f;

        /// <summary>
        /// Used to gather the triangles that are valid in a ClosestPoint test
        /// </summary>
        private static List<int> sClosestTrianglesIndexes = new List<int>();

        /// <summary>
        /// Center of the octree node
        /// </summary>
        public Vector3 Center = Vector3.zero;

        /// <summary>
        /// Size of the octree node
        /// </summary>
        public Vector3 Size = Vector3.zero;

        /// <summary>
        /// Min position of the node
        /// </summary>
        public Vector3 Min = Vector3.zero;

        /// <summary>
        /// Max position of the node
        /// </summary>
        public Vector3 Max = Vector3.zero;

        /// <summary>
        /// Child nodes that contain elements
        /// </summary>
        public MeshOctreeNode[] Children = null;

        /// <summary>
        /// Indexes to triangles who exist in the node. Note that we
        /// only need to store the first index of the triangle (of the
        /// MeshTriangles array)
        /// </summary>
        public List<int> TriangleIndexes = null;

        /// <summary>
        /// Provides access to the mesh vertices this node works with
        /// </summary>
        public Vector3[] MeshVertices = null;

        /// <summary>
        /// Provides access to the mesh triangles this node works with
        /// </summary>
        public int[] MeshTriangles = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MeshOctreeNode()
        {
        }

        /// <summary>
        /// Position constructor
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rSize"></param>
        public MeshOctreeNode(Vector3 rCenter, Vector3 rSize)
        {
            Center = rCenter;
            Size = rSize;

            Min = Center - (Size * 0.5f);
            Max = Center + (Size * 0.5f);
        }

        /// <summary>
        /// Position constructor
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rSize"></param>
        public MeshOctreeNode(float rX, float rY, float rZ, Vector3 rSize)
        {
            Center = new Vector3(rX, rY, rZ);
            Size = rSize;

            Min = Center - (Size * 0.5f);
            Max = Center + (Size * 0.5f);
        }

        /// <summary>
        /// Position constructor
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rSize"></param>
        public MeshOctreeNode(float rX, float rY, float rZ, Vector3 rSize, Vector3[] rVertexArray, int[] rTriangleArray)
        {
            Center = new Vector3(rX, rY, rZ);
            Size = rSize;

            Min = Center - (Size * 0.5f);
            Max = Center + (Size * 0.5f);

            MeshVertices = rVertexArray;
            MeshTriangles = rTriangleArray;
        }

        /// <summary>
        /// Determines if the point (in local space) is within the bounds of the node.
        /// </summary>
        /// <param name="rPoint"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 rPoint)
        {
            if (rPoint.x + EPSILON < Min.x) { return false; }
            if (rPoint.x - EPSILON > Max.x) { return false; }
            if (rPoint.y + EPSILON < Min.y) { return false; }
            if (rPoint.y - EPSILON > Max.y) { return false; }
            if (rPoint.z + EPSILON < Min.z) { return false; }
            if (rPoint.z - EPSILON > Max.z) { return false; }

            return true;
        }

        /// <summary>
        /// Determins if the sphere (in local space is within the bounds of the node.
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 rPoint, float rRadius)
        {
            Vector3 lSurfacePoint = rPoint;
            lSurfacePoint.x = (lSurfacePoint.x > Max.x) ? Max.x : lSurfacePoint.x;
            lSurfacePoint.x = (lSurfacePoint.x < Min.x) ? Min.x : lSurfacePoint.x;
            lSurfacePoint.y = (lSurfacePoint.y > Max.y) ? Max.y : lSurfacePoint.y;
            lSurfacePoint.y = (lSurfacePoint.y < Min.y) ? Min.y : lSurfacePoint.y;
            lSurfacePoint.z = (lSurfacePoint.z > Max.z) ? Max.z : lSurfacePoint.z;
            lSurfacePoint.z = (lSurfacePoint.z < Min.z) ? Min.z : lSurfacePoint.z;

            return (lSurfacePoint - rPoint).sqrMagnitude <= rRadius * rRadius;
        }
        
        /// <summary>
        /// Finds the closest point on the mesh given the specified point (in local space)
        /// </summary>
        /// <param name="rPoint"></param>
        /// <returns></returns>
        public Vector3 ClosestPoint(Vector3 rPoint)
        {
            Vector3 lPoint;

            Vector3 lClosestPoint = Vector3.zero;
            lClosestPoint.x = float.MaxValue;

            if (rPoint.x + EPSILON < Min.x) { return lClosestPoint; }
            if (rPoint.x - EPSILON > Max.x) { return lClosestPoint; }
            if (rPoint.y + EPSILON < Min.y) { return lClosestPoint; }
            if (rPoint.y - EPSILON > Max.y) { return lClosestPoint; }
            if (rPoint.z + EPSILON < Min.z) { return lClosestPoint; }
            if (rPoint.z - EPSILON > Max.z) { return lClosestPoint; }

            if (Children == null)
            {
                if (TriangleIndexes != null)
                {
                    for (int i = 0; i < TriangleIndexes.Count; i++)
                    {
                        int lTriangleIndex = TriangleIndexes[i];
                        Vector3 lVertex1 = MeshVertices[MeshTriangles[lTriangleIndex]];
                        Vector3 lVertex2 = MeshVertices[MeshTriangles[lTriangleIndex + 1]];
                        Vector3 lVertex3 = MeshVertices[MeshTriangles[lTriangleIndex + 2]];

                        MeshExt.ClosestPointOnTriangle(ref rPoint, ref lVertex1, ref lVertex2, ref lVertex3, out lPoint);
                        if (lPoint.x == float.MaxValue) { continue; }

                        if (lClosestPoint.x == float.MaxValue || Vector3.SqrMagnitude(lPoint - rPoint) < Vector3.SqrMagnitude(lClosestPoint - rPoint)) 
                        { 
                            lClosestPoint = lPoint;

                            //lVertex1 = MeshExt.DebugTransform.TransformPoint(lVertex1);
                            //lVertex2 = MeshExt.DebugTransform.TransformPoint(lVertex2);
                            //lVertex3 = MeshExt.DebugTransform.TransformPoint(lVertex3);
                            //DebugDraw.DrawLineOverlay(lVertex1, lVertex2, 0.01f, Color.magenta, 1f);
                            //DebugDraw.DrawLineOverlay(lVertex2, lVertex3, 0.01f, Color.magenta, 1f);
                            //DebugDraw.DrawLineOverlay(lVertex3, lVertex1, 0.01f, Color.magenta, 1f);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    lPoint = Children[i].ClosestPoint(rPoint);
                    if (lPoint.x == float.MaxValue) { continue; }

                    if (lClosestPoint.x == float.MaxValue || Vector3.SqrMagnitude(lPoint - rPoint) < Vector3.SqrMagnitude(lClosestPoint - rPoint)) { lClosestPoint = lPoint; }
                }
            }

            return lClosestPoint;
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
            if (rRadius == 0f) { return ClosestPoint(rPoint); }

            Vector3 lPoint;

            Vector3 lClosestPoint = Vector3.zero;
            lClosestPoint.x = float.MaxValue;

            // Gather all the triangles that are in range
            sClosestTrianglesIndexes.Clear();
            GetTriangles(rPoint, rRadius, sClosestTrianglesIndexes);

            // Go through all the triangles and find the closest
            for (int i = 0; i < sClosestTrianglesIndexes.Count; i++)
            {
                int lTriangleIndex = sClosestTrianglesIndexes[i];
                Vector3 lVertex1 = MeshVertices[MeshTriangles[lTriangleIndex]];
                Vector3 lVertex2 = MeshVertices[MeshTriangles[lTriangleIndex + 1]];
                Vector3 lVertex3 = MeshVertices[MeshTriangles[lTriangleIndex + 2]];

                MeshExt.ClosestPointOnTriangle(ref rPoint, ref lVertex1, ref lVertex2, ref lVertex3, out lPoint);
                if (lPoint.x == float.MaxValue) { continue; }

                if (lClosestPoint.x == float.MaxValue || Vector3.SqrMagnitude(lPoint - rPoint) < Vector3.SqrMagnitude(lClosestPoint - rPoint))
                {
                    lClosestPoint = lPoint;

                    ////lVertex1 = MeshExt.DebugTransform.TransformPoint(lVertex1);
                    ////lVertex2 = MeshExt.DebugTransform.TransformPoint(lVertex2);
                    ////lVertex3 = MeshExt.DebugTransform.TransformPoint(lVertex3);
                    ////DebugDraw.DrawLineOverlay(lVertex1, lVertex2, 0.01f, Color.magenta, 1f);
                    ////DebugDraw.DrawLineOverlay(lVertex2, lVertex3, 0.01f, Color.magenta, 1f);
                    ////DebugDraw.DrawLineOverlay(lVertex3, lVertex1, 0.01f, Color.magenta, 1f);
                }
            }

            return lClosestPoint;
        }

        /// <summary>
        /// Returns the closest triangle index that the point is positioned at.
        /// </summary>
        /// <param name="rPoint">Position (in local space) we are testing</param>
        /// <returns></returns>
        public int ClosestTriangle(Vector3 rPoint)
        {
            int lClosestIndex = -1;

            if (rPoint.x + EPSILON < Min.x) { return lClosestIndex; }
            if (rPoint.x - EPSILON > Max.x) { return lClosestIndex; }
            if (rPoint.y + EPSILON < Min.y) { return lClosestIndex; }
            if (rPoint.y - EPSILON > Max.y) { return lClosestIndex; }
            if (rPoint.z + EPSILON < Min.z) { return lClosestIndex; }
            if (rPoint.z - EPSILON > Max.z) { return lClosestIndex; }

            Vector3 lPoint;

            Vector3 lClosestPoint = Vector3.zero;
            lClosestPoint.x = float.MaxValue;

            if (Children == null)
            {
                if (TriangleIndexes != null)
                {
                    for (int i = 0; i < TriangleIndexes.Count; i++)
                    {
                        int lTriangleIndex = TriangleIndexes[i];
                        Vector3 lVertex1 = MeshVertices[MeshTriangles[lTriangleIndex]];
                        Vector3 lVertex2 = MeshVertices[MeshTriangles[lTriangleIndex + 1]];
                        Vector3 lVertex3 = MeshVertices[MeshTriangles[lTriangleIndex + 2]];

                        MeshExt.ClosestPointOnTriangle(ref rPoint, ref lVertex1, ref lVertex2, ref lVertex3, out lPoint);
                        if (lPoint.x == float.MaxValue) { continue; }

                        if (lClosestPoint.x == float.MaxValue || Vector3.SqrMagnitude(lPoint - rPoint) < Vector3.SqrMagnitude(lClosestPoint - rPoint)) 
                        {
                            lClosestIndex = lTriangleIndex;
                            lClosestPoint = lPoint; 
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    int lTestClosestIndex = Children[i].ClosestTriangle(rPoint);
                    if (lTestClosestIndex >= 0) 
                    {
                        lClosestIndex = lTestClosestIndex;
                    }
                }
            }

            return lClosestIndex;
        }

        /// <summary>
        /// Returns the node that the point belongs to
        /// </summary>
        /// <param name="rPoint"></param>
        /// <returns></returns>
        public MeshOctreeNode ClosestNode(Vector3 rPoint)
        {
            MeshOctreeNode lClosestNode = null;

            if (rPoint.x + EPSILON < Min.x) { return lClosestNode; }
            if (rPoint.x - EPSILON > Max.x) { return lClosestNode; }
            if (rPoint.y + EPSILON < Min.y) { return lClosestNode; }
            if (rPoint.y - EPSILON > Max.y) { return lClosestNode; }
            if (rPoint.z + EPSILON < Min.z) { return lClosestNode; }
            if (rPoint.z - EPSILON > Max.z) { return lClosestNode; }

            if (Children == null)
            {
                lClosestNode = this;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    MeshOctreeNode lTestClosestNode = Children[i].ClosestNode(rPoint);
                    if (lTestClosestNode != null)
                    {
                        if (lClosestNode == null || lTestClosestNode.Size.sqrMagnitude < lClosestNode.Size.sqrMagnitude)
                        {
                            lClosestNode = lTestClosestNode;
                        }
                    }
                }
            }

            return lClosestNode;
        }

        /// <summary>
        /// Insert the triangle index into the node if it belongs. If it does and
        /// we have child nodes, push the triangle down.
        /// </summary>
        /// <param name="rTriangleIndex">Index into the mesh triangles</param>
        public void Insert(int rTriangleIndex)
        {
            Vector3 lTriangleCenter;
            Vector3 lTriangleMin;
            Vector3 lTriangleMax;

            GetTriangleBounds(rTriangleIndex, out lTriangleCenter, out lTriangleMin, out lTriangleMax);
            Insert(rTriangleIndex, lTriangleCenter, lTriangleMin, lTriangleMax);
        }

        /// <summary>
        /// Insert the triangle index into the node if it belongs. If it does and
        /// we have child nodes, push the triangle down.
        /// </summary>
        /// <param name="rTriangleIndex">Index into the mesh triangles</param>
        /// <param name="rTriangleCenter">Centroid of the triangle</param>
        /// <param name="rTriangleRadius">Radius of the triangle</param>
        public void Insert(int rTriangleIndex, Vector3 rTriangleCenter, float rTriangleRadius)
        {
            // First test if the triangle belongs in this node
            Vector3 lDirection = Center - rTriangleCenter;
            Vector3 lClosestPoint = rTriangleCenter + (lDirection.normalized * Mathf.Min(lDirection.magnitude, rTriangleRadius));

            if (lClosestPoint.x + EPSILON < Min.x) { return; }
            if (lClosestPoint.x - EPSILON > Max.x) { return; }
            if (lClosestPoint.y + EPSILON < Min.y) { return; }
            if (lClosestPoint.y - EPSILON > Max.y) { return; }
            if (lClosestPoint.z + EPSILON < Min.z) { return; }
            if (lClosestPoint.z - EPSILON > Max.z) { return; }

            // Determine if we should insert here or split
            if (Children == null)
            {
                // Ensure we have a place for our values
                if (TriangleIndexes == null) { TriangleIndexes = new List<int>(); }

                // If we have room, add the triangle
                if (TriangleIndexes.Count < MAX_TRIANGLES || Size.x <= MIN_NODE_SIZE)
                {
                    TriangleIndexes.Add(rTriangleIndex);
                }
                // If we have too many values already, split
                else
                {
                    Split();
                }
            }

            // If we are split, push the insert down
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].Insert(rTriangleIndex, rTriangleCenter, rTriangleRadius);
                }
            }
        }

        /// <summary>
        /// Insert the triangle index into the node if it belongs. If it does and
        /// we have child nodes, push the triangle down.
        /// </summary>
        /// <param name="rTriangleIndex">Index into the mesh triangles</param>
        /// <param name="rTriangleCenter">Centroid of the triangle</param>
        /// <param name="rTriangleMin">Minimum position of the AABB for the triangle</param>
        /// <param name="rTriangleMax">Maximum position of the AABB for the triangle</param>
        public void Insert(int rTriangleIndex, Vector3 rTriangleCenter, Vector3 rTriangleMin, Vector3 rTriangleMax)
        {
            if (rTriangleMax.x + EPSILON < Min.x) { return; }
            if (rTriangleMin.x - EPSILON > Max.x) { return; }
            if (rTriangleMax.y + EPSILON < Min.y) { return; }
            if (rTriangleMin.y - EPSILON > Max.y) { return; }
            if (rTriangleMax.z + EPSILON < Min.z) { return; }
            if (rTriangleMin.z - EPSILON > Max.z) { return; }

            // Determine if we should insert here or split
            if (Children == null)
            {
                // Ensure we have a place for our values
                if (TriangleIndexes == null) { TriangleIndexes = new List<int>(); }

                // If we have room, add the triangle
                if (TriangleIndexes.Count < MAX_TRIANGLES || Size.x <= MIN_NODE_SIZE)
                {
                    TriangleIndexes.Add(rTriangleIndex);
                }
                // If we have too many values already, split
                else
                {
                    Split();
                }
            }

            // If we are split, push the insert down
            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].Insert(rTriangleIndex, rTriangleCenter, rTriangleMin, rTriangleMax);
                }
            }
        }
        
        /// <summary>
        /// Split the octree and distribute it's values
        /// </summary>
        public virtual void Split()
        {
            Vector3 lHalfSize = Size * 0.5f;
            Vector3 lQuarterSize = lHalfSize * 0.5f;

            // Split into eight children
            Children = new MeshOctreeNode[8];
            Children[0] = new MeshOctreeNode(Center.x - lQuarterSize.x, Center.y - lQuarterSize.y, Center.z - lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // bottom left back
            Children[1] = new MeshOctreeNode(Center.x + lQuarterSize.x, Center.y - lQuarterSize.y, Center.z - lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // bottom right back
            Children[2] = new MeshOctreeNode(Center.x - lQuarterSize.x, Center.y + lQuarterSize.y, Center.z - lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // top left back
            Children[3] = new MeshOctreeNode(Center.x + lQuarterSize.x, Center.y + lQuarterSize.y, Center.z - lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // top right back
            Children[4] = new MeshOctreeNode(Center.x - lQuarterSize.x, Center.y - lQuarterSize.y, Center.z + lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // bottom left forward
            Children[5] = new MeshOctreeNode(Center.x + lQuarterSize.x, Center.y - lQuarterSize.y, Center.z + lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // bottom right forward
            Children[6] = new MeshOctreeNode(Center.x - lQuarterSize.x, Center.y + lQuarterSize.y, Center.z + lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // top left forward
            Children[7] = new MeshOctreeNode(Center.x + lQuarterSize.x, Center.y + lQuarterSize.y, Center.z + lQuarterSize.z, lHalfSize, MeshVertices, MeshTriangles); // top right forward

            // Distribute values to the children
            for (int lIndex = 0; lIndex < TriangleIndexes.Count; lIndex++)
            {
                //float lTriangleRadius;
                Vector3 lTriangleCenter;
                Vector3 lTriangleMin;
                Vector3 lTriangleMax;
                int lTriangleIndex = TriangleIndexes[lIndex];               
                
                //GetTriangleBounds(lTriangleIndex, out lTriangleCenter, out lTriangleRadius);
                GetTriangleBounds(lTriangleIndex, out lTriangleCenter, out lTriangleMin, out lTriangleMax);

                for (int i = 0; i < 8; i++)
                {
                    //Children[i].Insert(lTriangleIndex, lTriangleCenter, lTriangleRadius);
                    Children[i].Insert(lTriangleIndex, lTriangleCenter, lTriangleMin, lTriangleMax);
                }
            }

            // Since we have children, clear out values
            TriangleIndexes.Clear();
            TriangleIndexes = null;
        }

        /// <summary>
        /// Retrieve all the triangles that are part of the nodes that this sphere
        /// intersects with.
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <param name="rTriangles"></param>
        /// <returns></returns>
        public void GetTriangles(Vector3 rPoint, float rRadius, List<int> rTriangles)
        {
            if (!ContainsPoint(rPoint, rRadius)) { return; }

            if (Children == null)
            {
                if (TriangleIndexes != null)
                {
                    for (int i = 0; i < TriangleIndexes.Count; i++)
                    {
                        int lIndex = TriangleIndexes[i];
                        if (!rTriangles.Contains(lIndex))
                        {
                            rTriangles.Add(lIndex);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    Children[i].GetTriangles(rPoint, rRadius, rTriangles);
                }
            }
        }

        /// <summary>
        /// Distributes values to the children
        /// </summary>
        /// <param name="rTriangleIndex">Index into the mesh triangles</param>
        /// <param name="rTriangleCenter">Centroid of the triangle</param>
        /// <param name="rTriangleRadius">Radius of the triangle</param>
        public void GetTriangleBounds(int rTriangleIndex, out Vector3 rTriangleCenter, out float rTriangleRadius)
        {
            Vector3 lVertex1 = MeshVertices[MeshTriangles[rTriangleIndex]];
            Vector3 lVertex2 = MeshVertices[MeshTriangles[rTriangleIndex + 1]];
            Vector3 lVertex3 = MeshVertices[MeshTriangles[rTriangleIndex + 2]];
            rTriangleCenter = (lVertex1 + lVertex2 + lVertex3) / 3f;

            float lVertex1Distance = Vector3.SqrMagnitude(lVertex1 - rTriangleCenter);
            float lVertex2Distance = Vector3.SqrMagnitude(lVertex2 - rTriangleCenter);
            float lVertex3Distance = Vector3.SqrMagnitude(lVertex3 - rTriangleCenter);
            rTriangleRadius = Mathf.Sqrt(Mathf.Max(lVertex1Distance, Mathf.Max(lVertex2Distance, lVertex3Distance)));
        }

        /// <summary>
        /// Using the index into the mesh verticies, grab the local space AA-bounds.
        /// </summary>
        /// <param name="rTriangleIndex">Index into the mesh triangles</param>
        /// <param name="rTriangleCenter">Centroid of the triangle</param>
        /// <param name="rTriangleRadius">Radius of the triangle</param>
        public void GetTriangleBounds(int rTriangleIndex, out Vector3 rTriangleCenter, out Vector3 rTriangleMin, out Vector3 rTriangleMax)
        {
            Vector3 lVertex1 = MeshVertices[MeshTriangles[rTriangleIndex]];
            Vector3 lVertex2 = MeshVertices[MeshTriangles[rTriangleIndex + 1]];
            Vector3 lVertex3 = MeshVertices[MeshTriangles[rTriangleIndex + 2]];
            rTriangleCenter = (lVertex1 + lVertex2 + lVertex3) / 3f;

            rTriangleMin = Vector3.zero;
            rTriangleMax = Vector3.zero;

            rTriangleMin.x = Mathf.Min(lVertex1.x, Mathf.Min(lVertex2.x, lVertex3.x));
            rTriangleMax.x = Mathf.Max(lVertex1.x, Mathf.Max(lVertex2.x, lVertex3.x));

            rTriangleMin.y = Mathf.Min(lVertex1.y, Mathf.Min(lVertex2.y, lVertex3.y));
            rTriangleMax.y = Mathf.Max(lVertex1.y, Mathf.Max(lVertex2.y, lVertex3.y));

            rTriangleMin.z = Mathf.Min(lVertex1.z, Mathf.Min(lVertex2.z, lVertex3.z));
            rTriangleMax.z = Mathf.Max(lVertex1.z, Mathf.Max(lVertex2.z, lVertex3.z));
        }

        /// <summary>
        /// Renders the node to the scene
        /// </summary>
        public void OnSceneGUI(Transform rTransform)
        {
#if UNITY_EDITOR

            Vector3 lMin = rTransform.TransformPoint(Min);
            Vector3 lMax = rTransform.TransformPoint(Max);

            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(lMin.x, lMin.y, lMin.z), new Vector3(lMax.x, lMin.y, lMin.z));
            Handles.DrawLine(new Vector3(lMin.x, lMin.y, lMin.z), new Vector3(lMin.x, lMax.y, lMin.z));
            Handles.DrawLine(new Vector3(lMin.x, lMin.y, lMin.z), new Vector3(lMin.x, lMin.y, lMax.z));
            Handles.DrawLine(new Vector3(lMin.x, lMax.y, lMin.z), new Vector3(lMax.x, lMax.y, lMin.z));
            Handles.DrawLine(new Vector3(lMax.x, lMin.y, lMin.z), new Vector3(lMax.x, lMax.y, lMin.z));
            Handles.DrawLine(new Vector3(lMax.x, lMin.y, lMin.z), new Vector3(lMax.x, lMin.y, lMax.z));
            Handles.DrawLine(new Vector3(lMin.x, lMax.y, lMin.z), new Vector3(lMin.x, lMax.y, lMax.z));
            Handles.DrawLine(new Vector3(lMax.x, lMax.y, lMin.z), new Vector3(lMax.x, lMax.y, lMax.z));
            Handles.DrawLine(new Vector3(lMax.x, lMax.y, lMax.z), new Vector3(lMin.x, lMax.y, lMax.z));
            Handles.DrawLine(new Vector3(lMax.x, lMax.y, lMax.z), new Vector3(lMax.x, lMin.y, lMax.z));
            Handles.DrawLine(new Vector3(lMin.x, lMax.y, lMax.z), new Vector3(lMin.x, lMin.y, lMax.z));
            Handles.DrawLine(new Vector3(lMin.x, lMin.y, lMax.z), new Vector3(lMax.x, lMin.y, lMax.z));

            if (Children != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    Children[i].OnSceneGUI(rTransform);
                }
            }

#endif
        }
    }
}
