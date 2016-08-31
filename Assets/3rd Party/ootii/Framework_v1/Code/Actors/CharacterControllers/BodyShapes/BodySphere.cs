using System;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Data.Serializers;
using com.ootii.Geometry;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ootii.Actors
{
    /// <summary>
    /// Contains information that defines a sphere we'll use
    /// to help manage collisions with an actor
    /// </summary>
    [Serializable]
    public class BodySphere : BodyShape
    {
        /// <summary>
        /// Relative position from the transform. If no transform
        /// is defined, it's the position from the origin.
        /// </summary>
        public override Vector3 Offset
        {
            get { return _Offset; }

            set
            {
                if (_Offset == value) { return; }

                _Offset = value;

                if (mColliders != null && Collider != null)
                {
                    Transform lTransform = (_Transform != null ? _Transform : _Parent);
                    float lScale = ((lTransform.lossyScale.x + lTransform.lossyScale.y + lTransform.lossyScale.z) / 3f);

                    Collider.center = _Offset / lScale;
                }
            }
        }

        /// <summary>
        /// Radius of the sphere
        /// </summary>
        public override float Radius
        {
            get { return _Radius; }

            set
            {
                if (_Radius == value) { return; }

                _Radius = value;

                if (mColliders != null && Collider != null)
                {
                    Transform lTransform = (_Transform != null ? _Transform : _Parent);
                    float lScale = ((lTransform.lossyScale.x + lTransform.lossyScale.y + lTransform.lossyScale.z) / 3f);

                    Collider.radius = _Radius / lScale;
                }
            }
        }

        /// <summary>
        /// Returns the collider associated with the body shape
        /// </summary>
        [SerializationIgnore]
        public new SphereCollider Collider
        {
            get
            {
                if (mColliders == null || mColliders.Length == 0) { return null; }
                return mColliders[0] as SphereCollider;
            }

            set
            {
                if (mColliders == null || mColliders.Length == 0) { mColliders = new Collider[1]; }
                mColliders[0] = value;
            }
        }

        /// <summary>
        /// Checks if the shape currently overlaps any colliders
        /// </summary>
        /// <param name="rPositionDelta">Movement to add to the current position</param>
        /// <param name="rLayerMask">Layer mask for determing what we'll collide with</param>
        /// <returns>Boolean that says if a hit occured or not</returns>
        public override List<BodyShapeHit> CollisionOverlap(Vector3 rPositionDelta, Quaternion rRotationDelta, int rLayerMask)
        {
            List<BodyShapeHit> lHits = new List<BodyShapeHit>();

            Vector3 lBodyShapePos1 = rPositionDelta + (_Transform == null ? _Parent.position + ((_Parent.rotation * rRotationDelta) * _Offset) : _Transform.position + ((_Transform.rotation * rRotationDelta) * _Offset));

            Collider[] lColliders = null;
            int lColliderHits = RaycastExt.SafeOverlapSphere(lBodyShapePos1, _Radius, out lColliders, -1, _Parent);

            for (int i = 0; i < lColliderHits; i++)
            {
                if (lColliders[i].transform == _Transform) { continue; }
                if (_CharacterController != null && _CharacterController.IsIgnoringCollision(lColliders[i])) { continue; }

                // Once we get here, we have a valid collider
                Vector3 lLinePoint = lBodyShapePos1;
                Vector3 lColliderPoint = GeometryExt.ClosestPoint(lBodyShapePos1, _Radius, lColliders[i]);

                float lDistance = Vector3.Distance(lLinePoint, lColliderPoint);
                if (lDistance < _Radius + 0.001f)
                {
                    BodyShapeHit lHit = BodyShapeHit.Allocate();
                    lHit.StartPosition = lBodyShapePos1;
                    lHit.HitCollider = lColliders[i];
                    lHit.HitOrigin = lLinePoint;
                    lHit.HitPoint = lColliderPoint;
                    lHit.HitDistance = lDistance - _Radius - 0.001f;

                    lHits.Add(lHit);
                }
            }

            return lHits;
        }

        /// <summary>
        /// Casts out a shape to see if a collision will occur. The resulting array 
        /// should NOT be persisted. It is re-used by this function over and over to 
        /// reduce memory allocation.
        /// </summary>
        /// <param name="rPositionDelta">Movement to add to the current position</param>
        /// <param name="rDirection">Direction of the cast</param>
        /// <param name="rDistance">Distance of the case</param>
        /// <param name="rLayerMask">Layer mask for determing what we'll collide with</param>
        /// <returns>Returns an array of BodyShapeHit values representing all the hits that take place</returns>
        public override BodyShapeHit[] CollisionCastAll(Vector3 rPositionDelta, Vector3 rDirection, float rDistance, int rLayerMask)
        {
            Vector3 lBodyShapePos1 = rPositionDelta + (_Transform == null ? _Parent.position + (_Parent.rotation * _Offset) : _Transform.position + (_Transform.rotation * _Offset));

            // Clear any existing body shape hits. They are released by the calloer
            for (int i = 0; i < mBodyShapeHitArray.Length; i++) { mBodyShapeHitArray[i] = null; }

            // Use the non-allocating version if we can
            int lHitCount = 0;

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            mRaycastHitArray = UnityEngine.Physics.SphereCastAll(lBodyShapePos1, _Radius, rDirection, rDistance + EPSILON, rLayerMask);
            if (mRaycastHitArray != null) 
            { 
                lHitCount = mRaycastHitArray.Length; 
                mBodyShapeHitArray = new BodyShapeHit[lHitCount];
            }
#else
            lHitCount = UnityEngine.Physics.SphereCastNonAlloc(lBodyShapePos1, _Radius, rDirection, mRaycastHitArray, rDistance + EPSILON, rLayerMask, QueryTriggerInteraction.Ignore);
#endif

            int lBodyShapeHitsIndex = 0;
            for (int i = 0; i < lHitCount; i++)
            {
                if (mRaycastHitArray[i].collider.isTrigger) { continue; }
                if (_CharacterController != null && _CharacterController.IsIgnoringCollision(mRaycastHitArray[i].collider)) { continue; }

                Transform lCurrentTransform = mRaycastHitArray[i].collider.transform;
                if (lCurrentTransform == _Transform) { continue; }

                // Ensure we're not colliding with a transform in our chain
                bool lIsValidHit = true;
                while (lCurrentTransform != null)
                {
                    if (lCurrentTransform == _Parent)
                    {
                        lIsValidHit = false;
                        break;
                    }

                    lCurrentTransform = lCurrentTransform.parent;
                }

                if (!lIsValidHit) { continue; }

                // Once we get here, we have a valid collider
                BodyShapeHit lBodyShapeHit = BodyShapeHit.Allocate();
                lBodyShapeHit.StartPosition = lBodyShapePos1;
                lBodyShapeHit.Shape = this;
                lBodyShapeHit.Hit = mRaycastHitArray[i];
                lBodyShapeHit.HitOrigin = lBodyShapePos1;
                lBodyShapeHit.HitCollider = mRaycastHitArray[i].collider;
                lBodyShapeHit.HitPoint = mRaycastHitArray[i].point;
                lBodyShapeHit.HitNormal = mRaycastHitArray[i].normal;

                // This distance is the distance between the surfaces and not the start!
                lBodyShapeHit.HitDistance = mRaycastHitArray[i].distance;

                // With the sphere cast all, we can recieve hits for colliders that
                // start by intruding on the sphere. In this case, the distance is "0". So,
                // we'll find the true distance ourselves.
                if (mRaycastHitArray[i].distance == 0f)
                {
                    Vector3 lColliderPoint = Vector3.zero;

                    if (lBodyShapeHit.HitCollider is TerrainCollider)
                    {
                        lColliderPoint = GeometryExt.ClosestPoint(lBodyShapePos1, rDirection * rDistance, _Radius, (TerrainCollider)lBodyShapeHit.HitCollider);
                    }
                    else
                    {
                        lColliderPoint = GeometryExt.ClosestPoint(lBodyShapePos1, _Radius, lBodyShapeHit.HitCollider);
                    }

                    // If we don't have a valid point, we will skip
                    if (lColliderPoint == Vector3.zero)
                    {
                        BodyShapeHit.Release(lBodyShapeHit);
                        continue;
                    }

                    // If the hit is further than our radius, we can skip
                    Vector3 lHitVector = lColliderPoint - lBodyShapePos1;
                    //if (lHitVector.magnitude > _Radius + EPSILON)
                    //{
                    //    BodyShapeHit.Release(lBodyShapeHit);
                    //    continue;
                    //}

                    // Setup the remaining info
                    lBodyShapeHit.HitOrigin = lBodyShapePos1;
                    lBodyShapeHit.HitPoint = lColliderPoint;

                    // We want distance between the surfaces. We have the start point and
                    // surface collider point. So, we remove our radius to get to the surface.
                    lBodyShapeHit.HitDistance = lHitVector.magnitude - _Radius;
                    lBodyShapeHit.HitPenetration = (lBodyShapeHit.HitDistance < 0f);

                    // Shoot a ray for the normal
                    RaycastHit lRaycastHitInfo;
                    if (RaycastExt.SafeRaycast(lBodyShapePos1, lHitVector.normalized, out lRaycastHitInfo, Mathf.Max(lBodyShapeHit.HitDistance + _Radius, _Radius + 0.01f)))
                    {
                        lBodyShapeHit.HitNormal = lRaycastHitInfo.normal;
                    }
                    // If the ray is so close that we can't get a result we can end up here
                    else if (lBodyShapeHit.HitDistance < EPSILON)
                    {
                        lBodyShapeHit.HitNormal = (lBodyShapePos1 - lColliderPoint).normalized;
                    }
                }

                // Add the collision info
                if (lBodyShapeHit != null)
                {
                    // We can't really trust the hit normal we have since it probably came from an edge. So, we'll
                    // shoot a ray along our movement path. This will give us a better angle to look at. However, if we're
                    // falling (probably from gravity), we don't want to replace the edge value.
                    if (rDirection != Vector3.down)
                    {
                        RaycastHit lRaycastHitInfo;
                        if (RaycastExt.SafeRaycast(lBodyShapeHit.HitPoint - (rDirection * rDistance), rDirection, out lRaycastHitInfo, rDistance + _Radius, -1, _Parent))
                        {
                            lBodyShapeHit.HitNormal = lRaycastHitInfo.normal;
                        }
                    }

                    // Store the distance between the hit point and our character's root
                    lBodyShapeHit.HitRootDistance = _Parent.InverseTransformPoint(lBodyShapeHit.HitPoint).y;

                    // Add the valid hit to our array
                    mBodyShapeHitArray[lBodyShapeHitsIndex]= lBodyShapeHit;
                    lBodyShapeHitsIndex++;
                }
            }

            // Return this array. The array should not be kept
            return mBodyShapeHitArray;
        }

        /// <summary>
        /// Grabs the closets contact point to this shape. During the process, we may generate a
        /// new position for the shape
        /// </summary>
        /// <param name="rCollider">Collider we want to find the closest point to</param>
        /// <param name="rMovement">Movement that this body is planning on taking</param>
        /// <param name="rProcessTerrain">Determines if we'll process TerrainColliders</param>
        /// <param name="rShapeTransform">Main transform of "this" body shape we're testing</param>
        /// <param name="rShapePosition">Planned body shape position bsed on rShapeTransform and rMovement</param>
        /// <param name="rContactPoint">Contact point found if a collision occurs or "zero" if none found</param>
        /// <returns></returns>
        public override bool ClosestPoint(Collider rCollider, Vector3 rMovement, bool rProcessTerrain, out Vector3 rShapePoint, out Vector3 rContactPoint)
        {
            Transform lTransform = (_Transform != null ? _Transform : _Parent);
            rShapePoint = lTransform.position + (lTransform.rotation * _Offset);

            // Closest contact point to the shape
            rContactPoint = Vector3.zero;

            // Test the collider for the closest contact point
            if (rProcessTerrain && rCollider is TerrainCollider)
            {
                rContactPoint = GeometryExt.ClosestPoint(rShapePoint, rMovement.normalized, _Radius, (TerrainCollider)rCollider);
            }
            else
            {
                rContactPoint = GeometryExt.ClosestPoint(rShapePoint, _Radius, rCollider);
            }

            // Report back if we have a valid contact point
            return (rContactPoint.sqrMagnitude > 0f);
        }

        /// <summary>
        /// Gets the origin of the hit. This is the closest point between the
        /// hit and the shape
        /// </summary>
        public override Vector3 CalculateHitOrigin(Vector3 rHitPoint, Vector3 rStartPosition, Vector3 rEndPosition)
        {
            return rStartPosition;
        }

        /// <summary>
        /// Creates a unity collider that represents the body shape
        /// </summary>
        public override void CreateUnityColliders()
        {
            if (_Parent == null) { return; }

#if UNITY_EDITOR

            PrefabType lPrefabType = UnityEditor.PrefabUtility.GetPrefabType(_Parent.gameObject);
            if (lPrefabType == PrefabType.Prefab || lPrefabType == PrefabType.ModelPrefab) { return; }

            UnityEngine.Object lPrefabParent = UnityEditor.PrefabUtility.GetPrefabParent(_Parent.gameObject);
            UnityEngine.Object lPrefabObject = UnityEditor.PrefabUtility.GetPrefabObject(_Parent.gameObject);
            if (lPrefabParent == null && lPrefabObject != null) { return; }

            if (UnityEditor.EditorApplication.isPlaying)
            {
#endif

                // Get rid of any existing colliders
                DestroyUnityColliders();

                // Recreate the collider
                Transform lTransform = (_Transform != null ? _Transform : _Parent);
                float lScale = ((lTransform.lossyScale.x + lTransform.lossyScale.y + lTransform.lossyScale.z) / 3f);

                SphereCollider lCollider = lTransform.gameObject.AddComponent<SphereCollider>(); 
                lCollider.radius = _Radius / lScale;
                lCollider.center = _Offset / lScale;

                if (mColliders == null || mColliders.Length == 0) { mColliders = new UnityEngine.Collider[1]; }
                mColliders[0] = lCollider;

#if UNITY_EDITOR
            }
#endif
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Used to render out the inspector for the shape
        /// </summary>
        /// <returns></returns>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            EditorGUILayout.LabelField("Body Sphere", EditorStyles.boldLabel);

            if (_Parent != null)
            {
                //ActorController lController = _Parent.GetComponent<ActorController>();
                //if (lController != null)
                //{
                //    if (lController.OverlapRadius < _Radius)
                //    {
                //        Color lGUIColor = GUI.color;
                //        GUI.color = new Color(1f, 0.7f, 0.7f, 1f);
                //        EditorGUILayout.HelpBox("Overlap radius is less than this radius. Increase overlap radius so collisions can be found.", MessageType.Warning);
                //        GUI.color = lGUIColor;
                //    }

                //    if (_Offset.y <= _Radius && lController.BaseRadius != _Radius && (_Transform == null || _Transform == _Parent))
                //    {
                //        Color lGUIColor = GUI.color;
                //        GUI.color = new Color(1f, 0.7f, 0.7f, 1f);
                //        EditorGUILayout.HelpBox("This shape sits on the ground. So, the 'Grounding Radius' above should probably be the same radius as this shape.", MessageType.Warning);
                //        GUI.color = lGUIColor;
                //    }

                //    //if (lController.MaxStepHeight > Offset.y && _IsEnabledOnGround)
                //    //{
                //    //    Color lGUIColor = GUI.color;
                //    //    GUI.color = new Color(1f, 0.7f, 0.7f, 1f);
                //    //    EditorGUILayout.HelpBox("The step height is higher than the offset. This could cause low collisions to be ignored.", MessageType.Warning);
                //    //    GUI.color = lGUIColor;
                //    //}
                //}
            }

            GUILayout.Space(5);

            string lNewName = EditorGUILayout.TextField(new GUIContent("Name", ""), Name);
            if (lNewName != Name)
            {
                lIsDirty = true;
                Name = lNewName;
            }

            float lNewRadius = EditorGUILayout.FloatField(new GUIContent("Radius", ""), Radius);
            if (lNewRadius != Radius)
            {
                lIsDirty = true;
                Radius = lNewRadius;
            }

            bool lNewUseUnityColliders = EditorGUILayout.Toggle(new GUIContent("Use Unity Colliders", "Determines if we generate unity colliders to match the body shapes."), UseUnityColliders);
            if (lNewUseUnityColliders != UseUnityColliders)
            {
                lIsDirty = true;
                UseUnityColliders = lNewUseUnityColliders;
            }

            GUILayout.Space(5f);

            bool lNewIsEnabledOnGround = EditorGUILayout.Toggle(new GUIContent("Enabled on ground", "Determines if the shape is valid while on the ground."), IsEnabledOnGround);
            if (lNewIsEnabledOnGround != IsEnabledOnGround)
            {
                lIsDirty = true;
                IsEnabledOnGround = lNewIsEnabledOnGround;
            }

            bool lNewIsEnabledOnSlope = EditorGUILayout.Toggle(new GUIContent("Enabled on ramp", "Determines if the shape is valid while on a slope."), IsEnabledOnSlope);
            if (lNewIsEnabledOnSlope != IsEnabledOnSlope)
            {
                lIsDirty = true;
                IsEnabledOnSlope = lNewIsEnabledOnSlope;
            }

            bool lNewIsEnabledAboveGround = EditorGUILayout.Toggle(new GUIContent("Enabled in air", "Determines if the shape is valid while in the air."), IsEnabledAboveGround);
            if (lNewIsEnabledAboveGround != IsEnabledAboveGround)
            {
                lIsDirty = true;
                IsEnabledAboveGround = lNewIsEnabledAboveGround;
            }

            GUILayout.Space(5);

            Transform lNewStartTransform = EditorGUILayout.ObjectField(new GUIContent("Center Transform", ""), Transform, typeof(Transform), true) as Transform;
            if (lNewStartTransform != Transform)
            {
                lIsDirty = true;
                Transform = lNewStartTransform;
            }

            Vector3 lNewStartOffset = EditorGUILayout.Vector3Field(new GUIContent("Center Offset", ""), Offset);
            if (lNewStartOffset != Offset)
            {
                lIsDirty = true;
                Offset = lNewStartOffset;
            }

            return lIsDirty;
        }

#endif

    }
}
