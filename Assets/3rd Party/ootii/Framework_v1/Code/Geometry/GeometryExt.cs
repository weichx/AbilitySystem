using System;
using UnityEngine;
using com.ootii.Utilities.Debug;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Set of shape-based functions that allow us to evaluate and manipulate shapes
    /// </summary>
    public class GeometryExt
    {
        // Allows for floating point error
        public const float EPSILON = 0.0001f;

        /// <summary>
        /// Allows us to ignore collisions against a single transform
        /// </summary>
        public static Transform Ignore = null;

        /// <summary>
        /// Allows us to ignore collisions against an array of transforms
        /// </summary>
        public static Transform[] IgnoreArray = null;

        /// <summary>
        /// Directions used for finding the closes point on terrain
        /// </summary>
        public static Vector3[] SphericalDirections = null;

        /// <summary>
        /// Static constructor
        /// </summary>
        static GeometryExt()
        {
            SphericalDirections = new Vector3[14];
            SphericalDirections[0] = Vector3.forward;
            SphericalDirections[1] = Vector3.back;
            SphericalDirections[2] = Vector3.right;
            SphericalDirections[3] = Vector3.left;
            SphericalDirections[4] = Vector3.up;
            SphericalDirections[5] = Vector3.down;
            SphericalDirections[6] = Vector3.Normalize(new Vector3(1f, 1f, 1f));
            SphericalDirections[7] = Vector3.Normalize(new Vector3(-1f, 1f, 1f));
            SphericalDirections[8] = Vector3.Normalize(new Vector3(1f, -1f, 1f));
            SphericalDirections[9] = Vector3.Normalize(new Vector3(-1f, -1f, 1f));
            SphericalDirections[10] = Vector3.Normalize(new Vector3(1f, 1f, -1f));
            SphericalDirections[11] = Vector3.Normalize(new Vector3(-1f, 1f, -1f));
            SphericalDirections[12] = Vector3.Normalize(new Vector3(1f, -1f, -1f));
            SphericalDirections[13] = Vector3.Normalize(new Vector3(-1f, -1f, -1f));
        }

        #region Closest Contact Point Functions

        /// <summary>
        /// Gets the closest contact point on the collider given our desired position
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <param name="rCollider"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Collider rCollider, int rCollisionLayers = -1)
        {
            // Ensure our collider is actually on a layer we can collide with
            if (rCollisionLayers > -1 && rCollider != null && rCollider.gameObject != null)
            {
                if (((1 << rCollider.gameObject.layer) & rCollisionLayers) == 0)
                {
                    return Vector3.zero;
                }
            }

            // Find the contact point
            Vector3 lContactPoint = Vector3.zero;

            // Use the box collider
            if (rCollider is BoxCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (BoxCollider)rCollider);
            }
            // Use the sphere collider
            else if (rCollider is SphereCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (SphereCollider)rCollider);
            }
            // Use the capsule collider
            else if (rCollider is CapsuleCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (CapsuleCollider)rCollider);
            }
            // Use the character controller
            else if (rCollider is CharacterController)
            {
                lContactPoint = ClosestPoint(rPoint, (CharacterController)rCollider);
            }
            // Use the terrain collider
            else if (rCollider is TerrainCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (TerrainCollider)rCollider, 4f, rCollisionLayers);
            }
            // Use the mesh collider
            else if (rCollider is MeshCollider)
            {
                MeshCollider lCollider = (MeshCollider)rCollider;

                // If no mesh, we can stop
                if (lCollider.sharedMesh == null)
                {
                }
                // Planes are expensive with octrees because they aren't really volumes (2d vs 3d)
                // So, we turn the plane into a box collider
                else if (lCollider.sharedMesh.name == "Plane")
                {
                    Transform lTransform = lCollider.transform;

                    Vector3 lColliderSize = lCollider.sharedMesh.bounds.size;
                    //lColliderSize.x *= lTransform.lossyScale.x;
                    lColliderSize.y = 0.001f;
                    //lColliderSize.z *= lTransform.lossyScale.z;

                    lContactPoint = ClosestPoint(rPoint, lTransform, Vector3.zero, lColliderSize);
                }
                // If the shared mesh isn't readable, we need to use the bounds
                else if (!lCollider.sharedMesh.isReadable)
                {
                    lContactPoint = lCollider.ClosestPointOnBounds(rPoint);
                    Debug.LogWarning(string.Format("{0}'s mesh is not imported as 'Read/Write Enabled' and may not be accurate. For accurate collisions, check 'Read/Write Enabled' on the model's import settings.", lCollider.name));
                }
                // Otherwise, use the octree
                else
                {
                    lContactPoint = MeshExt.ClosestPoint(rPoint, 0f, rCollider.gameObject.transform, lCollider.sharedMesh);
                }
            }

            // Assume no collision
            return lContactPoint;
        }

        /// <summary>
        /// Gets the closest contact point on the collider given our desired position
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <param name="rCollider"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, float rRadius, Collider rCollider, int rCollisionLayers = -1)
        {
            // Ensure our collider is actually on a layer we can collide with
            if (rCollisionLayers > -1 && rCollider != null && rCollider.gameObject != null)
            {
                if (((1 << rCollider.gameObject.layer) & rCollisionLayers) == 0)
                {
                    return Vector3.zero;
                }
            }

            // Find the contact point
            Vector3 lContactPoint = Vector3.zero;

            // Use the box collider
            if (rCollider is BoxCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (BoxCollider)rCollider);
            }
            // Use the sphere collider
            else if (rCollider is SphereCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (SphereCollider)rCollider);
            }
            // Use the capsule collider
            else if (rCollider is CapsuleCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (CapsuleCollider)rCollider);
            }
            // Use the character controller
            else if (rCollider is CharacterController)
            {
                lContactPoint = ClosestPoint(rPoint, (CharacterController)rCollider);
            }
            // Use the terrain collider
            else if (rCollider is TerrainCollider)
            {
                lContactPoint = ClosestPoint(rPoint, (TerrainCollider)rCollider, rRadius, rCollisionLayers);
            }
            // Use the mesh collider
            else if (rCollider is MeshCollider)
            {
                MeshCollider lCollider = (MeshCollider)rCollider;

                // If no mesh, we can stop
                if (lCollider.sharedMesh == null)
                {
                }
                // Planes are expensive with octrees because they aren't really volumes (2d vs 3d)
                // So, we turn the plane into a box collider
                else if (lCollider.sharedMesh.name == "Plane")
                {
                    Transform lTransform = lCollider.transform;

                    Vector3 lColliderSize = lCollider.sharedMesh.bounds.size;
                    //lColliderSize.x *= lTransform.lossyScale.x;
                    lColliderSize.y = 0.001f;
                    //lColliderSize.z *= lTransform.lossyScale.z;

                    lContactPoint = ClosestPoint(rPoint, lTransform, Vector3.zero, lColliderSize);
                }
                // If the shared mesh isn't readable, we need to use the bounds
                else if (!lCollider.sharedMesh.isReadable)
                {
                    lContactPoint = lCollider.ClosestPointOnBounds(rPoint);
                    Debug.LogWarning(string.Format("{0}'s mesh is not imported as 'Read/Write Enabled' and may not be accurate. For accurate collisions, check 'Read/Write Enabled' on the model's import settings.", lCollider.name));
                }
                // Otherwise, use the octree
                else
                {
                    lContactPoint = MeshExt.ClosestPoint(rPoint, rRadius, rCollider.gameObject.transform, lCollider.sharedMesh);
                }
            }

            // Assume no collision
            return lContactPoint;
        }

        /// <summary>
        /// Finds the closest point on a line segment
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rLineStart"></param>
        /// <param name="rLineEnd"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Vector3 rLineStart, Vector3 rLineEnd)
        {
            Vector3 lLine = rLineEnd - rLineStart;

            Vector3 lProject = Vector3.Project(rPoint - rLineStart, lLine);
            Vector3 lAxisPoint = lProject + rLineStart;

            if (Vector3.Dot(lProject, lLine) < 0f)
            {
                lAxisPoint = rLineStart;
            }
            else if (lProject.sqrMagnitude > lLine.sqrMagnitude)
            {
                lAxisPoint = rLineEnd;
            }

            return lAxisPoint;
        }

        /// <summary>
        /// Finds the closest point on a triangle.
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
        public static Vector3 ClosestPoint(ref Vector3 point, ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
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
                return vertex1;
            }

            //Check if P in vertex region outside B
            Vector3 bp = point - vertex2;
            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0.0f && d4 <= d3)
            {
                return vertex2; // barycentric coordinates (0,1,0)
            }

            //Check if P in edge region of AB, if so return projection of P onto AB
            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0.0f && d1 >= 0.0f && d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);
                return vertex1 + v * ab; //Barycentric coordinates (1-v,v,0)
            }

            //Check if P in vertex region outside C
            Vector3 cp = point - vertex3;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0.0f && d5 <= d6)
            {
                return vertex3; //Barycentric coordinates (0,0,1)
            }

            //Check if P in edge region of AC, if so return projection of P onto AC
            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0.0f && d2 >= 0.0f && d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);
                return vertex1 + w * ac; //Barycentric coordinates (1-w,0,w)
            }

            //Check if P in edge region of BC, if so return projection of P onto BC
            float va = d3 * d6 - d5 * d4;
            if (va <= 0.0f && (d4 - d3) >= 0.0f && (d5 - d6) >= 0.0f)
            {
                float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return vertex2 + w * (vertex3 - vertex2); //Barycentric coordinates (0,1-w,w)
            }

            //P inside face region. Compute Q through its barycentric coordinates (u,v,w)
            float denom = 1.0f / (va + vb + vc);
            float v2 = vb * denom;
            float w2 = vc * denom;
            return vertex1 + ab * v2 + ac * w2; //= u*vertex1 + v*vertex2 + w*vertex3, u = va * denom = 1.0f - v - w
        }

        /// <summary>
        /// Finds the closest point on the sphere
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rPosition"></param>
        /// <param name="rRadius"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Vector3 rPosition, float rRadius)
        {
            // Direction from the collider to our position
            Vector3 lDirection = Vector3.Normalize(rPoint - rPosition);

            // Get a good position relative to the collider
            Vector3 lLocalPosition = lDirection * rRadius;

            // Turn that into world space
            return rPosition + lLocalPosition;
        }

        /// <summary>
        /// Finds the closest point on the box
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <param name="rCollider">Collider we're testing</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Transform rTransform, Vector3 rCenter, Vector3 rColliderSize)
        {
            // Cace the transform so we don't do extra GetTransform() calls
            Transform lTransform = rTransform;

            // The transform will fit the point into the right scale
            Vector3 lHalfColliderSize = rColliderSize * 0.5f;

            // Move the world space point to local space
            Vector3 lLocalPosition = (lTransform != null ? lTransform.InverseTransformPoint(rPoint) : rPoint);

            // Check if we're outside the box
            if (lLocalPosition.x < -lHalfColliderSize.x ||
                lLocalPosition.x > lHalfColliderSize.x ||
                lLocalPosition.y < -lHalfColliderSize.y ||
                lLocalPosition.y > lHalfColliderSize.y ||
                lLocalPosition.z < -lHalfColliderSize.z ||
                lLocalPosition.z > lHalfColliderSize.z
               )
            {
                // We're going to force the local point into the box so we can clamp
                // it to the boundary of the box.
                lLocalPosition -= rCenter;

                // Clamp to the box boundary
                lLocalPosition.x = Mathf.Clamp(lLocalPosition.x, -lHalfColliderSize.x, lHalfColliderSize.x);
                lLocalPosition.y = Mathf.Clamp(lLocalPosition.y, -lHalfColliderSize.y, lHalfColliderSize.y);
                lLocalPosition.z = Mathf.Clamp(lLocalPosition.z, -lHalfColliderSize.z, lHalfColliderSize.z);

                // Put the position back where it was
                lLocalPosition += rCenter;
            }
            // If we're inside the box, move to the closest plane
            else
            {
                float lLocalX = lHalfColliderSize.x - Mathf.Abs(lLocalPosition.x);
                float lLocalY = lHalfColliderSize.y - Mathf.Abs(lLocalPosition.y);
                float lLocalZ = lHalfColliderSize.z - Mathf.Abs(lLocalPosition.z);
                if (lLocalX < lLocalY && lLocalX < lLocalZ)
                {
                    lLocalPosition.x = (lLocalPosition.x < 0f ? -lHalfColliderSize.x : lHalfColliderSize.x);
                }
                else if (lLocalY < lLocalX && lLocalY < lLocalZ)
                {
                    lLocalPosition.y = (lLocalPosition.y < 0f ? -lHalfColliderSize.y : lHalfColliderSize.y);
                }
                else
                {
                    lLocalPosition.z = (lLocalPosition.z < 0f ? -lHalfColliderSize.z : lHalfColliderSize.z);
                }
            }

            // Finally, go back to world space
            if (lTransform == null)
            {
                return lLocalPosition;
            }

            return lTransform.TransformPoint(lLocalPosition);
        }

        /// <summary>
        /// Finds the closest point on a capsule
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rLineStart"></param>
        /// <param name="rLineEnd"></param>
        /// <returns></returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Vector3 rLineStart, Vector3 rLineEnd, float rRadius)
        {
            Vector3 lLine = rLineEnd - rLineStart;

            Vector3 lProject = Vector3.Project(rPoint - rLineStart, lLine);
            Vector3 lAxisPoint = lProject + rLineStart;

            if (Vector3.Dot(lProject, lLine) < 0f)
            {
                lAxisPoint = rLineStart;
            }
            else if (lProject.sqrMagnitude > lLine.sqrMagnitude)
            {
                lAxisPoint = rLineEnd;
            }

            return lAxisPoint + ((rPoint - lAxisPoint).normalized * rRadius);
        }

        /// <summary>
        /// Finds the closest point on the sphere collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, SphereCollider rCollider)
        {
            // Cache the transform so we don't do extra GetTransform() calls
            Transform lTransform = rCollider.transform;

            // Direction from the collider to our position
            Vector3 lDirection = Vector3.Normalize(rPoint - (lTransform.position + rCollider.center));

            // Get a point on the sphere's surface that is in the direction of our target point
            Vector3 lLocalPosition = lDirection * (rCollider.radius * lTransform.localScale.x);

            // Turn that into world space
            return (lTransform.position + rCollider.center) + lLocalPosition;
        }

        /// <summary>
        /// Finds the closest point on the capsule collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, CapsuleCollider rCollider)
        {
            Vector3 lClosestPoint = Vector3.zero;

            // Cache the transform so we don't do extra GetTransform() calls
            Transform lTransform = rCollider.transform;

            // Put the point in local coordinates
            Vector3 lLocalPoint = lTransform.InverseTransformPoint(rPoint);

            // Track the heigt so we can compare it
            float lHalfHeight = rCollider.height * 0.5f;

            // Aligned on the x-axis
            if (rCollider.direction == 0)
            {
                // Track the offset for the ends of the collider
                Vector3 lEndOffset = Vector3.right * (lHalfHeight - rCollider.radius);

                // Must be part of the upper end
                if (lLocalPoint.x > rCollider.center.x + lHalfHeight - rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center + lEndOffset, rCollider.radius);
                }
                // Must be part of the lower end
                else if (lLocalPoint.x < rCollider.center.x - lHalfHeight + rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.radius);
                }
                // Must be part of the center
                else
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.center + lEndOffset);
                    lClosestPoint = ClosestPoint(lLocalPoint, lClosestPoint, rCollider.radius);
                }

                // Return the closest point
                return lTransform.TransformPoint(lClosestPoint);
            }
            // Aligned on the y-axis
            else if (rCollider.direction == 1)
            {
                // Track the offset for the ends of the collider
                Vector3 lEndOffset = Vector3.up * (lHalfHeight - rCollider.radius);

                // Must be part of the upper end
                if (lLocalPoint.y > rCollider.center.y + lHalfHeight - rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center + lEndOffset, rCollider.radius);
                }
                // Must be part of the lower end
                else if (lLocalPoint.y < rCollider.center.y - lHalfHeight + rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.radius);
                }
                // Must be part of the center
                else
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.center + lEndOffset);
                    lClosestPoint = ClosestPoint(lLocalPoint, lClosestPoint, rCollider.radius);
                }

                // Return the closest point
                return lTransform.TransformPoint(lClosestPoint);
            }
            // Aligned on the z-axis
            else
            {
                // Track the offset for the ends of the collider
                Vector3 lEndOffset = Vector3.forward * (lHalfHeight - rCollider.radius);

                // Must be part of the upper end
                if (lLocalPoint.z > rCollider.center.z + lHalfHeight - rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center + lEndOffset, rCollider.radius);
                }
                // Must be part of the lower end
                else if (lLocalPoint.z < rCollider.center.z - lHalfHeight + rCollider.radius)
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.radius);
                }
                // Must be part of the center
                else
                {
                    lClosestPoint = ClosestPoint(lLocalPoint, rCollider.center - lEndOffset, rCollider.center + lEndOffset);
                    lClosestPoint = ClosestPoint(lLocalPoint, lClosestPoint, rCollider.radius);
                }

                // Return the closest point
                return lTransform.TransformPoint(lClosestPoint);
            }
        }

        /// <summary>
        /// Finds the closest point on the box collider
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <param name="rCollider">Collider we're testing</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, BoxCollider rCollider)
        {
            // Cace the transform so we don't do extra GetTransform() calls
            Transform lTransform = rCollider.transform;

            // The transform will fit the point into the right scale
            Vector3 lHalfColliderSize = rCollider.size * 0.5f;

            // Move the world space point to local space
            Vector3 lLocalPosition = lTransform.InverseTransformPoint(rPoint);

            // Check if we're outside the box
            if (lLocalPosition.x < -lHalfColliderSize.x ||
                lLocalPosition.x > lHalfColliderSize.x ||
                lLocalPosition.y < -lHalfColliderSize.y ||
                lLocalPosition.y > lHalfColliderSize.y ||
                lLocalPosition.z < -lHalfColliderSize.z ||
                lLocalPosition.z > lHalfColliderSize.z
               )
            {
                // We're going to force the local point into the box so we can clamp
                // it to the boundary of the box.
                lLocalPosition -= rCollider.center;

                // Clamp to the box boundary
                lLocalPosition.x = Mathf.Clamp(lLocalPosition.x, -lHalfColliderSize.x, lHalfColliderSize.x);
                lLocalPosition.y = Mathf.Clamp(lLocalPosition.y, -lHalfColliderSize.y, lHalfColliderSize.y);
                lLocalPosition.z = Mathf.Clamp(lLocalPosition.z, -lHalfColliderSize.z, lHalfColliderSize.z);

                // Put the position back where it was
                lLocalPosition += rCollider.center;
            }
            // If we're inside the box, move to the closest plane
            else
            {
                float lLocalX = lHalfColliderSize.x - Mathf.Abs(lLocalPosition.x);
                float lLocalY = lHalfColliderSize.y - Mathf.Abs(lLocalPosition.y);
                float lLocalZ = lHalfColliderSize.z - Mathf.Abs(lLocalPosition.z);
                if (lLocalX < lLocalY && lLocalX < lLocalZ)
                {
                    lLocalPosition.x = (lLocalPosition.x < 0f ? -lHalfColliderSize.x : lHalfColliderSize.x);
                }
                else if (lLocalY < lLocalX && lLocalY < lLocalZ)
                {
                    lLocalPosition.y = (lLocalPosition.y < 0f ? -lHalfColliderSize.y : lHalfColliderSize.y);
                }
                else
                {
                    lLocalPosition.z = (lLocalPosition.z < 0f ? -lHalfColliderSize.z : lHalfColliderSize.z);
                }
            }

            // Finally, go back to world space
            return lTransform.TransformPoint(lLocalPosition);
        }

        /// <summary>
        /// Finds the closest point on the terrain collider
        /// </summary>
        /// <param name="rPoint">Point to test</param>
        /// <param name="rCollider">Collider we're testing</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, TerrainCollider rCollider, float rRadius = 4f, int rCollisionLayers = -1)
        {
            RaycastHit lHitInfo = new RaycastHit();
            lHitInfo.distance = float.MaxValue;

            for (int i = 0; i < SphericalDirections.Length; i++)
            {
                //com.ootii.Graphics.GraphicsManager.DrawLine(rPoint, rPoint + (SphericalDirections[i] * (lRadius + 0.05f)), Color.cyan);

                RaycastHit lRayHitInfo;
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                if (UnityEngine.Physics.Raycast(rPoint, SphericalDirections[i], out lRayHitInfo, rRadius + 0.05f, rCollisionLayers))
#else
                if (UnityEngine.Physics.Raycast(rPoint, SphericalDirections[i], out lRayHitInfo, rRadius + 0.05f, rCollisionLayers, QueryTriggerInteraction.Ignore))
#endif
                {
                    if (lRayHitInfo.distance < lHitInfo.distance && !IgnoreCollider(lRayHitInfo.collider))
                    {
                        lHitInfo = lRayHitInfo;
                    }
                }
            }

            // If we have a valid hit, return that point
            if (lHitInfo.distance < float.MaxValue)
            {
                return lHitInfo.point;
            }

            // Default value
            return Vector3.zero;
        }

        /// <summary>
        /// Finds the closest point on the terrain collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, Vector3 rDirection, float rRadius, TerrainCollider rCollider, int rCollisionLayers = -1)
        {
            RaycastHit lHitInfo;

            // If there's movement, we can do do a sphere cast
            if (rDirection.sqrMagnitude > 0f)
            {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                if (UnityEngine.Physics.SphereCast(rPoint, rRadius * 0.5f, rDirection, out lHitInfo, rRadius, rCollisionLayers))
#else
                if (UnityEngine.Physics.SphereCast(rPoint, rRadius * 0.5f, rDirection, out lHitInfo, rRadius, rCollisionLayers, QueryTriggerInteraction.Ignore))
#endif
                {
                    if (lHitInfo.collider == rCollider)
                    {
                        // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                        RaycastHit lRayHitInfo;
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                        if (UnityEngine.Physics.Raycast(rPoint, lHitInfo.point - rPoint, out lRayHitInfo, rRadius + 0.01f, rCollisionLayers))
#else
                        if (UnityEngine.Physics.Raycast(rPoint, lHitInfo.point - rPoint, out lRayHitInfo, rRadius + 0.01f, rCollisionLayers, QueryTriggerInteraction.Ignore))
#endif
                        {
                            lHitInfo = lRayHitInfo;
                        }

                        return lHitInfo.point;
                    }
                }
            }

            // If we didn't get a hit, do sphere based ray tests
            lHitInfo = new RaycastHit();
            lHitInfo.distance = float.MaxValue;
            for (int i = 0; i < SphericalDirections.Length; i++)
            {
                //com.ootii.Graphics.GraphicsManager.DrawLine(rPoint, rPoint + (SphericalDirections[i] * (rRadius + 0.05f)), Color.cyan);

                RaycastHit lRayHitInfo;
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                if (UnityEngine.Physics.Raycast(rPoint, SphericalDirections[i], out lRayHitInfo, rRadius + 0.05f, rCollisionLayers))
#else
                if (UnityEngine.Physics.Raycast(rPoint, SphericalDirections[i], out lRayHitInfo, rRadius + 0.05f, rCollisionLayers, QueryTriggerInteraction.Ignore))
#endif
                {
                    if (lRayHitInfo.distance < lHitInfo.distance && !IgnoreCollider(lRayHitInfo.collider))
                    {
                        lHitInfo = lRayHitInfo;
                    }
                }
            }

            // If we have a valid hit, return that point
            if (lHitInfo.distance < float.MaxValue)
            {
                return lHitInfo.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Finds the closest point on the character controller
        /// Note: This function expects a standard Unity Character Controller where the
        /// character only rotates on the y-axis (yaw).
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static Vector3 ClosestPoint(Vector3 rPoint, CharacterController rController)
        {
            Vector3 lClosestPoint = Vector3.zero;

            // Cache the transform so we don't do extra GetTransform() calls
            Transform lTransform = rController.transform;

            // Put the point in local coordinates
            Vector3 lLocalPoint = rPoint - lTransform.position;

            // Track the offset for the ends of the collider
            Vector3 lEndOffset = Vector3.up * ((rController.height * 0.5f) - rController.radius);

            // Must be part of the upper end
            if (lLocalPoint.y > rController.height - rController.radius)
            {
                lClosestPoint = ClosestPoint(lLocalPoint, rController.center + lEndOffset, rController.radius);
            }
            // Must be part of the lower end
            else if (lLocalPoint.y < rController.radius)
            {
                lClosestPoint = ClosestPoint(lLocalPoint, rController.center - lEndOffset, rController.radius);
            }
            // Must be part of the center
            else
            {
                lClosestPoint = ClosestPoint(lLocalPoint, rController.center - lEndOffset, rController.center + lEndOffset);
                lClosestPoint = ClosestPoint(lLocalPoint, lClosestPoint, rController.radius);
            }

            // Return the closest point
            return lClosestPoint + lTransform.position;
        }


        /// <summary>
        /// Gets the closest contact point on the collider given our desired position
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rRadius"></param>
        /// <param name="rCollider"></param>
        /// <returns></returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, float rRadius, Collider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint, int rCollisionLayers = -1)
        {
            // Ensure our collider is actually on a layer we can collide with
            if (rCollisionLayers > -1 && rCollider != null && rCollider.gameObject != null)
            {
                if (((1 << rCollider.gameObject.layer) & rCollisionLayers) == 0)
                {
                    return;
                }
            }

            // Find the contact point
            // Use the box collider
            if (rCollider is BoxCollider)
            {
                ClosestPoints(rStart, rEnd, (BoxCollider)rCollider, ref rLinePoint, ref rColliderPoint);
            }
            // Use the sphere collider
            else if (rCollider is SphereCollider)
            {
                ClosestPoints(rStart, rEnd, (SphereCollider)rCollider, ref rLinePoint, ref rColliderPoint);
            }
            // Use the capsule collider
            else if (rCollider is CapsuleCollider)
            {
                ClosestPoints(rStart, rEnd, (CapsuleCollider)rCollider, ref rLinePoint, ref rColliderPoint);
            }
            // Use the character controller
            else if (rCollider is CharacterController)
            {
                ClosestPoints(rStart, rEnd, (CharacterController)rCollider, ref rLinePoint, ref rColliderPoint);
            }
            // Use the terrain collider
            else if (rCollider is TerrainCollider)
            {
                ClosestPoints(rStart, rEnd, (TerrainCollider)rCollider, ref rLinePoint, ref rColliderPoint, rRadius, rCollisionLayers);
            }
            // Use the mesh collider
            else if (rCollider is MeshCollider)
            {
                ClosestPoints(rStart, rEnd, rRadius, (MeshCollider)rCollider, ref rLinePoint, ref rColliderPoint);
            }
        }

        /// <summary>
        /// Grab the closest points between two line segments.
        /// 
        /// http://geomalgorithms.com/a07-_distance.html
        /// </summary>
        /// <param name="rStart1"></param>
        /// <param name="rEnd1"></param>
        /// <param name="rStart2"></param>
        /// <param name="rEnd2"></param>
        /// <param name="rLine1Point"></param>
        /// <param name="rLine2Point"></param>
        public static void ClosestPoints(Vector3 rStart1, Vector3 rEnd1, Vector3 rStart2, Vector3 rEnd2, ref Vector3 rLine1Point, ref Vector3 rLine2Point)
        {
            Vector3 lLine1 = rEnd1 - rStart1;
            float lLine1Extent = lLine1.magnitude * 0.5f;
            Vector3 lLine1Direction = lLine1.normalized;
            Vector3 lLine1Center = rStart1 + (lLine1Direction * lLine1Extent);
            float lLine1Distance = 0f;

            Vector3 lLine2 = rEnd2 - rStart2;
            float lLine2Extent = lLine2.magnitude * 0.5f;
            Vector3 lLine2Direction = lLine2.normalized;
            Vector3 lLine2Center = rStart2 + (lLine2Direction * lLine2Extent);
            float lLine2Distance = 0f;

            Vector3 lDiff = lLine1Center - lLine2Center;

            float lLine1DotLine2 = -lLine1Direction.Dot(lLine2Direction);
            float lDot1 = lDiff.Dot(lLine1Direction);
            float lDot2 = -lDiff.Dot(lLine2Direction);
            float lFactor = Mathf.Abs(1f - lLine1DotLine2 * lLine1DotLine2);

            // Ensure our lines aren't parallel
            if (lFactor >= EPSILON)
            {
                lLine1Distance = lLine1DotLine2 * lDot2 - lDot1;
                lLine2Distance = lLine1DotLine2 * lDot1 - lDot2;
                float lExtentDet0 = lLine1Extent * lFactor;
                float lExtenttDet1 = lLine2Extent * lFactor;

                if (lLine1Distance >= -lExtentDet0)
                {
                    if (lLine1Distance <= lExtentDet0)
                    {
                        if (lLine2Distance >= -lExtenttDet1)
                        {
                            if (lLine2Distance <= lExtenttDet1)
                            {
                                float invDet = ((float)1) / lFactor;
                                lLine1Distance *= invDet;
                                lLine2Distance *= invDet;
                            }
                            else
                            {
                                lLine2Distance = lLine2Extent;

                                float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                                if (lLine1DistanceTemp < -lLine1Extent)
                                {
                                    lLine1Distance = -lLine1Extent;
                                }
                                else if (lLine1DistanceTemp <= lLine1Extent)
                                {
                                    lLine1Distance = lLine1DistanceTemp;
                                }
                                else
                                {
                                    lLine1Distance = lLine1Extent;
                                }
                            }
                        }
                        else
                        {
                            lLine2Distance = -lLine2Extent;

                            float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                            if (lLine1DistanceTemp < -lLine1Extent)
                            {
                                lLine1Distance = -lLine1Extent;
                            }
                            else if (lLine1DistanceTemp <= lLine1Extent)
                            {
                                lLine1Distance = lLine1DistanceTemp;
                            }
                            else
                            {
                                lLine1Distance = lLine1Extent;
                            }
                        }
                    }
                    else
                    {
                        if (lLine2Distance >= -lExtenttDet1)
                        {
                            if (lLine2Distance <= lExtenttDet1)
                            {
                                lLine1Distance = lLine1Extent;

                                float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                                if (lLine2DistanceTemp < -lLine2Extent)
                                {
                                    lLine2Distance = -lLine2Extent;
                                }
                                else if (lLine2DistanceTemp <= lLine2Extent)
                                {
                                    lLine2Distance = lLine2DistanceTemp;
                                }
                                else
                                {
                                    lLine2Distance = lLine2Extent;
                                }
                            }
                            else
                            {
                                lLine2Distance = lLine2Extent;

                                float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                                if (lLine1DistanceTemp < -lLine1Extent)
                                {
                                    lLine1Distance = -lLine1Extent;
                                }
                                else if (lLine1DistanceTemp <= lLine1Extent)
                                {
                                    lLine1Distance = lLine1DistanceTemp;
                                }
                                else
                                {
                                    lLine1Distance = lLine1Extent;

                                    float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                                    if (lLine2DistanceTemp < -lLine2Extent)
                                    {
                                        lLine2Distance = -lLine2Extent;
                                    }
                                    else if (lLine2DistanceTemp <= lLine2Extent)
                                    {
                                        lLine2Distance = lLine2DistanceTemp;
                                    }
                                    else
                                    {
                                        lLine2Distance = lLine2Extent;
                                    }
                                }
                            }
                        }
                        else
                        {
                            lLine2Distance = -lLine2Extent;

                            float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                            if (lLine1DistanceTemp < -lLine1Extent)
                            {
                                lLine1Distance = -lLine1Extent;
                            }
                            else if (lLine1DistanceTemp <= lLine1Extent)
                            {
                                lLine1Distance = lLine1DistanceTemp;
                            }
                            else
                            {
                                lLine1Distance = lLine1Extent;

                                float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                                if (lLine2DistanceTemp > lLine2Extent)
                                {
                                    lLine2Distance = lLine2Extent;
                                }
                                else if (lLine2DistanceTemp >= -lLine2Extent)
                                {
                                    lLine2Distance = lLine2DistanceTemp;
                                }
                                else
                                {
                                    lLine2Distance = -lLine2Extent;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (lLine2Distance >= -lExtenttDet1)
                    {
                        if (lLine2Distance <= lExtenttDet1)
                        {
                            lLine1Distance = -lLine1Extent;

                            float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                            if (lLine2DistanceTemp < -lLine2Extent)
                            {
                                lLine2Distance = -lLine2Extent;
                            }
                            else if (lLine2DistanceTemp <= lLine2Extent)
                            {
                                lLine2Distance = lLine2DistanceTemp;
                            }
                            else
                            {
                                lLine2Distance = lLine2Extent;
                            }
                        }
                        else
                        {
                            lLine2Distance = lLine2Extent;

                            float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                            if (lLine1DistanceTemp > lLine1Extent)
                            {
                                lLine1Distance = lLine1Extent;
                            }
                            else if (lLine1DistanceTemp >= -lLine1Extent)
                            {
                                lLine1Distance = lLine1DistanceTemp;
                            }
                            else
                            {
                                lLine1Distance = -lLine1Extent;

                                float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                                if (lLine2DistanceTemp < -lLine2Extent)
                                {
                                    lLine2Distance = -lLine2Extent;
                                }
                                else if (lLine2DistanceTemp <= lLine2Extent)
                                {
                                    lLine2Distance = lLine2DistanceTemp;
                                }
                                else
                                {
                                    lLine2Distance = lLine2Extent;
                                }
                            }
                        }
                    }
                    else
                    {
                        lLine2Distance = -lLine2Extent;

                        float lLine1DistanceTemp = -(lLine1DotLine2 * lLine2Distance + lDot1);
                        if (lLine1DistanceTemp > lLine1Extent)
                        {
                            lLine1Distance = lLine1Extent;
                        }
                        else if (lLine1DistanceTemp >= -lLine1Extent)
                        {
                            lLine1Distance = lLine1DistanceTemp;
                        }
                        else
                        {
                            lLine1Distance = -lLine1Extent;

                            float lLine2DistanceTemp = -(lLine1DotLine2 * lLine1Distance + lDot2);
                            if (lLine2DistanceTemp < -lLine2Extent)
                            {
                                lLine2Distance = -lLine2Extent;
                            }
                            else if (lLine2DistanceTemp <= lLine2Extent)
                            {
                                lLine2Distance = lLine2DistanceTemp;
                            }
                            else
                            {
                                lLine2Distance = lLine2Extent;
                            }
                        }
                    }
                }
            }
            // If the lines are parallel, we'll attempt to still find a result
            else
            {
                float lTotalExtents = lLine1Extent + lLine2Extent;
                float lSign = (lLine1DotLine2 > 0f ? -1f : 1f);
                float lAverage = (lDot1 - lSign * lDot2) * 0.5f;
                float lInvertAverage = -lAverage;
                if (lInvertAverage < -lTotalExtents)
                {
                    lInvertAverage = -lTotalExtents;
                }
                else if (lInvertAverage > lTotalExtents)
                {
                    lInvertAverage = lTotalExtents;
                }

                lLine2Distance = -lSign * lInvertAverage * lLine2Extent / lTotalExtents;
                lLine1Distance = lInvertAverage + lSign * lLine2Distance;
            }

            // Create the final point
            rLine1Point = lLine1Center + (lLine1Direction * lLine1Distance);
            rLine2Point = lLine2Center + (lLine2Direction * lLine2Distance);
        }

        /// <summary>
        /// Finds the closest points between a line segment and a sphere
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, Vector3 rPosition, float rRadius, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Vector3 lLine = rEnd - rStart;
            Vector3 lLineDirection = lLine.normalized;

            Vector3 lToSphere = rPosition - rStart;

            float lCosAngle = Vector3.Dot(lToSphere, lLineDirection);
            Vector3 lProject = lLineDirection * Mathf.Max(lCosAngle, 0f);
            float lAdjLength = Mathf.Min(lProject.magnitude, lLine.magnitude);

            rLinePoint = rStart + (lLineDirection * lAdjLength);

            lToSphere = rPosition - rLinePoint;
            rColliderPoint = rLinePoint + (lToSphere.normalized * (lToSphere.magnitude - rRadius));
        }

        /// <summary>
        /// Finds the closest points between a line segment and a box
        /// </summary>
        /// <param name="rStart">Start of the line segment</param>
        /// <param name="rEnd">End of the line segment</param>
        /// <param name="rCollider">Box collider</param>
        /// <param name="rLinePoint">Closest point on the line</param>
        /// <param name="rColliderPoint">Closest point on the box collider</param>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, Transform rTransform, Vector3 rCenter, Vector3 rColliderSize, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Vector3 lLine = rEnd - rStart;
            Vector3 lLineDirection = lLine.normalized;
            Vector3 lLineCenter = rStart + (lLineDirection * (lLine.magnitude * 0.5f));
            Vector3 lDiff = lLineCenter - rTransform.position;

            Vector3 lBoxXAxis = rTransform.right;
            Vector3 lBoxYAxis = rTransform.up;
            Vector3 lBoxZAxis = rTransform.forward;
            Vector3 lBoxPoint = new Vector3(lDiff.Dot(lBoxXAxis), lDiff.Dot(lBoxYAxis), lDiff.Dot(lBoxZAxis));
            Vector3 lBoxDirection = new Vector3(lLineDirection.Dot(lBoxXAxis), lLineDirection.Dot(lBoxYAxis), lLineDirection.Dot(lBoxZAxis));

            Vector3 lBoxExtents = rColliderSize * 0.5f;
            lBoxExtents.x *= rTransform.lossyScale.x;
            lBoxExtents.y *= rTransform.lossyScale.y;
            lBoxExtents.z *= rTransform.lossyScale.z;

            // We don't want negative components in the direction vector, so invert
            if (lBoxDirection.x < 0f)
            {
                lBoxPoint.x = -lBoxPoint.x;
                lBoxDirection.x = -lBoxDirection.x;
            }

            if (lBoxDirection.y < 0f)
            {
                lBoxPoint.y = -lBoxPoint.y;
                lBoxDirection.y = -lBoxDirection.y;
            }

            if (lBoxDirection.z < 0f)
            {
                lBoxPoint.z = -lBoxPoint.z;
                lBoxDirection.z = -lBoxDirection.z;
            }

            float lLineDistance = 0f;

            if (lBoxDirection.x > 0f)
            {
                if (lBoxDirection.y > 0f)
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, 1, ref lLineDistance);
                    }
                }
                else
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, 2, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, ref lLineDistance);
                    }
                }
            }
            else
            {
                if (lBoxDirection.y > 0f)
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 1, 2, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 1, ref lLineDistance);
                    }
                }
                else
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 2, ref lLineDistance);
                    }
                    else
                    {
                        lLineDistance = 0f;
                    }
                }
            }

            // Compute closest point on line.
            rLinePoint = lLineCenter + (lLineDirection * lLineDistance);

            float lLineExtent = lLine.magnitude / 2f;
            if (lLineDistance >= -lLineExtent)
            {
                if (lLineDistance > lLineExtent)
                {
                    rLinePoint = rEnd;
                }
            }
            else
            {
                rLinePoint = rStart;
            }

            rColliderPoint = ClosestPoint(rLinePoint, rTransform, rCenter, rColliderSize);
        }

        /// <summary>
        /// Finds the closest points between a line segment and a sphere collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, SphereCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Transform lTransform = rCollider.transform;
            float lRadius = rCollider.radius * lTransform.lossyScale.x;

            Vector3 lLine = rEnd - rStart;
            Vector3 lLineDirection = lLine.normalized;

            Vector3 lToSphere = (lTransform.position + rCollider.center) - rStart;

            float lCosAngle = Vector3.Dot(lToSphere, lLineDirection);
            Vector3 lProject = lLineDirection * Mathf.Max(lCosAngle, 0f);
            float lAdjLength = Mathf.Min(lProject.magnitude, lLine.magnitude);

            rLinePoint = rStart + (lLineDirection * lAdjLength);

            lToSphere = (lTransform.position + rCollider.center) - rLinePoint;
            rColliderPoint = rLinePoint + (lToSphere.normalized * (lToSphere.magnitude - lRadius));
        }

        /// <summary>
        /// Finds the closest points between a line segment and a capsule collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, CapsuleCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Transform lTransform = rCollider.transform;

            float lHeight = rCollider.height; // * lTransform.lossyScale.y;
            float lRadius = rCollider.radius; // * lTransform.lossyScale.x;

            Vector3 lEndOffset = Vector3.zero;
            lEndOffset[rCollider.direction] = (lHeight / 2f) - lRadius;

            Vector3 lCapsuleStart = lTransform.TransformPoint(rCollider.center - lEndOffset);
            Vector3 lCapsuleEnd = lTransform.TransformPoint(rCollider.center + lEndOffset);

            ClosestPoints(rStart, rEnd, lCapsuleStart, lCapsuleEnd, ref rLinePoint, ref rColliderPoint);

            Vector3 lToLine = rLinePoint - rColliderPoint;
            rColliderPoint = rColliderPoint + (lToLine.normalized * (lRadius * lTransform.lossyScale.x));
        }

        /// <summary>
        /// Finds the closest points between a line segment and a box collider
        /// </summary>
        /// <param name="rStart">Start of the line segment</param>
        /// <param name="rEnd">End of the line segment</param>
        /// <param name="rCollider">Box collider</param>
        /// <param name="rLinePoint">Closest point on the line</param>
        /// <param name="rColliderPoint">Closest point on the box collider</param>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, BoxCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Transform lTransform = rCollider.transform;

            Vector3 lLine = rEnd - rStart;
            Vector3 lLineDirection = lLine.normalized;
            Vector3 lLineCenter = rStart + (lLineDirection * (lLine.magnitude * 0.5f));
            Vector3 lDiff = lLineCenter - lTransform.position;

            Vector3 lBoxXAxis = lTransform.right;
            Vector3 lBoxYAxis = lTransform.up;
            Vector3 lBoxZAxis = lTransform.forward;
            Vector3 lBoxPoint = new Vector3(lDiff.Dot(lBoxXAxis), lDiff.Dot(lBoxYAxis), lDiff.Dot(lBoxZAxis));
            Vector3 lBoxDirection = new Vector3(lLineDirection.Dot(lBoxXAxis), lLineDirection.Dot(lBoxYAxis), lLineDirection.Dot(lBoxZAxis));

            // We do this since we're not using the transform
            Vector3 lBoxExtents = rCollider.size * 0.5f;
            lBoxExtents.x *= lTransform.lossyScale.x;
            lBoxExtents.y *= lTransform.lossyScale.y;
            lBoxExtents.z *= lTransform.lossyScale.z;

            // We don't want negative components in the direction vector, so invert
            if (lBoxDirection.x < 0f)
            {
                lBoxPoint.x = -lBoxPoint.x;
                lBoxDirection.x = -lBoxDirection.x;
            }

            if (lBoxDirection.y < 0f)
            {
                lBoxPoint.y = -lBoxPoint.y;
                lBoxDirection.y = -lBoxDirection.y;
            }

            if (lBoxDirection.z < 0f)
            {
                lBoxPoint.z = -lBoxPoint.z;
                lBoxDirection.z = -lBoxDirection.z;
            }

            float lLineDistance = 0f;

            if (lBoxDirection.x > 0f)
            {
                if (lBoxDirection.y > 0f)
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, 1, ref lLineDistance);
                    }
                }
                else
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, 2, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 0, ref lLineDistance);
                    }
                }
            }
            else
            {
                if (lBoxDirection.y > 0f)
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 1, 2, ref lLineDistance);
                    }
                    else
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 1, ref lLineDistance);
                    }
                }
                else
                {
                    if (lBoxDirection.z > 0f)
                    {
                        GetLineDistanceFromBoxExtent(ref lBoxExtents, ref lBoxPoint, ref lBoxDirection, 2, ref lLineDistance);
                    }
                    else
                    {
                        lLineDistance = 0f;
                    }
                }
            }

            // Compute closest point on line.
            rLinePoint = lLineCenter + (lLineDirection * lLineDistance);

            float lLineExtent = lLine.magnitude / 2f;
            if (lLineDistance >= -lLineExtent)
            {
                if (lLineDistance > lLineExtent)
                {
                    rLinePoint = rEnd;
                }
            }
            else
            {
                rLinePoint = rStart;
            }

            rColliderPoint = ClosestPoint(rLinePoint, rCollider);
        }

        /// <summary>
        /// Finds the closest points between a line segment and a character collider
        /// Note: This function expects a standard Unity Character Controller where the
        /// character only rotates on the y-axis (yaw).
        /// </summary>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, CharacterController rController, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            Transform lTransform = rController.transform;

            Vector3 lEndOffset = Vector3.zero;
            lEndOffset[1] = (rController.height / 2f) - rController.radius;

            Vector3 lCapsuleStart = lTransform.TransformPoint(rController.center - lEndOffset);
            Vector3 lCapsuleEnd = lTransform.TransformPoint(rController.center + lEndOffset);

            ClosestPoints(rStart, rEnd, lCapsuleStart, lCapsuleEnd, ref rLinePoint, ref rColliderPoint);

            Vector3 lToCapsule = rColliderPoint - rLinePoint;
            rColliderPoint = rLinePoint + (lToCapsule.normalized * (lToCapsule.magnitude - rController.radius));
        }


        /// <summary>
        /// Finds the closest points between a line segment and a terrain collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, TerrainCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint, float rRadius = 4f, int rCollisionLayers = -1)
        {
            // Taking a new approach to finding the closest point on terrain
            float lStep = rRadius;
            float lClosestDistance = float.MaxValue;

            float lLineLength = (rEnd - rStart).magnitude;
            Vector3 lLineDirection = (rEnd - rStart).normalized;

            for (float lStepped = 0; lStepped < lLineLength + lStep; lStepped += lStep)
            {
                if (lStepped > lLineLength) { lStepped = lLineLength; }

                Vector3 lPoint = rStart + (lLineDirection * lStepped);
                Vector3 lColliderPoint = ClosestPoint(lPoint, rRadius, rCollider, rCollisionLayers);

                if (lColliderPoint.sqrMagnitude > 0f)
                {
                    float lColliderPointDistance = (lColliderPoint - lPoint).sqrMagnitude;
                    if (lColliderPointDistance < lClosestDistance)
                    {
                        rLinePoint = lPoint;
                        rColliderPoint = lColliderPoint;

                        lClosestDistance = lColliderPointDistance;
                    }
                }

                if (lStepped == lLineLength) { break; }
            }
        }
        
        /// <summary>
        /// Finds the closest points between a line segment and a terrain collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, Vector3 rMovement, TerrainCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint, float rRadius = 4f, int rCollisionLayers = -1)
        {
            if (rMovement.sqrMagnitude > 0f)
            {
                Vector3 lDirection = rMovement.normalized;
                float lDistance = rMovement.magnitude;

                RaycastHit lHitInfo;
                if (UnityEngine.Physics.CapsuleCast(rStart, rEnd, rRadius, lDirection, out lHitInfo, lDistance, rCollisionLayers))
                {
                    if (lHitInfo.collider == rCollider)
                    {
                        rColliderPoint = lHitInfo.point;
                        rLinePoint = ClosestPoint(rColliderPoint, rStart, rEnd);

                        return;
                    }
                }
            }

            // Taking a new approach to finding the closest point on terrain
            float lStep = rRadius;
            float lClosestDistance = float.MaxValue;

            float lLineLength = (rEnd - rStart).magnitude;
            Vector3 lLineDirection = (rEnd - rStart).normalized;

            for (float lStepped = 0; lStepped < lLineLength + lStep; lStepped += lStep)
            {
                if (lStepped > lLineLength) { lStepped = lLineLength; }

                Vector3 lPoint = rStart + (lLineDirection * lStepped);
                Vector3 lColliderPoint = ClosestPoint(lPoint, rMovement, rRadius, rCollider, rCollisionLayers);

                if (lColliderPoint.sqrMagnitude > 0f)
                {
                    float lColliderPointDistance = (lColliderPoint - lPoint).sqrMagnitude;
                    if (lColliderPointDistance < lClosestDistance)
                    {
                        rLinePoint = lPoint;
                        rColliderPoint = lColliderPoint;

                        lClosestDistance = lColliderPointDistance;
                    }
                }

                if (lStepped == lLineLength) { break; }
            }
        }

        /// <summary>
        /// Finds the closest points between a line segment and a mesh collider
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static void ClosestPoints(Vector3 rStart, Vector3 rEnd, float rRadius, MeshCollider rCollider, ref Vector3 rLinePoint, ref Vector3 rColliderPoint)
        {
            if (rCollider == null || rCollider.sharedMesh == null) { return; }

            // Planes are expensive with octrees because they aren't really volumes (2d vs 3d)
            // So, we turn the plane into a box collider
            if (rCollider.sharedMesh.name == "Plane")
            {
                Transform lTransform = rCollider.transform;

                Vector3 lColliderSize = rCollider.sharedMesh.bounds.size;
                //lSize.x *= lTransform.lossyScale.x;
                lColliderSize.y = 0.001f;
                //lSize.z *= lTransform.lossyScale.z;

                ClosestPoints(rStart, rEnd, lTransform, Vector3.zero, lColliderSize, ref rLinePoint, ref rColliderPoint);

                return;
            }

            Vector3 lLine = rEnd - rStart;
            float lLineLength = lLine.magnitude;

            if (lLineLength == 0f)
            {
                rLinePoint = rStart;

                if (!rCollider.sharedMesh.isReadable)
                {
                    rColliderPoint = rCollider.ClosestPointOnBounds(rStart);
                    Debug.LogWarning(string.Format("{0}'s mesh is not imported as 'Read/Write Enabled' and may not be accurate. For accurate collisions, check 'Read/Write Enabled' on the model's import settings.", rCollider.name));
                }
                else
                {
                    rColliderPoint = MeshExt.ClosestPoint(rStart, rRadius, rCollider.gameObject.transform, rCollider.sharedMesh);
                }
            }
            else
            {
                float lStep = rRadius * 0.5f;
                float lClosestDistance = float.MaxValue;

                Vector3 lLineDirection = lLine.normalized;
                for (float lStepped = 0f; lStepped < lLineLength + lStep; lStepped += lStep)
                {
                    if (lStepped > lLineLength) { lStepped = lLineLength; }

                    Vector3 lPoint = rStart + (lLineDirection * lStepped);

                    Vector3 lColliderPoint;
                    if (!rCollider.sharedMesh.isReadable)
                    {
                        lColliderPoint = rCollider.ClosestPointOnBounds(lPoint);
                        Debug.LogWarning(string.Format("{0}'s mesh is not imported as 'Read/Write Enabled' and may not be accurate. For accurate collisions, check 'Read/Write Enabled' on the model's import settings.", rCollider.name));
                    }
                    else
                    {
                        lColliderPoint = MeshExt.ClosestPoint(lPoint, rRadius, rCollider.gameObject.transform, rCollider.sharedMesh);
                    }

                    if (lColliderPoint.sqrMagnitude > 0f)
                    {
                        float lColliderPointDistance = (lColliderPoint - lPoint).sqrMagnitude;
                        if (lColliderPointDistance < lClosestDistance)
                        {
                            rLinePoint = lPoint;
                            rColliderPoint = lColliderPoint;

                            lClosestDistance = lColliderPointDistance;
                        }
                    }

                    if (lStepped == lLineLength) { break; }
                }
            }

            //Graphics.GraphicsManager.DrawPoint(rLinePoint, Color.yellow, null, 2f);
            //Graphics.GraphicsManager.DrawPoint(rColliderPoint, Color.green, null, 2f);

            //// Otherwise, do the mesh collider test
            //Vector3 lStartContactPoint = MeshExt.ClosestPoint(rStart, rRadius, rCollider.gameObject.transform, rCollider.sharedMesh);
            //Vector3 lEndContactPoint = MeshExt.ClosestPoint(rEnd, rRadius, rCollider.gameObject.transform, rCollider.sharedMesh);


            //if (lStartContactPoint.sqrMagnitude > 0f)
            //{
            //    Graphics.GraphicsManager.DrawPoint(lStartContactPoint, Color.yellow, null, 2f);
            //}

            //if (lEndContactPoint.sqrMagnitude > 0f)
            //{
            //    Graphics.GraphicsManager.DrawPoint(lStartContactPoint, Color.green, null, 2f);
            //}


            //float lStartMag = lStartContactPoint.sqrMagnitude;
            //float lEndMag = lEndContactPoint.sqrMagnitude;

            //if (lStartMag > 0f && lEndMag > 0f)
            //{
            //    //if (Vector3.Distance(lStartContactPoint, lEndContactPoint) < EPSILON)
            //    //{
            //    //    rLinePoint = rStart;
            //    //    rColliderPoint = lStartContactPoint;
            //    //}
            //    //else
            //    //{
            //    //    ClosestPoints(rStart, rEnd, lStartContactPoint, lEndContactPoint, ref rLinePoint, ref rColliderPoint);
            //    //}

            //    float lStartDistance = Vector3.Distance(lStartContactPoint, rStart);
            //    float lEndDistance = Vector3.Distance(lEndContactPoint, rEnd);
            //    if (lStartDistance <= lEndDistance)
            //    {
            //        rLinePoint = rStart;
            //        rColliderPoint = lStartContactPoint;
            //    }
            //    else
            //    {
            //        rLinePoint = rEnd;
            //        rColliderPoint = lEndContactPoint;
            //    }
            //}
            //else if (lStartMag > 0f)
            //{
            //    rLinePoint = rStart;
            //    rColliderPoint = lStartContactPoint;
            //}
            //else if (lEndMag > 0f)
            //{
            //    rLinePoint = rEnd;
            //    rColliderPoint = lEndContactPoint;
            //}
        }

        #endregion

        #region Ray Intersect Functions

        /// <summary>
        /// Determine if a ray intersects a sphere
        /// http://www.lighthouse3d.com/tutorials/maths/ray-sphere-intersection/
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rSphereCenter"></param>
        /// <param name="rSphereRadius"></param>
        /// <returns></returns>
        public static bool RaySphereIntersect(Vector3 rRayStart, Vector3 rRayDirection, Vector3 rSphereCenter, float rSphereRadius) //; Sphere&amp; sphere, float&amp; tmin, float&amp; tmax)
        {
            Vector3 lToObject = rSphereCenter - rRayStart;

            // Determine if the sphere is behind the ray
            float lRaySphereDot = Vector3.Dot(rRayDirection, lToObject);

            // NOTE: This assumes the ray can't start inside the sphere!!
            if (lRaySphereDot <= 0) { return false; }

            // Project the sphere's center onto the ray
            Vector3 lProjectedSphereCenter = Vector3.Project(lToObject, rRayDirection);

            // If the distance of this projections is larger than the radius, no intersect
            if (Vector3.Distance(lToObject, lProjectedSphereCenter) > rSphereRadius) { return false; }

            // There's at least one contact point
            return true;
        }

        #endregion

        #region Line Intersect Functions

        /// <summary>
        /// Determine the intersection point of the line and a plane
        /// </summary>
        /// <param name="rLineStart"></param>
        /// <param name="rLineEnd"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static bool LinePlaneIntersect(Vector3 rLineStart, Vector3 rLineEnd, Plane rPlane)
        {
            //float lDistance = 0f;

            Vector3 lLine = (rLineEnd - rLineStart);

            float lDenominator = Vector3.Dot(rPlane.normal, lLine);
            if (Mathf.Abs(lDenominator) > EPSILON)
            {
                float t = -(Vector3.Dot(rPlane.normal, rLineStart) + rPlane.distance) / lDenominator;
                if (t < 0.0f || t > 1.0f)
                {
                    //lDistance = 0.0f;
                    return false;
                }

                //lDistance = t;
                return true;
            }
            else
            {
                // parallel - return false even if it's in the plane
                //lDistance = 0.0f;
                return false;
            }
        }

        /// <summary>
        /// Determine if a ray intersects a sphere
        /// http://www.lighthouse3d.com/tutorials/maths/ray-sphere-intersection/
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rSphereCenter"></param>
        /// <param name="rSphereRadius"></param>
        /// <returns></returns>
        public static bool LineSphereIntersect(Vector3 rLineStart, Vector3 rLineEnd, Vector3 rSphereCenter, float rSphereRadius)
        {
            Vector3 lLine = (rLineEnd - rLineStart);

            float lLineLength = lLine.magnitude;
            Vector3 lLineDirection = lLine.normalized;

            // Grab the direction to the object
            Vector3 lToObject = rSphereCenter - rLineStart;

            // Determine if the sphere is behind the ray
            float lRaySphereDot = Vector3.Dot(lLineDirection, lToObject);

            // NOTE: This assumes the ray can't start inside the sphere!!
            if (lRaySphereDot <= 0) { return false; }

            // Project the sphere's center onto the ray
            Vector3 lProjectedPoint = Vector3.Project(lToObject, lLineDirection);
            float lProjectedDistance = Vector3.Distance(lToObject, lProjectedPoint);

            // If the distance of this projection to the sphere's center is larger than the radius, no intersect
            if (lProjectedDistance > rSphereRadius)
            {
                return false;
            }
            // If the distance (roughly) equals the radius, there is only one contact point
            else if (Mathf.Abs(lProjectedDistance - rSphereRadius) < EPSILON)
            {
                float lDistance = Vector3.Distance(lProjectedPoint, rLineStart);
                if (lDistance > lLineLength) { return false; }
            }
            // In this case, we have two contact points
            else
            {
                // b^2 = c^2 - a^2
                float lHitToProjected = Mathf.Sqrt((rSphereRadius * rSphereRadius) - (lProjectedDistance * lProjectedDistance));
                if ((lProjectedPoint.magnitude - lHitToProjected) > lLineLength) { return false; }
            }

            // There's at least one contact point
            return true;
        }

        /// <summary>
        /// For now, this is a bit of a hack. This is really a OABBox test
        /// </summary>
        /// <param name="rLineStart"></param>
        /// <param name="rLineEnd"></param>
        /// <param name="rTransform"></param>
        /// <param name="rHeight"></param>
        /// <param name="rRadius"></param>
        /// <returns></returns>
        public static bool LineCylinderIntersect(Vector3 rLineStart, Vector3 rLineEnd, Transform rTransform, float rHeight, float rRadius)
        {
            Matrix4x4 lToWorldSpace = Matrix4x4.TRS(rTransform.position, rTransform.rotation, Vector3.one);
            Matrix4x4 lToObjectSpace = lToWorldSpace.inverse;

            // Move the world space line to local space
            Vector3 lLineStart = lToObjectSpace.MultiplyPoint(rLineStart);
            Vector3 lLineEnd = lToObjectSpace.MultiplyPoint(rLineEnd);
            Vector3 lLineHalf = (lLineEnd - lLineStart) * 0.5f;
            Vector3 lLineHalfAbs = new Vector3(Mathf.Abs(lLineHalf.x), Mathf.Abs(lLineHalf.y), Mathf.Abs(lLineHalf.z));

            // Determine the box dimensions
            Vector3 lBoxMax = new Vector3(rRadius, rHeight * 0.5f, rRadius);
            Vector3 lBoxMin = -lBoxMax;
            Vector3 lBoxCenter = (lBoxMax - lBoxMin) * 0.5f;
            Vector3 lClosest = lLineStart + lLineHalf - ((lBoxMin + lBoxMax) * 0.5f);

            // Seperating axis tests
            if (Mathf.Abs(lClosest.x) > lBoxCenter.x + lLineHalfAbs.x) { return false; }
            if (Mathf.Abs(lClosest.y) > lBoxCenter.y + lLineHalfAbs.y) { return false; }
            if (Mathf.Abs(lClosest.z) > lBoxCenter.z + lLineHalfAbs.z) { return false; }

            if (Mathf.Abs(lLineHalf.y * lClosest.z - lLineHalf.z * lClosest.y) > (lBoxCenter.y * lLineHalfAbs.z) + (lBoxCenter.z * lLineHalfAbs.y) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.z * lClosest.x - lLineHalf.x * lClosest.z) > (lBoxCenter.z * lLineHalfAbs.x) + (lBoxCenter.x * lLineHalfAbs.z) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.x * lClosest.y - lLineHalf.y * lClosest.x) > (lBoxCenter.x * lLineHalfAbs.y) + (lBoxCenter.y * lLineHalfAbs.x) + EPSILON) { return false; };

            // We must collide
            return true;
        }

        /// <summary>
        /// For now, this is a bit of a hack. This is really a OABBox test
        /// </summary>
        /// <param name="rLineStart"></param>
        /// <param name="rLineEnd"></param>
        /// <param name="rTransform"></param>
        /// <param name="rHeight"></param>
        /// <param name="rRadius"></param>
        /// <returns></returns>
        public static bool LineCylinderFromBaseIntersect(Vector3 rLineStart, Vector3 rLineEnd, Transform rTransform, float rHeight, float rRadius)
        {
            Matrix4x4 lToWorldSpace = Matrix4x4.TRS(rTransform.position + (rTransform.up * -(rHeight / 2f)), rTransform.rotation, Vector3.one);
            Matrix4x4 lToObjectSpace = lToWorldSpace.inverse;

            // Move the world space line to local space
            Vector3 lLineStart = lToObjectSpace.MultiplyPoint(rLineStart);
            Vector3 lLineEnd = lToObjectSpace.MultiplyPoint(rLineEnd);
            Vector3 lLineHalf = (lLineEnd - lLineStart) * 0.5f;
            Vector3 lLineHalfAbs = new Vector3(Mathf.Abs(lLineHalf.x), Mathf.Abs(lLineHalf.y), Mathf.Abs(lLineHalf.z));

            // Determine the box dimensions
            Vector3 lBoxMax = new Vector3(rRadius, rHeight * 0.5f, rRadius);
            Vector3 lBoxMin = -lBoxMax;
            Vector3 lBoxCenter = (lBoxMax - lBoxMin) * 0.5f;
            Vector3 lClosest = lLineStart + lLineHalf - ((lBoxMin + lBoxMax) * 0.5f);

            // Seperating axis tests
            if (Mathf.Abs(lClosest.x) > lBoxCenter.x + lLineHalfAbs.x) { return false; }
            if (Mathf.Abs(lClosest.y) > lBoxCenter.y + lLineHalfAbs.y) { return false; }
            if (Mathf.Abs(lClosest.z) > lBoxCenter.z + lLineHalfAbs.z) { return false; }

            if (Mathf.Abs(lLineHalf.y * lClosest.z - lLineHalf.z * lClosest.y) > (lBoxCenter.y * lLineHalfAbs.z) + (lBoxCenter.z * lLineHalfAbs.y) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.z * lClosest.x - lLineHalf.x * lClosest.z) > (lBoxCenter.z * lLineHalfAbs.x) + (lBoxCenter.x * lLineHalfAbs.z) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.x * lClosest.y - lLineHalf.y * lClosest.x) > (lBoxCenter.x * lLineHalfAbs.y) + (lBoxCenter.y * lLineHalfAbs.x) + EPSILON) { return false; };

            // We must collide
            return true;
        }

        /// <summary>
        /// Determines if the line collides with an object-aligned bounding box
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static bool LineBoxIntersect(Vector3 rLineStart, Vector3 rLineEnd, Transform rTransform, float rWidth, float rHeight, float rDepth)
        {
            Matrix4x4 lToWorldSpace = Matrix4x4.TRS(rTransform.position, rTransform.rotation, Vector3.one);
            Matrix4x4 lToObjectSpace = lToWorldSpace.inverse;

            // Move the world space line to local space
            Vector3 lLineStart = lToObjectSpace.MultiplyPoint(rLineStart);
            Vector3 lLineEnd = lToObjectSpace.MultiplyPoint(rLineEnd);
            Vector3 lLineHalf = (lLineEnd - lLineStart) * 0.5f;
            Vector3 lLineHalfAbs = new Vector3(Mathf.Abs(lLineHalf.x), Mathf.Abs(lLineHalf.y), Mathf.Abs(lLineHalf.z));

            // Determine the box dimensions
            Vector3 lBoxMax = new Vector3(rWidth * 0.5f, rHeight * 0.5f, rDepth * 0.5f);
            Vector3 lBoxMin = -lBoxMax;
            Vector3 lBoxCenter = (lBoxMax - lBoxMin) * 0.5f;
            Vector3 lClosest = lLineStart + lLineHalf - ((lBoxMin + lBoxMax) * 0.5f);

            // Seperating axis tests
            if (Mathf.Abs(lClosest.x) > lBoxCenter.x + lLineHalfAbs.x) { return false; }
            if (Mathf.Abs(lClosest.y) > lBoxCenter.y + lLineHalfAbs.y) { return false; }
            if (Mathf.Abs(lClosest.z) > lBoxCenter.z + lLineHalfAbs.z) { return false; }

            if (Mathf.Abs(lLineHalf.y * lClosest.z - lLineHalf.z * lClosest.y) > (lBoxCenter.y * lLineHalfAbs.z) + (lBoxCenter.z * lLineHalfAbs.y) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.z * lClosest.x - lLineHalf.x * lClosest.z) > (lBoxCenter.z * lLineHalfAbs.x) + (lBoxCenter.x * lLineHalfAbs.z) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.x * lClosest.y - lLineHalf.y * lClosest.x) > (lBoxCenter.x * lLineHalfAbs.y) + (lBoxCenter.y * lLineHalfAbs.x) + EPSILON) { return false; };

            // We must collide
            return true;
        }

        /// <summary>
        /// Determines if the line collides with an object-aligned bounding box
        /// </summary>
        /// <param name="rCollider">Collider we're testing</param>
        /// <param name="rPosition">Desired position</param>
        /// <returns>Contact point</returns>
        public static bool LineBoxFromBaseIntersect(Vector3 rLineStart, Vector3 rLineEnd, Vector3 rPosition, Quaternion rRotation, float rWidth, float rHeight, float rDepth)
        {
            Matrix4x4 lToWorldSpace = Matrix4x4.TRS(rPosition, rRotation, Vector3.one);
            Matrix4x4 lToObjectSpace = lToWorldSpace.inverse;

            // Move the world space line to local space
            Vector3 lLineStart = lToObjectSpace.MultiplyPoint(rLineStart);
            Vector3 lLineEnd = lToObjectSpace.MultiplyPoint(rLineEnd);
            Vector3 lLineHalf = (lLineEnd - lLineStart) * 0.5f;
            Vector3 lLineHalfAbs = new Vector3(Mathf.Abs(lLineHalf.x), Mathf.Abs(lLineHalf.y), Mathf.Abs(lLineHalf.z));

            // Determine the box dimensions
            Vector3 lBoxMax = new Vector3(rWidth * 0.5f, rHeight * 0.5f, rDepth);
            Vector3 lBoxMin = new Vector3(rWidth * -0.5f, rHeight * -0.5f, 0f);
            Vector3 lBoxCenter = (lBoxMax - lBoxMin) * 0.5f;
            Vector3 lClosest = lLineStart + lLineHalf - ((lBoxMin + lBoxMax) * 0.5f);

            // Seperating axis tests
            if (Mathf.Abs(lClosest.x) > lBoxCenter.x + lLineHalfAbs.x) { return false; }
            if (Mathf.Abs(lClosest.y) > lBoxCenter.y + lLineHalfAbs.y) { return false; }
            if (Mathf.Abs(lClosest.z) > lBoxCenter.z + lLineHalfAbs.z) { return false; }

            if (Mathf.Abs(lLineHalf.y * lClosest.z - lLineHalf.z * lClosest.y) > (lBoxCenter.y * lLineHalfAbs.z) + (lBoxCenter.z * lLineHalfAbs.y) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.z * lClosest.x - lLineHalf.x * lClosest.z) > (lBoxCenter.z * lLineHalfAbs.x) + (lBoxCenter.x * lLineHalfAbs.z) + EPSILON) { return false; };
            if (Mathf.Abs(lLineHalf.x * lClosest.y - lLineHalf.y * lClosest.x) > (lBoxCenter.x * lLineHalfAbs.y) + (lBoxCenter.y * lLineHalfAbs.x) + EPSILON) { return false; };

            // We must collide
            return true;
        }

        #endregion

        #region Contains Point Functions

        /// <summary>
        /// http://www.flipcode.com/archives/Fast_Point-In-Cylinder_Test.shtml
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="rRadius"></param>
        /// <param name="testpt"></param>
        /// <returns></returns>
        public static bool CylinderContainsPoint(Vector3 pt1, Vector3 pt2, float rRadius, Vector3 testpt )
        {
            float radius_sq = rRadius * rRadius;
            float lengthsq = (pt2 - pt1).sqrMagnitude;

	        float dx, dy, dz;   // vector d  from line segment point 1 to point 2
            float pdx, pdy, pdz;    // vector pd from point 1 to test point
            float dot, dsq;

            dx = pt2.x - pt1.x;	// translate so pt1 is origin.  Make vector from
	        dy = pt2.y - pt1.y;     // pt1 to pt2.  Need for this is easily eliminated
	        dz = pt2.z - pt1.z;

	        pdx = testpt.x - pt1.x;		// vector from pt1 to test point.
	        pdy = testpt.y - pt1.y;
	        pdz = testpt.z - pt1.z;

	        // Dot the d and pd vectors to see if point lies behind the 
	        // cylinder cap at pt1.x, pt1.y, pt1.z

	        dot = pdx* dx + pdy* dy + pdz* dz;

	        // If dot is less than zero the point is behind the pt1 cap.
	        // If greater than the cylinder axis line segment length squared
	        // then the point is outside the other end cap at pt2.

	        if( dot< 0.0f || dot> lengthsq )
	        {
                //return( -1.0f );
                return false;
            }
	        else 
	        {
		        // Point lies within the parallel caps, so find
		        // distance squared from point to line, using the fact that sin^2 + cos^2 = 1
		        // the dot = cos() * |d||pd|, and cross*cross = sin^2 * |d|^2 * |pd|^2
		        // Carefull: '*' means mult for scalars and dotproduct for vectors
		        // In short, where dist is pt distance to cyl axis: 
		        // dist = sin( pd to d ) * |pd|
		        // distsq = dsq = (1 - cos^2( pd to d)) * |pd|^2
		        // dsq = ( 1 - (pd * d)^2 / (|pd|^2 * |d|^2) ) * |pd|^2
		        // dsq = pd * pd - dot * dot / lengthsq
		        //  where lengthsq is d*d or |d|^2 that is passed into this function 

		        // distance squared to the cylinder axis:

		        dsq = (pdx* pdx + pdy* pdy + pdz* pdz) - dot* dot/lengthsq;

		        if( dsq > radius_sq )
		        {
                    //return( -1.0f );
                    return false;
		        }
		        else
		        {
                    //return( dsq );		// return distance squared to axis
                    return true;
		        }
	        }
        }

        /// <summary>
        /// Determines if the box collider contains the specified point
        /// </summary>
        /// <param name="rPoint"></param>
        /// <param name="rCollider"></param>
        /// <returns></returns>
        public static bool ContainsPoint(Vector3 rPoint, BoxCollider rCollider)
        {
            // Cace the transform so we don't do extra GetTransform() calls
            Transform lTransform = rCollider.transform;

            // The transform will fit the point into the right scale
            Vector3 lHalfColliderSize = rCollider.size * 0.5f;

            // Move the world space point to local space
            Vector3 lLocalPosition = lTransform.InverseTransformPoint(rPoint);

            // Check if we're outside the box
            if (lLocalPosition.x < -lHalfColliderSize.x ||
                lLocalPosition.x > lHalfColliderSize.x ||
                lLocalPosition.y < -lHalfColliderSize.y ||
                lLocalPosition.y > lHalfColliderSize.y ||
                lLocalPosition.z < -lHalfColliderSize.z ||
                lLocalPosition.z > lHalfColliderSize.z
               )
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Support Functions

        /// <summary>
        /// Determines if we should ignore the collider
        /// </summary>
        /// <param name="rCollider"></param>
        /// <returns></returns>
        private static bool IgnoreCollider(Collider rCollider)
        {
            if (rCollider == null || rCollider.transform == null)
            {
                return true;
            }

            if (rCollider.isTrigger)
            {
                return true;
            }

            if (Ignore != null)
            {
                if (rCollider.transform == Ignore) { return true; }
                if (IsDescendant(Ignore, rCollider.transform)) { return true; }
            }

            if (IgnoreArray != null)
            {
                for (int i = 0; i < IgnoreArray.Length; i++)
                {
                    if (rCollider.transform == IgnoreArray[i]) { return true; }
                    if (IsDescendant(IgnoreArray[i], rCollider.transform)) { return true; }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the "descendant" transform is a child (or grand child)
        /// of the "parent" transform.
        /// </summary>
        /// <param name="rParent"></param>
        /// <param name="rTest"></param>
        /// <returns></returns>
        private static bool IsDescendant(Transform rParent, Transform rDescendant)
        {
            if (rParent == null) { return false; }

            Transform lDescendantParent = rDescendant;
            while (lDescendantParent != null)
            {
                if (lDescendantParent == rParent) { return true; }
                lDescendantParent = lDescendantParent.parent;
            }

            return false;
        }

        private static void GetLineDistanceFromBoxFace(ref Vector3 rBoxExtents, ref Vector3 rBoxPoint, ref Vector3 rBoxDirection, ref Vector3 rExtentToPoint, int rIndex0, int rIndex1, int rIndex2, ref float mLineDistance)
        {
            Vector3 lExtentToPoint = new Vector3();
            lExtentToPoint[rIndex1] = rBoxPoint[rIndex1] + rBoxExtents[rIndex1];
            lExtentToPoint[rIndex2] = rBoxPoint[rIndex2] + rBoxExtents[rIndex2];

            if (rBoxDirection[rIndex0] * lExtentToPoint[rIndex1] >= rBoxDirection[rIndex1] * rExtentToPoint[rIndex0])
            {
                if (rBoxDirection[rIndex0] * lExtentToPoint[rIndex2] >= rBoxDirection[rIndex2] * rExtentToPoint[rIndex0])
                {
                    float lInverse = 1f / rBoxDirection[rIndex0];
                    mLineDistance = -rExtentToPoint[rIndex0] * lInverse;
                }
                else
                {
                    float lLengthSqr = rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex2] * rBoxDirection[rIndex2];
                    float lTemp = lLengthSqr * lExtentToPoint[rIndex1] - rBoxDirection[rIndex1] * (rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2]);

                    if (lTemp <= 2f * lLengthSqr * rBoxExtents[rIndex1])
                    {
                        float lTempPct = lTemp / lLengthSqr;
                        lLengthSqr += rBoxDirection[rIndex1] * rBoxDirection[rIndex1];
                        lTemp = lExtentToPoint[rIndex1] - lTempPct;

                        float lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lTemp + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2];
                        mLineDistance = -lDelta / lLengthSqr;
                    }
                    else
                    {
                        lLengthSqr += rBoxDirection[rIndex1] * rBoxDirection[rIndex1];

                        float lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * rExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2];
                        mLineDistance = -lDelta / lLengthSqr;
                    }
                }
            }
            else
            {
                if (rBoxDirection[rIndex0] * lExtentToPoint[rIndex2] >= rBoxDirection[rIndex2] * rExtentToPoint[rIndex0])
                {
                    float lLengthSqr = rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex1] * rBoxDirection[rIndex1];
                    float lTemp = lLengthSqr * lExtentToPoint[rIndex2] - rBoxDirection[rIndex2] * (rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1]);

                    if (lTemp <= 2f * lLengthSqr * rBoxExtents[rIndex2])
                    {
                        float lTempPct = lTemp / lLengthSqr;
                        lLengthSqr += rBoxDirection[rIndex2] * rBoxDirection[rIndex2];
                        lTemp = lExtentToPoint[rIndex2] - lTempPct;

                        float lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * lTemp;
                        mLineDistance = -lDelta / lLengthSqr;
                    }
                    else
                    {
                        lLengthSqr += rBoxDirection[rIndex2] * rBoxDirection[rIndex2];

                        float lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * rExtentToPoint[rIndex2];
                        mLineDistance = -lDelta / lLengthSqr;
                    }
                }
                else
                {
                    float lDelta = 0f;

                    float lLengthSqr = rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex2] * rBoxDirection[rIndex2];
                    float lTemp = lLengthSqr * lExtentToPoint[rIndex1] - rBoxDirection[rIndex1] * (rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2]);

                    if (lTemp >= 0f)
                    {
                        if (lTemp <= 2f * lLengthSqr * rBoxExtents[rIndex1])
                        {
                            float lTempPct = lTemp / lLengthSqr;
                            lLengthSqr += rBoxDirection[rIndex1] * rBoxDirection[rIndex1];
                            lTemp = lExtentToPoint[rIndex1] - lTempPct;

                            lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lTemp + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2];
                            mLineDistance = -lDelta / lLengthSqr;
                        }
                        else
                        {
                            lLengthSqr += rBoxDirection[rIndex1] * rBoxDirection[rIndex1];

                            lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * rExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2];
                            mLineDistance = -lDelta / lLengthSqr;
                        }

                        return;
                    }

                    lLengthSqr = rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex1] * rBoxDirection[rIndex1];
                    lTemp = lLengthSqr * lExtentToPoint[rIndex2] - rBoxDirection[rIndex2] * (rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1]);

                    if (lTemp >= 0f)
                    {
                        if (lTemp <= 2f * lLengthSqr * rBoxExtents[rIndex2])
                        {
                            float lTempPct = lTemp / lLengthSqr;
                            lLengthSqr += rBoxDirection[rIndex2] * rBoxDirection[rIndex2];
                            lTemp = lExtentToPoint[rIndex2] - lTempPct;

                            lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * lTemp;
                            mLineDistance = -lDelta / lLengthSqr;
                        }
                        else
                        {
                            lLengthSqr += rBoxDirection[rIndex2] * rBoxDirection[rIndex2];

                            lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * rExtentToPoint[rIndex2];
                            mLineDistance = -lDelta / lLengthSqr;
                        }

                        return;
                    }

                    lLengthSqr += rBoxDirection[rIndex2] * rBoxDirection[rIndex2];

                    lDelta = rBoxDirection[rIndex0] * rExtentToPoint[rIndex0] + rBoxDirection[rIndex1] * lExtentToPoint[rIndex1] + rBoxDirection[rIndex2] * lExtentToPoint[rIndex2];
                    mLineDistance = -lDelta / lLengthSqr;
                }
            }
        }

        private static void GetLineDistanceFromBoxExtent(ref Vector3 rBoxExtents, ref Vector3 rBoxPoint, ref Vector3 rBoxDirection, ref float rLineDistance)
        {
            Vector3 lExtentToPoint = new Vector3(rBoxPoint.x - rBoxExtents[0], rBoxPoint.y - rBoxExtents[1], rBoxPoint.z - rBoxExtents[2]);

            if (rBoxDirection.y * lExtentToPoint.x >= rBoxDirection.x * lExtentToPoint.y)
            {
                if (rBoxDirection.z * lExtentToPoint.x >= rBoxDirection.x * lExtentToPoint.z)
                {
                    GetLineDistanceFromBoxFace(ref rBoxExtents, ref rBoxPoint, ref rBoxDirection, ref lExtentToPoint, 0, 1, 2, ref rLineDistance);
                }
                else
                {
                    GetLineDistanceFromBoxFace(ref rBoxExtents, ref rBoxPoint, ref rBoxDirection, ref lExtentToPoint, 2, 0, 1, ref rLineDistance);
                }
            }
            else
            {
                if (rBoxDirection.z * lExtentToPoint.y >= rBoxDirection.y * lExtentToPoint.z)
                {
                    GetLineDistanceFromBoxFace(ref rBoxExtents, ref rBoxPoint, ref rBoxDirection, ref lExtentToPoint, 1, 2, 0, ref rLineDistance);
                }
                else
                {
                    GetLineDistanceFromBoxFace(ref rBoxExtents, ref rBoxPoint, ref rBoxDirection, ref lExtentToPoint, 2, 0, 1, ref rLineDistance);
                }
            }
        }

        private static void GetLineDistanceFromBoxExtent(ref Vector3 rBoxExtents, ref Vector3 rBoxPoint, ref Vector3 rBoxDirection, int rIndex0, int rIndex1, ref float rLineDistance)
        {
            float lExtentToPoint0 = rBoxPoint[rIndex0] - rBoxExtents[rIndex0];
            float lExtentToPoint1 = rBoxPoint[rIndex1] - rBoxExtents[rIndex1];
            float lProduct0 = rBoxDirection[rIndex1] * lExtentToPoint0;
            float lProduct1 = rBoxDirection[rIndex0] * lExtentToPoint1;

            float lDelta = 0f;
            float lInverse = 0f;

            if (lProduct0 >= lProduct1)
            {
                rBoxPoint[rIndex0] = rBoxExtents[rIndex0];

                lExtentToPoint1 = rBoxPoint[rIndex1] + rBoxExtents[rIndex1];
                lDelta = lProduct0 - rBoxDirection[rIndex0] * lExtentToPoint1;
                if (lDelta >= 0f)
                {
                    lInverse = 1f / (rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex1] * rBoxDirection[rIndex1]);
                    rLineDistance = -(rBoxDirection[rIndex0] * lExtentToPoint0 + rBoxDirection[rIndex1] * lExtentToPoint1) * lInverse;
                }
                else
                {
                    lInverse = 1f / rBoxDirection[rIndex0];
                    rLineDistance = -lExtentToPoint0 * lInverse;
                }
            }
            else
            {
                rBoxPoint[rIndex1] = rBoxExtents[rIndex1];

                lExtentToPoint0 = rBoxPoint[rIndex0] + rBoxExtents[rIndex0];
                lDelta = lProduct1 - rBoxDirection[rIndex1] * lExtentToPoint0;
                if (lDelta >= 0f)
                {
                    lInverse = 1f / (rBoxDirection[rIndex0] * rBoxDirection[rIndex0] + rBoxDirection[rIndex1] * rBoxDirection[rIndex1]);
                    rLineDistance = -(rBoxDirection[rIndex0] * lExtentToPoint0 + rBoxDirection[rIndex1] * lExtentToPoint1) * lInverse;
                }
                else
                {
                    lInverse = 1f / rBoxDirection[rIndex1];
                    rLineDistance = -lExtentToPoint1 * lInverse;
                }
            }
        }

        private static void GetLineDistanceFromBoxExtent(ref Vector3 rBoxExtents, ref Vector3 rBoxPoint, ref Vector3 rBoxDirection, int rIndex0, ref float mLineDistance)
        {
            mLineDistance = (rBoxExtents[rIndex0] - rBoxPoint[rIndex0]) / rBoxDirection[rIndex0];
        }

        private static void GetClosestPointFromTerrain(TerrainCollider rCollider, Vector3 rStart, Vector3 rEnd)
        {
            Vector3 lTerrainPosition = rCollider.transform.position;

            TerrainData lTerrainData = rCollider.terrainData;
            int lWidth = lTerrainData.heightmapWidth;
            int lHeight = lTerrainData.heightmapHeight;

            Vector3 lScale = lTerrainData.size;
            lScale = new Vector3(lScale.x / (lWidth - 1), lScale.y, lScale.z / (lHeight - 1));

            float[,] lData = lTerrainData.GetHeights(0, 0, lWidth, lHeight);

            Vector3[] lVertices = new Vector3[lWidth * lHeight];
            for (int y = 0; y < lHeight; y++)
            {
                for (int x = 0; x < lWidth; x++)
                {
                    lVertices[y * lWidth + x] = Vector3.Scale(lScale, new Vector3(-y, lData[x, y], x)) + lTerrainPosition;
                }
            }

            int[] lIndexes = new int[(lWidth - 1) * (lHeight - 1) * 6];

            int lIndex = 0;
            for (int y = 0; y < lHeight - 1; y++)
            {
                for (int x = 0; x < lWidth - 1; x++)
                {
                    // Triangle 1 of cell
                    lIndexes[lIndex++] = (y * lWidth) + x;
                    lIndexes[lIndex++] = ((y + 1) * lWidth) + x;
                    lIndexes[lIndex++] = (y * lWidth) + x + 1;

                    // Triangle 2 of cell
                    lIndexes[lIndex++] = ((y + 1) * lWidth) + x;
                    lIndexes[lIndex++] = ((y + 1) * lWidth) + x + 1;
                    lIndexes[lIndex++] = (y * lWidth) + x + 1;
                }
            }

            //Vector3 lClosestPoint = Vector3.zero;
            float lClosestDistance = float.MaxValue;
            for (lIndex = 0; lIndex < lIndexes.Length; lIndex += 3)
            {
                Vector3 lPoint = ClosestPoint(ref rEnd, ref lVertices[lIndexes[lIndex + 0]], ref lVertices[lIndexes[lIndex + 1]], ref lVertices[lIndexes[lIndex + 2]]);
                float lDistance = Vector3.SqrMagnitude(lPoint - rEnd);

                if (lDistance < lClosestDistance)
                {
                    //lClosestDistance = lDistance;
                    //lClosestPoint = lPoint;
                }
            }

            //rColliderPoint = lClosestPoint;
            //rLinePoint = ClosestPoint(rColliderPoint, rStart, rEnd);
        }

        #endregion
    }
}
