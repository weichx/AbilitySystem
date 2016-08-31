using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using com.ootii.Utilities.Debug;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Provides functions that are specific to unity meshes
    /// </summary>
    public class MeshExt
    {
        /// <summary>
        /// Stores mesh data that has been processed to speed up the closest point search
        /// </summary>
        public static Dictionary<int, MeshOctree> MeshOctrees = new Dictionary<int, MeshOctree>();

        /// <summary>
        /// Records the last parse time for diagnostics
        /// </summary>
        public static Dictionary<int, float> MeshParseTime = new Dictionary<int, float>();

        /// <summary>
        /// Just used to hold a transform for debugging
        /// </summary>
        public static Transform DebugTransform = null;

        /// <summary>
        /// Given a mesh, cycle through it and look for the nearest vertex. We'll use this
        /// as a starting point for figuring out the closest point, but it's pretty brute force
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rTransform"></param>
        /// <param name="rMesh"></param>
        /// <returns></returns>
        public static Vector3 ClosestVertex(Vector3 rPoint, Transform rTransform, Mesh rMesh)
        {
            Vector3 lNearestVertex = Vector3.zero;
            float lMinDistanceSqr = float.MaxValue;

            // Move the point to local space
            Vector3 lLocalPosition = rTransform.InverseTransformPoint(rPoint);

            // Find the nearest one as quickly as possible
            for (int i = rMesh.vertices.Length - 1; i >= 0; i--)
            {
                Vector3 lVertex = rMesh.vertices[i];
                float lDistanceSqr = (lLocalPosition - lVertex).sqrMagnitude;

                if (lDistanceSqr < lMinDistanceSqr)
                {
                    lMinDistanceSqr = lDistanceSqr;
                    lNearestVertex = lVertex;
                }
            }

            return lNearestVertex;
        }
        
        /// <summary>
        /// Given a mesh, cycle through it and look for the nearest vertex. We'll use this
        /// as a starting point for figuring out the closest point
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rTransform"></param>
        /// <param name="rMesh"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, float rRadius, Transform rTransform, Mesh rMesh)
        {
            MeshOctree lMeshOctree = null;

            int lID = rMesh.GetInstanceID();
            if (MeshOctrees.ContainsKey(lID))
            {
                lMeshOctree = MeshOctrees[lID];
            }
            else
            {
                Stopwatch lTimer = new Stopwatch();
                lTimer.Start();

                lMeshOctree = new MeshOctree(rMesh);

                lTimer.Stop();

                MeshOctrees.Add(lID, lMeshOctree);
                MeshParseTime.Add(lID, lTimer.ElapsedTicks / (float)TimeSpan.TicksPerSecond);
            }

            //DebugTransform = rTransform;
            //DebugDraw.DrawSphereMesh(rPoint, 0.05f, Color.green, 1f);

            // Move the point to local space
            Vector3 lLocalPoint = rTransform.InverseTransformPoint(rPoint);

            // Now that we're in the root, find the closest point
            Vector3 lClosestPoint = MeshOctrees[lID].ClosestPoint(lLocalPoint, rRadius);

            if (lClosestPoint.x == float.MaxValue) { lClosestPoint = Vector3.zero; }

            // Move the point to world space
            if (lClosestPoint.sqrMagnitude != 0f)
            {
                lClosestPoint = rTransform.TransformPoint(lClosestPoint);
                //DebugDraw.DrawSphereMesh(lClosestPoint, 0.05f, Color.red, 1f);
            }

            // Return the world space point
            return lClosestPoint;
        }
        
        /// <summary>
        /// Determines the closest point between a point and a triangle.
        /// 
        /// The code in this method is copyrighted by the SlimDX Group under the MIT license:
        /// 
        /// Copyright (c) 2007-2010 SlimDX Group
        /// 
        /// Permission is hereby granted, free of charge, to any person obtaining a copy
        /// of this software and associated documentation files (the "Software"), to deal
        /// in the Software without restriction, including without limitation the rights
        /// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        /// copies of the Software, and to permit persons to whom the Software is
        /// furnished to do so, subject to the following conditions:
        /// 
        /// The above copyright notice and this permission notice shall be included in
        /// all copies or substantial portions of the Software.
        /// 
        /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        /// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        /// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        /// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        /// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        /// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        /// THE SOFTWARE.
        /// 
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="vertex1">The first vertex to test.</param>
        /// <param name="vertex2">The second vertex to test.</param>
        /// <param name="vertex3">The third vertex to test.</param>
        /// <param name="result">When the method completes, contains the closest point between the two objects.</param>
        public static void ClosestPointOnTriangle(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 result)
        {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 136

            //Check if P in vertex region outside A
            Vector3 ab = vertex2 - vertex1;
            Vector3 ac = vertex3 - vertex1;
            Vector3 ap = point - vertex1;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0.0f && d2 <= 0.0f)
            {
                result = vertex1; //Barycentric coordinates (1,0,0)
                return;
            }

            //Check if P in vertex region outside B
            Vector3 bp = point - vertex2;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                result = vertex2; // barycentric coordinates (0,1,0)
                return;
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                result = vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
                return;
            }

            //Check if P in vertex region outside C
            Vector3 cp = point - vertex3;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                result = vertex3; //Barycentric coordinates (0,0,1)
                return;
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                result = vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
                return;
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                result = vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
                return;
            }

            //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            result = vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }
    }
}
