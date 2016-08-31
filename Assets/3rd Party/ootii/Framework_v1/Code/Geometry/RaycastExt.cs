//#define OOTII_DEBUG

using System.Collections.Generic;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Provides functions for specialized raycast solutions
    /// </summary>
    public static class RaycastExt
    {
        /// <summary>
        /// Used when we need to return an empty raycast
        /// </summary>
        public static RaycastHit EmptyHitInfo = new RaycastHit();

        /// <summary>
        /// We use this if we don't want to re-allocate arrays. This is simple, but
        /// won't work with multi-threading and the contents need to be used immediately,
        /// as they are not persistant across alls.
        /// </summary>
        public static RaycastHit[] SharedHitArray = new RaycastHit[10];

        /// <summary>
        /// We use this if we don't want to re-allocate arrays. This is simple, but
        /// won't work with multi-threading and the contents need to be used immediately,
        /// as they are not persistant across alls.
        /// </summary>
        public static Collider[] SharedColliderArray = new Collider[10];

        // ***********************************************************************************
        // Newer non-allocating versions of the call
        // ***********************************************************************************

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = false;
                RaycastHit lHitInfo;

                if (rLayerMask >= 0)
                {
                    lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out lHitInfo, rDistance, rLayerMask);
                }
                else
                {
                    lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out lHitInfo, rDistance);
                }

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (lHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += lHitInfo.distance + 0.05f;
                    rRayStart = lHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (lHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                lHitInfo.distance += lDistanceOffset;

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;

#else

            // In this specific case, we can get out early since there is a way to ignore triggers
            if (rIgnore == null && rIgnoreList == null && rLayerMask >= 0)
            {
                return UnityEngine.Physics.Raycast(rRayStart, rRayDirection, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }

            // Perform the more expensive raycast
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return false;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return false; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return false; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return false; }
                    }
                }

                return true;
            }
            // Go through all the hits and see if any hit
            else
            {
                for (int i = 0; i < lHits; i++)
                {
                    if (SharedHitArray[i].collider.isTrigger) { continue; }

                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { continue; }

                    if (rIgnoreList != null)
                    {
                        bool lIgnoreFound = false;
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lIgnoreFound = true;
                                break;
                            }
                        }

                        if (lIgnoreFound) { continue; }
                    }

                    return true;
                }
            }

            return false;

#endif
        }

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rHitInfo">First RaycastHit value that the ray hits</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, out RaycastHit rHitInfo, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null, bool rDebug = false)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)

            if (rLayerMask < 0)
            {
                return SafeRaycast(rRayStart, rRayDirection, rDistance, rIgnore, out rHitInfo);
            }
            else
            {
                return SafeRaycast(rRayStart, rRayDirection, rDistance, rLayerMask, rIgnore, out rHitInfo);
            }

#else

#if OOTII_DEBUG
            if (rDebug)
            {
                Debug.DrawLine(rRayStart, rRayStart + (rRayDirection * rDistance), Color.blue, 5f);
            }
#endif

            // In this specific case, we can get out early since there is a way to ignore triggers
            if (rIgnore == null && rIgnoreList == null && rLayerMask >= 0)
            {
                return UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }

            // Perform the more expensive raycast
            rHitInfo = EmptyHitInfo;

            // Use the non allocating version
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return false;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return false; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return false; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return false; }
                    }
                }

                rHitInfo = SharedHitArray[0];
                return true;
            }
            // Find the closest hit and test it
            else
            {
                for (int i = 0; i < lHits; i++)
                {
                    if (SharedHitArray[i].collider.isTrigger) { continue; }

                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { continue; }

                    if (rIgnoreList != null)
                    {
                        bool lIgnoreFound = false;
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lIgnoreFound = true;
                                break;
                            }
                        }

                        if (lIgnoreFound) { continue; }
                    }

                    // If we got here, we have a valid it. See if it's closer
                    if (rHitInfo.collider == null || SharedHitArray[i].distance < rHitInfo.distance)
                    {
                        rHitInfo = SharedHitArray[i];
                    }
                }

                if (rHitInfo.collider != null)
                {
                    return true;
                }
            }

            return false;

#endif
        }

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rHitArray">Array of RaycastHit objects that were hit</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static int SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, out RaycastHit[] rHitArray, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            
            rHitArray = SafeRaycastAll(rRayStart, rRayDirection, rDistance, false);
            return (rHitArray != null ? rHitArray.Length : 0);

#else
            rHitArray = null;

            // Use the non allocating version
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.RaycastNonAlloc(rRayStart, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return 0;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return 0; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return 0; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return 0; }
                    }
                }

                rHitArray = SharedHitArray;
                return 1;
            }
            // Go through all the hits and see if any hit
            else
            {
                int lValidHits = 0;
                for (int i = 0; i < lHits; i++)
                {
                    bool lShift = false;
                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (SharedHitArray[i].collider.isTrigger) { lShift = true; }

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { lShift = true; }

                    if (rIgnoreList != null)
                    {
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lShift = true;
                                break;
                            }
                        }
                    }

                    if (lShift)
                    {
                        // Since we are shifting left, out count is reduced
                        lHits--;

                        // Shift the contents, but we care about the old count (hence the + 1)
                        for (int j = i; j < lHits; j++)
                        {
                            SharedHitArray[j] = SharedHitArray[j + 1];
                        }

                        // Move our index so when the for-loop iterates us forward, we stay put
                        i--;
                    }
                    else
                    {
                        lValidHits++;
                    }

                    // With the valid hits gathered, we now need to sort the array
                }

                rHitArray = SharedHitArray;
                return lValidHits;
            }

