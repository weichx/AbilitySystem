using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using com.ootii.Data.Serializers;
using com.ootii.Geometry;
using com.ootii.Utilities;

namespace com.ootii.Actors
{
    /// <summary>
    /// Contains information that defines a shape we'll use
    /// to help manage collisions with an actor
    /// 
    /// Well, here's the issue...
    /// 
    /// We want to use a list to store the BodyShapes (Body Capsule, Body Sphere, etc.). In
    /// order for Unity to serialize a list of polymorhic types, they need to inherit from
    /// ScriptableObject or it just instanciates the base type (BodyShape).
    /// 
    /// No problem... except if you have an object in your scene with a list of ScriptableObjects,
    /// when you duplicate the object (and the list items get duplicated too) the list items
    /// become REFERENCES to the original items. That means changing a value in the new
    /// list item also changes the original list item. ugh!
    /// 
    /// So, back to faking polymorphic list items.
    /// </summary>
    public abstract class BodyShape
    {
        public const float EPSILON = 0.001f;

        /// <summary>
        /// Friendly name to help identify the shape
        /// </summary>
        public string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Determines if we'll create and use Unity colliders along with the body shapes.
        /// </summary>
        public bool _UseUnityColliders = true;
        public bool UseUnityColliders
        {
            get { return _UseUnityColliders; }

            set
            {
                _UseUnityColliders = value;

                if (_UseUnityColliders)
                {
                    if (mColliders == null || mColliders.Length == 0)
                    {
                        CreateUnityColliders();
                    }
                }
                else
                {
                    if (mColliders != null)
                    {
                        DestroyUnityColliders();
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the body shape is enabled while we are grounded
        /// </summary>
        public bool _IsEnabledOnGround = true;
        public bool IsEnabledOnGround
        {
            get { return _IsEnabledOnGround; }
            set { _IsEnabledOnGround = value; }
        }

        /// <summary>
        /// Determines if the body shape is enabled when we're on a slope
        /// </summary>
        public bool _IsEnabledOnSlope = true;
        public bool IsEnabledOnSlope
        {
            get { return _IsEnabledOnSlope; }
            set { _IsEnabledOnSlope = value; }
        }

        /// <summary>
        /// Determines if the body shape is enabled when we're not grounded
        /// </summary>
        public bool _IsEnabledAboveGround = true;
        public bool IsEnabledAboveGround
        {
            get { return _IsEnabledAboveGround; }
            set { _IsEnabledAboveGround = value; }
        }

        /// <summary>
        /// Actor's transform that this shape belongs to
        /// </summary>
        [NonSerialized]
        public Transform _Parent = null;

        [SerializationIgnore]
        public Transform Parent
        {
            get { return _Parent; }
        }

        /// <summary>
        /// Character controller the body shape is tied to
        /// </summary>
        [NonSerialized]
        public ICharacterController _CharacterController = null;

        [SerializationIgnore]
        public ICharacterController CharacterController
        {
            get { return _CharacterController; }
        }

        /// <summary>
        /// Transform the collision sphere is tied to. This allows the
        /// sphere to change positions based on animations.
        /// </summary>
        [NonSerialized]
        public Transform _Transform = null;

        [SerializationIgnore]
        public virtual Transform Transform
        {
            get { return _Transform; }

            set
            {
                if (_Transform == value) { return; }

                _Transform = value;

                if (_UseUnityColliders && mColliders != null)
                {
                    CreateUnityColliders();
                }
            }
        }

        /// <summary>
        /// Relative position from the transform. If no transform
        /// is defined, it's the position from the origin.
        /// </summary>
        public Vector3 _Offset = Vector3.zero;
        public virtual Vector3 Offset
        {
            get { return _Offset; }
            set { _Offset = value; }
        }

        /// <summary>
        /// Radius of the sphere
        /// </summary>
        public float _Radius = 0.25f;
        public virtual float Radius
        {
            get { return _Radius; }
            set { _Radius = value; }
        }

        /// <summary>
        /// Returns the collider associated with the body shape
        /// </summary>
        protected Collider[] mColliders = null;

        [SerializationIgnore]
        public virtual Collider[] Colliders
        {
            get { return mColliders; }
            set { mColliders = value; }
        }

        [SerializationIgnore]
        public virtual Collider Collider
        {
            get
            {
                if (mColliders == null || mColliders.Length == 0) { return null; }
                return mColliders[0];
            }

            set
            {
                if (mColliders == null || mColliders.Length == 0) { mColliders = new Collider[1]; }
                mColliders[0] = value;
            }
        }

        /// <summary>
        /// Keeps us from reallocating the arrays over and over
        /// </summary>
        protected RaycastHit[] mRaycastHitArray = new RaycastHit[15];
        protected BodyShapeHit[] mBodyShapeHitArray = new BodyShapeHit[15];

        /// <summary>
        /// Typically body shapes don't need to be updated. However, if we're dealing
        /// with capsules, we may need to move the spheres that represent them
        /// </summary>
        public virtual void LateUpdate()
        {
        }

        /// <summary>
        /// Checks if the shape currently overlaps any colliders
        /// </summary>
        /// <param name="rPositionDelta">Movement to add to the current position</param>
        /// <param name="rLayerMask">Layer mask for determing what we'll collide with</param>
        /// <returns>Boolean that says if a hit occured or not</returns>
        public virtual List<BodyShapeHit> CollisionOverlap(Vector3 rPositionDelta, Quaternion rRotationDelta, int rLayerMask)
        {
            return null;
        }

        /// <summary>
        /// Casts out a shape to see if a collision will occur.
        /// </summary>
        /// <param name="rPositionDelta">Movement to add to the current position</param>
        /// <param name="rDirection">Direction of the cast</param>
        /// <param name="rDistance">Distance of the case</param>
        /// <param name="rLayerMask">Layer mask for determing what we'll collide with</param>
        /// <returns>Returns an array of BodyShapeHit values representing all the hits that take place</returns>
        public virtual BodyShapeHit[] CollisionCastAll(Vector3 rPositionDelta, Vector3 rDirection, float rDistance, int rLayerMask)
        {
            return null;
        }

        /// <summary>
        /// Grabs the closest point on the actor's body shapes to the origin
        /// </summary>
        /// <param name="rOrigin">Position we're testing from</param>
        /// <returns>Position on the body shape surfaces that is the closest point or Vector3.zero if no point is found</returns>
        public virtual Vector3 ClosestPoint(Vector3 rOrigin)
        {
            Transform lTransform = (_Transform != null ? _Transform : _Parent);
            Vector3 lPosition = lTransform.position + (lTransform.rotation * _Offset);

            // Closest contact point to the sphere based on 
            return GeometryExt.ClosestPoint(rOrigin, lPosition, _Radius);
        }
        
        /// <summary>
        /// Grabs the closets contact point to this shape. During the process, we may generate a
        /// new position for the shape
        /// </summary>
        /// <param name="rCollider">Collider we want to find the closest point to</param>
        /// <param name="rMovement">Movement that this body is planning on taking (for terrain checks)</param>
        /// <param name="rProcessTerrain">Determines if we'll process TerrainColliders</param>
        /// <param name="rContactPoint">Contact point found if a collision occurs or "zero" if none found</param>
        /// <returns></returns>
        public virtual bool ClosestPoint(Collider rCollider, Vector3 rMovement, bool rProcessTerrain, out Vector3 rShapePoint, out Vector3 rContactPoint)
        {
            Transform lTransform = (_Transform != null ? _Transform : _Parent);
            rShapePoint = lTransform.position + (lTransform.rotation * _Offset);

            rContactPoint = Vector3.zero;

            return false;
        }

        /// <summary>
        /// Gets the origin of the hit. This is the closest point between the
        /// hit and the shape
        /// </summary>
        public virtual Vector3 CalculateHitOrigin(Vector3 rHitPoint, Vector3 rStartPosition, Vector3 rEndPosition)
        {
            return GeometryExt.ClosestPoint(rHitPoint, rStartPosition, rEndPosition);
        }

        /// <summary>
        /// Creates a unity collider that represents the body shape
        /// </summary>
        public virtual void CreateUnityColliders()
        {
        }

        /// <summary>
        /// Destroys the unity collider that represents the body shape
        /// </summary>
        public virtual void DestroyUnityColliders()
        {
            if (mColliders != null)
            {
                for (int i = 0; i < mColliders.Length; i++)
                {
                    Collider lCollider = mColliders[i];

                    if (Application.isPlaying)
                    {
                        UnityEngine.Collider.Destroy(lCollider);
                    }
                    else
                    {
                        UnityEngine.Collider.DestroyImmediate(lCollider);
                    }
                }
            }

            mColliders = null;
        }

        /// <summary>
        /// Creates a JSON string that represents the shape's serialized state. We
        /// do this since Unity can't handle putting lists of derived objects into
        /// prefabs.
        /// </summary>
        /// <returns>JSON string representing the object</returns>
        public virtual string Serialize()
        {
            StringBuilder lStringBuilder = new StringBuilder();
            lStringBuilder.Append("{");

            // These four properties are important from the base MotionControllerMotion
            lStringBuilder.Append(", \"Type\" : \"" + this.GetType().AssemblyQualifiedName + "\"");

            // Cycle through all the properties. 
            // Unfortunately Binding flags don't seem to be working. So,
            // we need to ensure we don't include base properties
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }
                if (lProperty.GetValue(this, null) == null) { continue; }

                object lValue = lProperty.GetValue(this, null);
                if (lProperty.PropertyType == typeof(Vector2))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector2)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Vector3))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector3)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Vector4))
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + ((Vector4)lValue).ToString("G8") + "\"");
                }
                else if (lProperty.PropertyType == typeof(Transform))
                {
                    string lTransformPath = "";

                    Transform lCurrentTransform = (Transform)lValue;
                    while (lCurrentTransform != null)
                    {
                        // We use this condition to ensure we're dealing with a relative path
                        // and not a global path
                        if (lCurrentTransform.parent != null)
                        {
                            lTransformPath = lCurrentTransform.name + (lTransformPath.Length > 0 ? "/" : "") + lTransformPath;
                        }

                        lCurrentTransform = lCurrentTransform.parent;
                        if (lCurrentTransform == _Parent) { break; }
                    }

                    if (lTransformPath.Length == 0) { lTransformPath = "."; }
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + lTransformPath + "\"");
                }
                else
                {
                    lStringBuilder.Append(", \"" + lProperty.Name + "\" : \"" + lValue.ToString() + "\"");
                }
            }

            lStringBuilder.Append("}");

            return lStringBuilder.ToString();
        }

        /// <summary>
        /// Given a JSON string that is the definition of the object, we parse
        /// out the properties and set them.
        /// </summary>
        /// <param name="rDefinition">JSON string</param>
        public virtual void Deserialize(string rDefinition)
        {
            JSONNode lDefinitionNode = JSONNode.Parse(rDefinition);
            if (lDefinitionNode == null) { return; }

            // Cycle through the properties and load the values we can
            PropertyInfo[] lProperties = this.GetType().GetProperties();
            foreach (PropertyInfo lProperty in lProperties)
            {
                if (!lProperty.CanWrite) { continue; }

                JSONNode lValueNode = lDefinitionNode[lProperty.Name];
                if (lValueNode == null) { continue; }

                if (lProperty.PropertyType == typeof(string))
                {
                    lProperty.SetValue(this, lValueNode.Value, null);
                }
                else if (lProperty.PropertyType == typeof(int))
                {
                    lProperty.SetValue(this, lValueNode.AsInt, null);
                }
                else if (lProperty.PropertyType == typeof(float))
                {
                    lProperty.SetValue(this, lValueNode.AsFloat, null);
                }
                else if (lProperty.PropertyType == typeof(bool))
                {
                    lProperty.SetValue(this, lValueNode.AsBool, null);
                }
                else if (lProperty.PropertyType == typeof(Vector2))
                {
                    Vector2 lVector2Value = Vector2.zero;
                    lVector2Value = lVector2Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector2Value, null);
                }
                else if (lProperty.PropertyType == typeof(Vector3))
                {
                    Vector3 lVector3Value = Vector3.zero;
                    lVector3Value = lVector3Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector3Value, null);
                }
                else if (lProperty.PropertyType == typeof(Vector4))
                {
                    Vector4 lVector4Value = Vector4.zero;
                    lVector4Value = lVector4Value.FromString(lValueNode.Value);

                    lProperty.SetValue(this, lVector4Value, null);
                }
                else if (lProperty.PropertyType == typeof(Transform))
                {
                    if (_Parent != null)
                    {
                        if (lValueNode.Value == ".")
                        {
                            // In this case, we want ourselves
                            lProperty.SetValue(this, _Parent, null);
                        }
                        else
                        {
                            // In this case, we want a relative path
                            Transform lTransform = _Parent.Find(lValueNode.Value);
                            if (lTransform != null) { lProperty.SetValue(this, lTransform, null); }
                        }
                    }
                }
                else
                {
                    //JSONClass lObject = lValueNode.AsObject;
                }
            }
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Used to render out the inspector for the shape
        /// </summary>
        /// <returns></returns>
        public virtual bool OnInspectorGUI()
        {
            return false;
        }

#endif

    }
}
