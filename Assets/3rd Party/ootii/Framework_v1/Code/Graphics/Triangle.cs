using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Graphics
{
    /// <summary>
    /// Provides a way to store lines for our graphics rendering
    /// </summary>
    public class Triangle
    {
        /// <summary>
        /// Transform that provides the reference for the positions
        /// </summary>
        public Transform Transform = null;

        /// <summary>
        /// Point of the triangle
        /// </summary>
        public Vector3 Point1 = Vector3.zero;

        /// <summary>
        /// Point of the triangle
        /// </summary>
        public Vector3 Point2 = Vector3.zero;

        /// <summary>
        /// Point of the triangle
        /// </summary>
        public Vector3 Point3 = Vector3.zero;

        /// <summary>
        /// Color of the shape
        /// </summary>
        public Color Color = Color.white;

        /// <summary>
        /// GameTime that the triangle will expire
        /// </summary>
        public float ExpirationTime = 0f;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<Triangle> sPool = new ObjectPool<Triangle>(20, 5);

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
        public static Triangle Allocate()
        {
            // Grab the next available object
            Triangle lInstance = sPool.Allocate();

            // Return the allocated instance (should be cleaned before
            // being put back into the pool)
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(Triangle rInstance)
        {
            if (object.ReferenceEquals(rInstance, null)) { return; }

            // Set values
            rInstance.Transform = null;
            rInstance.Point1 = Vector3.zero;
            rInstance.Point2 = Vector3.zero;
            rInstance.Point3 = Vector3.zero;
            rInstance.Color = Color.white;
            rInstance.ExpirationTime = 0f;

            // Make it available to others.
            sPool.Release(rInstance);
        }
    }
}