#endif
        }


        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rRadius">Radius of the sphere</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
            
            if (rLayerMask < 0)
            {
                RaycastHit lHitInfo;
                return SafeSphereCast(rRayStart, rRayDirection, rRadius, rDistance, rIgnore, out lHitInfo);
            }
            else
            {
                RaycastHit lHitInfo;
                return SafeSphereCast(rRayStart, rRayDirection, rRadius, rDistance, rLayerMask, rIgnore, out lHitInfo);
            }

#else

            // In this specific case, we can get out early since there is a way to ignore triggers
            if (rIgnore == null && rIgnoreList == null && rLayerMask >= 0)
            {
                RaycastHit lHitInfo;
                return UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out lHitInfo, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }

            // Perform the more expensive raycast
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return false;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return false; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return false; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return false; }
                    }
                }

                return true;
            }
            // Go through all the hits and see if any hit
            else
            {
                for (int i = 0; i < lHits; i++)
                {
                    if (SharedHitArray[i].collider.isTrigger) { continue; }

                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { continue; }

                    if (rIgnoreList != null)
                    {
                        bool lIgnoreFound = false;
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lIgnoreFound = true;
                                break;
                            }
                        }

                        if (lIgnoreFound) { continue; }
                    }

                    return true;
                }
            }

            return false;

#endif
        }

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rRadius">Radius of the sphere</param>
        /// <param name="rHitInfo">First RaycastHit value that the ray hits</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, out RaycastHit rHitInfo, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)

            if (rLayerMask < 0)
            {
                return SafeSphereCast(rRayStart, rRayDirection, rRadius, rDistance, rIgnore, out rHitInfo);
            }
            else
            {
                return SafeSphereCast(rRayStart, rRayDirection, rRadius, rDistance, rLayerMask, rIgnore, out rHitInfo);
            }

#else

            // In this specific case, we can get out early since there is a way to ignore triggers
            if (rIgnore == null && rIgnoreList == null && rLayerMask >= 0)
            {
                return UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }

            // Perform the more expensive raycast
            rHitInfo = EmptyHitInfo;

            // Use the non allocating version
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return false;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return false; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return false; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return false; }
                    }
                }

                rHitInfo = SharedHitArray[0];
                return true;
            }
            // Find the closest hit and test it
            else
            {
                for (int i = 0; i < lHits; i++)
                {
                    if (SharedHitArray[i].collider.isTrigger) { continue; }

                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { continue; }

                    if (rIgnoreList != null)
                    {
                        bool lIgnoreFound = false;
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lIgnoreFound = true;
                                break;
                            }
                        }

                        if (lIgnoreFound) { continue; }
                    }

                    // If we got here, we have a valid it. See if it's closer
                    if (rHitInfo.collider == null || SharedHitArray[i].distance < rHitInfo.distance)
                    {
                        rHitInfo = SharedHitArray[i];
                    }
                }

                if (rHitInfo.collider != null)
                {
                    return true;
                }
            }

            return false;

#endif
        }

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rRayStart">Starting point of the ray</param>
        /// <param name="rRayDirection">Direction of the ray</param>
        /// <param name="rHitArray">Array of RaycastHit objects that were hit</param>
        /// <param name="rDistance">Max distance f the ray</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static int SafeSphereCastAll(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, out RaycastHit[] rHitArray, float rDistance = 1000f, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)

            if (rLayerMask >= 0 && rIgnoreList == null)
            {
                rHitArray = SafeSphereCastAll(rRayStart, rRayDirection, rRadius, rDistance, rLayerMask, rIgnore);
            }
            else
            {
                rHitArray = SafeSphereCastAll(rRayStart, rRayDirection, rRadius, rDistance, rIgnoreList);
            }

            return (rHitArray != null ? rHitArray.Length : 0);

#else
            rHitArray = null;

            // Use the non allocating version
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.SphereCastNonAlloc(rRayStart, rRadius, rRayDirection, SharedHitArray, rDistance);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return 0;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedHitArray[0].collider.isTrigger) { return 0; }

                Transform lColliderTransform = SharedHitArray[0].collider.transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return 0; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return 0; }
                    }
                }

                rHitArray = SharedHitArray;
                return 1;
            }
            // Go through all the hits and see if any hit
            else
            {
                int lValidHits = 0;
                for (int i = 0; i < lHits; i++)
                {
                    bool lShift = false;
                    Transform lColliderTransform = SharedHitArray[i].collider.transform;

                    if (SharedHitArray[i].collider.isTrigger) { lShift = true; }

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { lShift = true; }

                    if (rIgnoreList != null)
                    {
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lShift = true;
                                break;
                            }
                        }
                    }

                    if (lShift)
                    {
                        // Since we are shifting left, out count is reduced
                        lHits--;

                        // Shift the contents, but we care about the old count (hence the + 1)
                        for (int j = i; j < lHits; j++)
                        {
                            SharedHitArray[j] = SharedHitArray[j + 1];
                        }

                        // Move our index so when the for-loop iterates us forward, we stay put
                        i--;
                    }
                    else
                    {
                        lValidHits++;
                    }
                }

                rHitArray = SharedHitArray;
                return lValidHits;
            }

