using UnityEngine;
using com.ootii.Collections;
using com.ootii.Geometry;

namespace com.ootii.Actors
{
    /// <summary>
    /// Contains information about a collision hit
    /// </summary>
    public class BodyShapeHit
    {
        /// <summary>
        /// Shape that tested for collision
        /// </summary>
        public BodyShape Shape;

        /// <summary>
        /// Start position of the shape. Either the center for a sphere or the
        /// "inner" end of a capsule.
        /// </summary>
        public Vector3 StartPosition;

        /// <summary>
        /// Zero for a sphere or the "inner" end of a capsule
        /// </summary>
        public Vector3 EndPosition;

        /// <summary>
        /// Collider that was hit
        /// </summary>
        public Collider HitCollider;

        /// <summary>
        /// Point on the shape's axis that the collision ray came from. This the
        /// StartPosition for a sphere or the closest point on the capsule's axis
        /// </summary>
        public Vector3 HitOrigin;

        /// <summary>
        /// Point on the collider's skin that we collided with. 
        /// </summary>
        public Vector3 HitPoint;

        /// <summary>
        /// Normal of the skin point that we collided with. 
        /// </summary>
        public Vector3 HitNormal;

        /// <summary>
        /// Distance between the surfaces that hit... NOT the HitOrigin and HitPoint.
        /// </summary>
        public float HitDistance;

        /// <summary>
        /// Vertical distance between the HitPoint and the root of the actor
        /// </summary>
        public float HitRootDistance;

        /// <summary>
        /// Determines if we're dealing with penetration or not
        /// </summary>
        public bool HitPenetration;

        /// <summary>
        /// Determines if we hit the platform we're currently on
        /// </summary>
        public bool IsPlatformHit;

        /// <summary>
        /// Information about the hit
        /// </summary>
        public RaycastHit Hit;

        /// <summary>
        /// Gets the origin of the hit. This is the closest point between the
        /// hit and the shape
        /// </summary>
        public void CalculateHitOrigin()
        {
            HitOrigin = Shape.CalculateHitOrigin(HitPoint, StartPosition, EndPosition);
        }

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<BodyShapeHit> sPool = new ObjectPool<BodyShapeHit>(20, 5);

        /// <summary>
        /// Returns the number of items allocated
        /// </summary>
        /// <value>The allocated.</value>
        public static int Length
        {
            get { return sPool.Length; }
        }

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static BodyShapeHit Allocate()
        {
            // Grab the next available object
            BodyShapeHit lInstance = sPool.Allocate();

            // Return it
            return lInstance;
        }

        /// <summary>
        /// Pulls an object from the pool.
        /// </summary>
        /// <returns></returns>
        public static BodyShapeHit Allocate(BodyShapeHit rInstance)
        {
            if (rInstance == null)
            {
                return sPool.Allocate();
            }
            else
            {
                BodyShapeHit lInstance = sPool.Allocate();
                lInstance.Shape = rInstance.Shape;
                lInstance.StartPosition = rInstance.StartPosition;
                lInstance.EndPosition = rInstance.EndPosition;
                lInstance.HitCollider = rInstance.HitCollider;
                lInstance.HitOrigin = rInstance.HitOrigin;
                lInstance.HitPoint = rInstance.HitPoint;
                lInstance.HitNormal = rInstance.HitNormal;
                lInstance.HitDistance = rInstance.HitDistance;
                lInstance.HitRootDistance = rInstance.HitRootDistance;
                lInstance.HitPenetration = rInstance.HitPenetration;
                lInstance.IsPlatformHit = rInstance.IsPlatformHit;
                lInstance.Hit = rInstance.Hit;

                return lInstance;
            }
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(BodyShapeHit rInstance)
        {
            if (rInstance == null) { return; }

            // Clear the object
            rInstance.Shape = null;
            rInstance.StartPosition = Vector3.zero;
            rInstance.EndPosition = Vector3.zero;
            rInstance.HitCollider = null;
            rInstance.HitOrigin = Vector3.zero;
            rInstance.HitPoint = Vector3.zero;
            rInstance.HitNormal = Vector3.zero;
            rInstance.HitDistance = 0f;
            rInstance.HitPenetration = false;
            rInstance.Hit = RaycastExt.EmptyHitInfo;

            // Allow it to be reused
            sPool.Release(rInstance);
        }
    }
}