#endif
        }

        /// <summary>
        /// Use the non-alloc version of raycast to see if the ray hits anything. Here we are
        /// not particular about what we hit. We just test for a hit
        /// </summary>
        /// <param name="rPosition">Position of the sphere</param>
        /// <param name="rRadius">Radius of the sphere</param>
        /// <param name="rCollisionArray">Array of collision objects that were hit</param>
        /// <param name="rLayerMask">Layer mask to determine what we'll hit</param>
        /// <param name="rIgnore">Single transform we'll test if we should ignore</param>
        /// <param name="rIgnoreList">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static int SafeOverlapSphere(Vector3 rPosition, float rRadius, out Collider[] rColliderArray, int rLayerMask = -1, Transform rIgnore = null, List<Transform> rIgnoreList = null)
        {
#if (UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)

            if (rLayerMask < 0)
            {
                if (rIgnore != null)
                {
                    rColliderArray = SafeOverlapSphere(rPosition, rRadius, rIgnore);
                }
                else
                {
                    rColliderArray = SafeOverlapSphere(rPosition, rRadius, rIgnoreList);
                }
            }
            else
            {
                if (rIgnore != null)
                {
                    rColliderArray = SafeOverlapSphere(rPosition, rRadius, rLayerMask, rIgnore);
                }
                else
                {
                    rColliderArray = SafeOverlapSphere(rPosition, rRadius, rLayerMask, rIgnoreList);
                }
            }

            return (rColliderArray != null ? rColliderArray.Length : 0);

#else
            rColliderArray = null;

            // Use the non allocating version
            int lHits = 0;

            if (rLayerMask >= 0)
            {
                lHits = UnityEngine.Physics.OverlapSphereNonAlloc(rPosition, rRadius, SharedColliderArray, rLayerMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                lHits = UnityEngine.Physics.OverlapSphereNonAlloc(rPosition, rRadius, SharedColliderArray);
            }

            // With no hits, this is easy
            if (lHits == 0)
            {
                return 0;
            }
            // One hit is also easy
            else if (lHits == 1)
            {
                if (SharedColliderArray[0].isTrigger) { return 0; }

                Transform lColliderTransform = SharedColliderArray[0].transform;

                if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { return 0; }

                if (rIgnoreList != null)
                {
                    for (int i = 0; i < rIgnoreList.Count; i++)
                    {
                        if (IsDescendant(rIgnoreList[i], lColliderTransform)) { return 0; }
                    }
                }

                rColliderArray = SharedColliderArray;
                return 1;
            }
            // Go through all the hits and see if any hit
            else
            {
                int lValidHits = 0;
                for (int i = 0; i < lHits; i++)
                {
                    bool lShift = false;
                    Transform lColliderTransform = SharedColliderArray[i].transform;

                    if (SharedColliderArray[i].isTrigger) { lShift = true; }

                    if (rIgnore != null && IsDescendant(rIgnore, lColliderTransform)) { lShift = true; }

                    if (rIgnoreList != null)
                    {
                        for (int j = 0; j < rIgnoreList.Count; j++)
                        {
                            if (IsDescendant(rIgnoreList[j], lColliderTransform))
                            {
                                lShift = true;
                                break;
                            }
                        }
                    }

                    if (lShift)
                    {
                        // Move our index so when the for-loop iterates us forward, we stay put
                        lHits--;

                        // Shift the contents left, but we care about the old count (hence the + 1)
                        for (int j = i; j < lHits; j++)
                        {
                            SharedColliderArray[j] = SharedColliderArray[j + 1];
                        }

                        // Move our index so when the for-loop iterates us forward, we stay put
                        i--;
                    }
                    else
                    {
                        lValidHits++;
                    }
                }

                rColliderArray = SharedColliderArray;
                return lValidHits;
            }

#endif
        }

        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <param name="rTransform"></param>
        /// <param name="rMaxDistance"></param>
        /// <param name="rMaxHeight"></param>
        /// <param name="rCollisionLayers"></param>
        /// <param name="rEdgeHitInfo"></param>
        /// <returns></returns>
        public static bool GetForwardEdge(Transform rTransform, float rMaxDistance, float rMaxHeight, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

            // Shoot above the expected height to make sure that it's open. We don't want to hit anything
            Vector3 lRayStart = rTransform.position + (rTransform.up * (rMaxHeight + 0.001f));
            Vector3 lRayDirection = rTransform.forward;
            float lRayDistance = rMaxDistance * 1.5f;

            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

            // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
            lRayStart = lRayStart + (rTransform.forward * rMaxDistance);
            lRayDirection = -rTransform.up;
            lRayDistance = rMaxHeight;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

            // This is the height of our edge
            float lEdgeHeight = (rMaxHeight + 0.001f) - rEdgeHitInfo.distance;

            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
            lRayStart = rTransform.position + (rTransform.up * (lEdgeHeight - 0.001f));
            lRayDirection = rTransform.forward;
            lRayDistance = rMaxDistance;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

#if OOTII_DEBUG
            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
#endif

            // If we get here, there was a valid hit
            return true;
        }


        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <returns></returns>
        public static bool GetForwardEdge(Transform rTransform, Vector3 rPosition, float rMinHeight, float rMaxHeight, float rMaxDepth, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

            // Shoot above the expected height to make sure that it's open. We don't want to hit anything
            Vector3 lRayStart = rPosition + (rTransform.up * (rMaxHeight + 0.001f));
            Vector3 lRayDirection = rTransform.forward;
            float lRayDistance = rMaxDepth;

            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
            {
                return false;
            }

            // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
            lRayStart = lRayStart + (rTransform.forward * rMaxDepth);
            lRayDirection = -rTransform.up;
            lRayDistance = rMaxHeight - rMinHeight;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
            {
                return false;
            }

            // This is the height of our edge
            float lEdgeHeight = (rMaxHeight + 0.001f) - rEdgeHitInfo.distance;

            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
            lRayStart = rPosition + (rTransform.up * (lEdgeHeight - 0.001f));
            lRayDirection = rTransform.forward;
            lRayDistance = rMaxDepth;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
            {
                return false;
            }

#if OOTII_DEBUG
            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
#endif

            // If we get here, there was a valid hit
            return true;
        }

        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <param name="rTransform"></param>
        /// <param name="rMaxDistance"></param>
        /// <param name="rMaxHeight"></param>
        /// <param name="rCollisionLayers"></param>
        /// <param name="rEdgeHitInfo"></param>
        /// <returns></returns>
        public static bool GetForwardEdge(Transform rTransform, float rMaxDistance, float rMaxHeight, float rMinHeight, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

            // Shoot above the expected min height to make sure that it's blocked. We want to hit something
            Vector3 lRayStart = rTransform.position + (rTransform.up * (rMinHeight + 0.001f));
            Vector3 lRayDirection = rTransform.forward;
            float lRayDistance = rMaxDistance;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

            float lHitDepth = rEdgeHitInfo.distance;

            // Shoot above the expected height to make sure that it's open. We don't want to hit anything
            lRayStart = rTransform.position + (rTransform.up * (rMaxHeight + 0.001f));
            lRayDirection = rTransform.forward;
            lRayDistance = rMaxDistance;

            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                // If there is no ledge, we need to stop
                if (rEdgeHitInfo.distance < lHitDepth + 0.1f)
                {
                    return false;
                }
            }

            // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
            lRayStart = lRayStart + (rTransform.forward * (lHitDepth + 0.001f));
            lRayDirection = -rTransform.up;
            lRayDistance = rMaxHeight;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

            // This is the height of our edge
            float lEdgeHeight = (rMaxHeight + 0.001f) - rEdgeHitInfo.distance;

            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
            lRayStart = rTransform.position + (rTransform.up * (lEdgeHeight - 0.001f));
            lRayDirection = rTransform.forward;
            lRayDistance = rMaxDistance;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
            {
                return false;
            }

#if OOTII_DEBUG
            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
#endif

            // If we get here, there was a valid hit
            return true;
        }

        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <param name="rTransform"></param>
        /// <param name="rMaxDistance"></param>
        /// <param name="rMaxHeight"></param>
        /// <param name="rCollisionLayers"></param>
        /// <param name="rEdgeHitInfo"></param>
        /// <returns></returns>
        public static bool GetForwardEdge2(Transform rTransform, float rMinHeight, float rMaxHeight, float rEdgeDepth, float rMaxDepth, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            return GetForwardEdge2(rTransform, rTransform.position, rTransform.forward, rTransform.up, rMinHeight, rMaxHeight, rEdgeDepth, rMaxDepth, rCollisionLayers, out rEdgeHitInfo);

//            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

//            // Shoot above the expected min height to make sure that it's blocked. We want to hit something
//            Vector3 lRayStart = rTransform.position + (rTransform.up * (rMinHeight + 0.001f));
//            Vector3 lRayDirection = rTransform.forward;
//            float lRayDistance = rMaxDepth;

//            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
//            {
//                return false;
//            }

//            float lHitDepth = rEdgeHitInfo.distance;

//            // Shoot above the expected height to make sure that it's open. We don't want to hit anything
//            lRayStart = rTransform.position + (rTransform.up * (rMaxHeight + 0.001f));
//            lRayDirection = rTransform.forward;
//            lRayDistance = rMaxDepth;

//            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
//            {
//                // If there is no ledge, we need to stop
//                if (rEdgeHitInfo.distance < lHitDepth + 0.1f)
//                {
//                    return false;
//                }
//            }

//            // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
//            lRayStart = lRayStart + (rTransform.forward * (lHitDepth + 0.001f));
//            lRayDirection = -rTransform.up;
//            lRayDistance = rMaxHeight;

//            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
//            {
//                return false;
//            }

//            // This is the height of our edge
//            float lEdgeHeight = (rMaxHeight + 0.001f) - rEdgeHitInfo.distance;

//            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
//            lRayStart = rTransform.position + (rTransform.up * (lEdgeHeight - 0.001f));
//            lRayDirection = rTransform.forward;
//            lRayDistance = rMaxDepth;

//            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform))
//            {
//                return false;
//            }

//#if OOTII_DEBUG
//            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
//#endif

//            // If we get here, there was a valid hit
//            return true;
        }

        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <returns></returns>
        public static bool GetForwardEdge2(Transform rTransform, Vector3 rPosition, float rMinHeight, float rMaxHeight, float rEdgeDepth, float rMaxDepth, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            return GetForwardEdge2(rTransform, rPosition, rTransform.forward, rTransform.up, rMinHeight, rMaxHeight, rEdgeDepth, rMaxDepth, rCollisionLayers, out rEdgeHitInfo);
//            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

//            // Shoot above the expected height to make sure that it's open. We don't want to hit anything
//            Vector3 lRayStart = rPosition + (rTransform.up * (rMaxHeight + 0.001f));
//            Vector3 lRayDirection = rTransform.forward;
//            float lRayDistance = rMaxDepth;

//            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
//            {
//                return false;
//            }

//            // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
//            lRayStart = lRayStart + (rTransform.forward * rMaxDepth);
//            lRayDirection = -rTransform.up;
//            lRayDistance = rMaxHeight - rMinHeight;

//            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
//            {
//                return false;
//            }

//            // This is the height of our edge
//            float lEdgeHeight = (rMaxHeight + 0.001f) - rEdgeHitInfo.distance;

//            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
//            lRayStart = rPosition + (rTransform.up * (lEdgeHeight - 0.001f));
//            lRayDirection = rTransform.forward;
//            lRayDistance = rMaxDepth;

//            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
//            {
//                return false;
//            }

//#if OOTII_DEBUG
//            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
//#endif

//            // If we get here, there was a valid hit
//            return true;
        }

        /// <summary>
        /// This function will help to find a forward edge. 
        /// </summary>
        /// <returns></returns>
        public static bool GetForwardEdge2(Transform rTransform, Vector3 rPosition, Vector3 rForward, Vector3 rUp, float rMinHeight, float rMaxHeight, float rEdgeDepth, float rMaxDepth, int rCollisionLayers, out RaycastHit rEdgeHitInfo)
        {
            rEdgeHitInfo = RaycastExt.EmptyHitInfo;

            float lEdgeHeight = 0f;
            float lEdgeDepth = float.MaxValue;
            float lAboveDepth = float.MaxValue;

            Vector3 lRayStart = rPosition + (rUp * (rMaxHeight - 0.001f));
            Vector3 lRayDirection = rForward;
            float lRayDistance = rMaxDepth;

            // Shoot just below the expected max height to get the depth above the edge (if we hit anything)
            if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
            {
                lEdgeDepth = rEdgeHitInfo.distance;
                lAboveDepth = lEdgeDepth;
            }
            // Shoot one more ray forward to see if we can get the depth and avoid having to step
            else
            {
                lRayStart = rPosition + (rUp * (rMinHeight + (rMaxHeight - rMinHeight) * 0.5f));
                if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
                {
                    lEdgeDepth = rEdgeHitInfo.distance;
                }
            }

            // Since have a depth hit, find the height
            if (lEdgeDepth < float.MaxValue)
            {
                // Shoot down to see if we hit a ledge. We want to hit the top of the ledge.
                lRayStart = rPosition + (rForward * (lEdgeDepth + 0.001f)) + (rUp * (rMaxHeight + 0.001f));
                lRayDirection = -rUp;
                lRayDistance = rMaxHeight - rMinHeight;

                if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
                {
                    return false;
                }

                // This is the height of our edge
                lEdgeHeight = rMaxHeight - (rEdgeHitInfo.distance + 0.001f);
            }
            // Since we didn't have a depth hit, we'll step forward until we get a hit going down
            else
            {
                lRayDirection = -rUp;
                lRayDistance = rMaxHeight - rMinHeight;

                for (float lDepth = rEdgeDepth; lDepth <= rMaxDepth; lDepth += (rEdgeDepth * 0.5f))
                {
                    lRayStart = rPosition + (rForward * lDepth) + (rUp * (rMaxHeight + 0.001f));
                    if (SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
                    {
                        lEdgeHeight = rMaxHeight - (rEdgeHitInfo.distance + 0.001f);
                        break;
                    }
                }
            }

            // If we don't have an edge height, we don't have an edge
            if (lEdgeHeight == 0f) { return false; }

            // Shoot a ray forward to find the actual edge. We want to hit the front of the ledge.
            lRayStart = rPosition + (rUp * lEdgeHeight);
            lRayDirection = rForward;
            lRayDistance = rMaxDepth;

            if (!SafeRaycast(lRayStart, lRayDirection, out rEdgeHitInfo, lRayDistance, rCollisionLayers, rTransform, null, true))
            {
                return false;
            }

            // If the edge isn't deep enough, stop.
            if (lAboveDepth - rEdgeHitInfo.distance < rEdgeDepth)
            {
                return false;
            }

#if OOTII_DEBUG
            Utilities.Debug.DebugDraw.DrawSphereMesh(rEdgeHitInfo.point, 0.02f, Color.red, 1f);
#endif

            // If we get here, there was a valid hit
            return true;
        }

        /// <summary>
        /// Insertion sort for an array of RaycastHit items. Insertion sort works
        /// great for small lists.
        /// </summary>
        /// <param name="rHitArray">Array to sort</param>
        /// <param name="rCount">Item count to sort</param>
        public static void Sort(RaycastHit[] rHitArray, int rCount)
        {        
            if (rHitArray == null) { return; }
            if (rHitArray.Length <= 1) { return; }
            if (rCount > rHitArray.Length) { rCount = rHitArray.Length; }

            int lSavedIndex = 0;
            RaycastHit lTemp;

            for (int lIndex = 1; lIndex < rCount; lIndex++)
            {
                lSavedIndex = lIndex;
                lTemp = rHitArray[lIndex];

                while ((lSavedIndex > 0) && (rHitArray[lSavedIndex - 1].distance > lTemp.distance))
                {
                    rHitArray[lSavedIndex] = rHitArray[lSavedIndex - 1];
                    lSavedIndex = lSavedIndex - 1;
                }

                rHitArray[lSavedIndex] = lTemp;
            }
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
        
        // ***********************************************************************************
        // Older allocating versions of the call
        // ***********************************************************************************

        /// <summary>
        /// NOT USED SEE NON-ALLOCATING VERSION
        /// <returns></returns>
        //public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance)
        //{
        //    int lHitCount = 0;
        //    float lDistanceOffset = 0f;

        //    // Since some objects (like triggers) are invalid for collisions, we want
        //    // to go through them. That means we may continue a ray cast even if a hit occured
        //    while (lHitCount < 5 && rDistance > 0f)
        //    {
        //        // Assume the next hit to be valid
        //        bool lIsValidHit = true;

        //        // Test from the current start
        //        RaycastHit lHitInfo;
        //        bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out lHitInfo, rDistance);

        //        // Easy, just return no hit
        //        if (!lHit) { return false; }

        //        // If we hit a trigger, we'll continue testing just a tad bit beyond.
        //        if (lHitInfo.collider.isTrigger) { lIsValidHit = false; }

        //        // If we have an invalid hit, we'll continue testing by using the hit point
        //        // (plus a little extra) as the new start
        //        if (!lIsValidHit)
        //        {
        //            lDistanceOffset += lHitInfo.distance + 0.05f;
        //            rRayStart = lHitInfo.point + (rRayDirection * 0.05f);

        //            rDistance -= (lHitInfo.distance + 0.05f);

        //            lHitCount++;
        //            continue;
        //        }

        //        // If we got here, we must have a valid hit. Update
        //        // the distance info incase we had to cycle through invalid hits
        //        lHitInfo.distance += lDistanceOffset;

        //        return true;
        //    }

        //    // If we got here, we exceeded our attempts and we should drop out
        //    return false;
        //}

        /// <summary>
        /// NOT USED SEE NON-ALLOCATING VERSION
        /// 
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeRaycastX(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to assign the out parameter
            rHitInfo = new RaycastHit();

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, int rLayerMask, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to assign the out parameter
            rHitInfo = new RaycastHit();

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, Transform rIgnore)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;
            Vector3 lRayStart = rRayStart;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                RaycastHit lHitInfo;
                bool lHit = UnityEngine.Physics.Raycast(lRayStart, rRayDirection, out lHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (lHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore == lHitInfo.collider.transform) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += lHitInfo.distance + 0.05f;
                    lRayStart = lHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (lHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                lHitInfo.distance += lDistanceOffset;
                lHitInfo.distance = Vector3.Distance(rRayStart, lHitInfo.point);

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, Transform rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance);

#if OOTII_DEBUG
                Color lColor = (lHit ? Color.red : Color.green);
                Debug.DrawLine(rRayStart, rRayStart + (rRayDirection * rDistance), lColor, 5f);
#endif


                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                // Valid hit. So get out.
                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, int rLayerMask, Transform rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                // Valid hit. So get out.
                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, List<Transform> rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;
            Vector3 lRayStart = rRayStart;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(lRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    lRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;


                rHitInfo.distance = Vector3.Distance(rRayStart, rHitInfo.point);

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycastRef(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, List<Transform> rIgnore, ref RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;
            Vector3 lRayStart = rRayStart;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(lRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    lRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                rHitInfo.distance = Vector3.Distance(rRayStart, rHitInfo.point);

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, int rLayerMask, List<Transform> rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycastRef(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, int rLayerMask, List<Transform> rIgnore, ref RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger)
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt<RaycastHit>(lHitArray, i); }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, bool rRemoveTriggers)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (rRemoveTriggers && lHitArray[0].collider.isTrigger)
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (rRemoveTriggers && lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, Transform rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore == lHitArray[0].collider.transform)
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt<RaycastHit>(lHitArray, i); }
                    if (rIgnore != null && rIgnore == lHitArray[i].collider.transform) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }
                }
            }

            return lHitArray;
        }

        ///// <summary>
        ///// When casting a ray from the motion controller, we don't want it to collide with
        ///// ourselves. The problem is that we may want to collide with another avatar. So, we
        ///// can't put every avatar on their own layer. This ray cast will take a little longer,
        ///// but will ignore this avatar.
        ///// 
        ///// Note: This function isn't virutal to eek out ever ounce of performance we can.
        ///// </summary>
        ///// <param name="rRayStart"></param>
        ///// <param name="rRayDirection"></param>
        ///// <param name="rHitInfo"></param>
        ///// <param name="rDistance"></param>
        ///// <returns></returns>
        //public static RaycastHit[] SafeRaycastAll(Vector3 rRayStart, Vector3 rRayDirection, float rDistance, List<Transform> rIgnore)
        //{
        //    RaycastHit[] lHitArray = UnityEngine.Physics.RaycastAll(rRayStart, rRayDirection, rDistance);

        //    // With no hits, this is easy
        //    if (lHitArray.Length == 0)
        //    {
        //    }
        //    // With one hit, this is easy too
        //    else if (lHitArray.Length == 1)
        //    {
        //        if (lHitArray[0].collider.isTrigger ||
        //            (rIgnore != null && rIgnore.Contains(lHitArray[0].collider.transform))
        //           )
        //        {
        //            lHitArray = new RaycastHit[0];
        //        }
        //    }
        //    // Find the closest hit
        //    else
        //    {
        //        // Order the array by distance and get rid of items that don't pass
        //        lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
        //        for (int i = lHitArray.Length - 1; i >= 0; i--)
        //        {
        //            if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt<RaycastHit>(lHitArray, i); }
        //            if (rIgnore != null && rIgnore.Contains(lHitArray[i].collider.transform)) { ArrayExt.RemoveAt(ref lHitArray, i); }
        //        }
        //    }

        //    return lHitArray;
        //}

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                RaycastHit lRayHitInfo;
                if (UnityEngine.Physics.Raycast(rRayStart, rHitInfo.point - rRayStart, out lRayHitInfo, rDistance))
                {
                    rHitInfo = lRayHitInfo;
                }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, Transform rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                RaycastHit lRayHitInfo;
                if (UnityEngine.Physics.Raycast(rRayStart, rHitInfo.point - rRayStart, out lRayHitInfo, rDistance))
                {
                    rHitInfo = lRayHitInfo;
                }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, int rLayerMask, Transform rIgnore, out RaycastHit rHitInfo)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                RaycastHit lRayHitInfo;
                if (UnityEngine.Physics.Raycast(rRayStart, rHitInfo.point - rRayStart, out lRayHitInfo, rDistance))
                {
                    rHitInfo = lRayHitInfo;
                }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, Transform rIgnore, out RaycastHit rHitInfo, bool rRecast)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                if (rRecast)
                {
                    RaycastHit lRayHitInfo;
                    if (UnityEngine.Physics.Raycast(rRayStart, rHitInfo.point - rRayStart, out lRayHitInfo, rDistance))
                    {
                        rHitInfo = lRayHitInfo;
                    }
                }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static bool SafeSphereCast(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, int rLayerMask, Transform rIgnore, out RaycastHit rHitInfo, bool rRecast)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.SphereCast(rRayStart, rRadius, rRayDirection, out rHitInfo, rDistance, rLayerMask);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // Turns out we can't actually trust the sphere cast as it sometimes returns incorrect point and normal values.
                if (rRecast)
                {
                    RaycastHit lRayHitInfo;
                    if (UnityEngine.Physics.Raycast(rRayStart, rHitInfo.point - rRayStart, out lRayHitInfo, rDistance, rLayerMask))
                    {
                        rHitInfo = lRayHitInfo;
                    }
                }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }

                // If we hit a transform to ignore
                if (lIsValidHit && rIgnore != null)
                {
                    Transform lCurrentTransform = rHitInfo.collider.transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lIsValidHit = false;
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    rRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                return true;
            }

            // We have to initialize the out param
            rHitInfo = EmptyHitInfo;

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }
        
        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeSphereCastAll(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, Transform rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.SphereCastAll(rRayStart, rRadius, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore == lHitArray[0].collider.transform)
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }

                    if (rIgnore != null)
                    {
                        bool lIsValidHit = true;
                        Transform lCurrentTransform = lHitArray[i].collider.transform;
                        while (lCurrentTransform != null)
                        {
                            if (lCurrentTransform == rIgnore)
                            {
                                lIsValidHit = false;
                                break;
                            }

                            lCurrentTransform = lCurrentTransform.parent;
                        }

                        if (!lIsValidHit) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }
                    }
                }
            }

            return lHitArray;
        }
        
        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeSphereCastAll(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, int rLayerMask, Transform rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.SphereCastAll(rRayStart, rRadius, rRayDirection, rDistance, rLayerMask);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore == lHitArray[0].collider.transform)
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }

                    if (rIgnore != null)
                    {
                        bool lIsValidHit = true;
                        Transform lCurrentTransform = lHitArray[i].collider.transform;
                        while (lCurrentTransform != null)
                        {
                            if (lCurrentTransform == rIgnore)
                            {
                                lIsValidHit = false;
                                break;
                            }

                            lCurrentTransform = lCurrentTransform.parent;
                        }

                        if (!lIsValidHit) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }
                    }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static RaycastHit[] SafeSphereCastAll(Vector3 rRayStart, Vector3 rRayDirection, float rRadius, float rDistance, List<Transform> rIgnore)
        {
            RaycastHit[] lHitArray = UnityEngine.Physics.SphereCastAll(rRayStart, rRadius, rRayDirection, rDistance);

            // With no hits, this is easy
            if (lHitArray.Length == 0)
            {
            }
            // With one hit, this is easy too
            else if (lHitArray.Length == 1)
            {
                if (lHitArray[0].collider.isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lHitArray[0].collider.transform))
                   )
                {
                    lHitArray = new RaycastHit[0];
                }
            }
            // Find the closest hit
            else
            {
                // Order the array by distance and get rid of items that don't pass
                lHitArray.Sort(delegate (RaycastHit rLeft, RaycastHit rRight) { return (rLeft.distance < rRight.distance ? -1 : (rLeft.distance > rRight.distance ? 1 : 0)); });
                for (int i = lHitArray.Length - 1; i >= 0; i--)
                {
                    if (lHitArray[i].collider.isTrigger) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }

                    if (rIgnore != null)
                    {
                        bool lIsValidHit = true;
                        Transform lCurrentTransform = lHitArray[i].collider.transform;
                        while (lCurrentTransform != null)
                        {
                            if (rIgnore.Contains(lCurrentTransform))
                            {
                                lIsValidHit = false;
                                break;
                            }

                            lCurrentTransform = lCurrentTransform.parent;
                        }

                        if (!lIsValidHit) { lHitArray = ArrayExt.RemoveAt(lHitArray, i); }
                    }
                }
            }

            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static Collider[] SafeOverlapSphere(Vector3 rPosition, float rRadius, Transform rIgnore)
        {
            // This causes 28 B of GC.
            Collider[] lHitArray = UnityEngine.Physics.OverlapSphere(rPosition, rRadius);

            // Get rid of elements we don't need
            for (int i = lHitArray.Length - 1; i >= 0; i--)
            {
                // If we hit a trigger to ignore
                if (lHitArray[i].isTrigger)
                {
                    lHitArray = ArrayExt.RemoveAt<Collider>(lHitArray, i);
                    continue;
                }

                // If we hit a transform to ignore
                if (rIgnore != null)
                {
                    Transform lCurrentTransform = lHitArray[i].transform;
                    while (lCurrentTransform != null)
                    {
                        if (lCurrentTransform == rIgnore)
                        {
                            lHitArray = ArrayExt.RemoveAt<Collider>(lHitArray, i);
                            break;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                    }
                }
            }

            // Return the rest
            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static Collider[] SafeOverlapSphere(Vector3 rRayStart, float rRadius, int rLayerMask, Transform rIgnore)
        {
            // This causes 28 B of GC.
            Collider[] lHitArray = UnityEngine.Physics.OverlapSphere(rRayStart, rRadius, rLayerMask);

            // Get rid of elements we don't need
            for (int i = lHitArray.Length - 1; i >= 0; i--)
            {
                if (lHitArray[i].isTrigger ||
                    (rIgnore != null && rIgnore == lHitArray[i].transform))
                {
                    lHitArray = ArrayExt.RemoveAt<Collider>(lHitArray, i);
                }
            }

            // Return the rest
            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static Collider[] SafeOverlapSphere(Vector3 rRayStart, float rRadius, List<Transform> rIgnore)
        {
            // This causes 28 B of GC.
            Collider[] lHitArray = UnityEngine.Physics.OverlapSphere(rRayStart, rRadius);

            // Get rid of elements we don't need
            for (int i = lHitArray.Length - 1; i >= 0; i--)
            {
                if (lHitArray[i].isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lHitArray[i].transform)))
                {
                    lHitArray = ArrayExt.RemoveAt<Collider>(lHitArray, i);
                }
            }

            // Return the rest
            return lHitArray;
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <returns></returns>
        public static Collider[] SafeOverlapSphere(Vector3 rRayStart, float rRadius, int rLayerMask, List<Transform> rIgnore)
        {
            // This causes 28 B of GC.
            Collider[] lHitArray = UnityEngine.Physics.OverlapSphere(rRayStart, rRadius, rLayerMask);

            // Get rid of elements we don't need
            for (int i = lHitArray.Length - 1; i >= 0; i--)
            {
                if (lHitArray[i].isTrigger ||
                    (rIgnore != null && rIgnore.Contains(lHitArray[i].transform)))
                {
                    lHitArray = ArrayExt.RemoveAt<Collider>(lHitArray, i);
                }
            }

            // Return the rest
            return lHitArray;
        }


        // Older function that shouldn't be used

        //public static bool SafeRaycast(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance, Transform rIgnore)
        //{
        //    return SafeRaycast(rRayStart, rRayDirection, rDistance, rIgnore, out rHitInfo);
        //}
        public static bool SafeRaycastX(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance, int rLayerMask, Transform rIgnore)
        {
            return SafeRaycast(rRayStart, rRayDirection, out rHitInfo, rDistance, rLayerMask, rIgnore);
        }

        /// <summary>
        /// When casting a ray from the motion controller, we don't want it to collide with
        /// ourselves. The problem is that we may want to collide with another avatar. So, we
        /// can't put every avatar on their own layer. This ray cast will take a little longer,
        /// but will ignore this avatar.
        /// 
        /// Note: This function isn't virutal to eek out ever ounce of performance we can.
        /// </summary>
        /// <param name="rRayStart"></param>
        /// <param name="rRayDirection"></param>
        /// <param name="rHitInfo"></param>
        /// <param name="rDistance"></param>
        /// <param name="rIgnore">List of transforms we should ignore collisions with</param>
        /// <returns></returns>
        public static bool SafeRaycastX(Vector3 rRayStart, Vector3 rRayDirection, ref RaycastHit rHitInfo, float rDistance, List<Transform> rIgnore)
        {
            int lHitCount = 0;
            float lDistanceOffset = 0f;
            Vector3 lRayStart = rRayStart;

            // Since some objects (like triggers) are invalid for collisions, we want
            // to go through them. That means we may continue a ray cast even if a hit occured
            while (lHitCount < 5 && rDistance > 0f)
            {
                // Assume the next hit to be valid
                bool lIsValidHit = true;

                // Test from the current start
                bool lHit = UnityEngine.Physics.Raycast(lRayStart, rRayDirection, out rHitInfo, rDistance);

                // Easy, just return no hit
                if (!lHit) { return false; }

                // If we hit a trigger, we'll continue testing just a tad bit beyond.
                if (rHitInfo.collider.isTrigger) { lIsValidHit = false; }
                if (rIgnore != null && rIgnore.Contains(rHitInfo.collider.transform)) { lIsValidHit = false; }

                // If we have an invalid hit, we'll continue testing by using the hit point
                // (plus a little extra) as the new start
                if (!lIsValidHit)
                {
                    lDistanceOffset += rHitInfo.distance + 0.05f;
                    lRayStart = rHitInfo.point + (rRayDirection * 0.05f);

                    rDistance -= (rHitInfo.distance + 0.05f);

                    lHitCount++;
                    continue;
                }

                // If we got here, we must have a valid hit. Update
                // the distance info incase we had to cycle through invalid hits
                rHitInfo.distance += lDistanceOffset;

                rHitInfo.distance = Vector3.Distance(rRayStart, rHitInfo.point);

                return true;
            }

            // If we got here, we exceeded our attempts and we should drop out
            return false;
        }
    }
}