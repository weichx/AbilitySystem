using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Graphics
{
    /// <summary>
    /// Provides a way to store characters for our graphics rendering
    /// </summary>
    public class TextCharacter
    {
        /// <summary>
        /// Character this class represents
        /// </summary>
        public char Character = (char)0;

        /// <summary>
        /// Colors representing the extracted pixels
        /// </summary>
        public Color[] Pixels = null;

        /// <summary>
        /// Width of the character
        /// </summary>
        public int Width = 0;

        /// <summary>
        /// Height of the character
        /// </summary>
        public int Height = 0;

        public int MinX = 0;

        public int MinY = 0;

        public int Advance = 0;

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<TextCharacter> sPool = new ObjectPool<TextCharacter>(20, 5);

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
        public static TextCharacter Allocate()
        {
            // Grab the next available object
            TextCharacter lInstance = sPool.Allocate();

            // Return the allocated instance (should be cleaned before
            // being put back into the pool)
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(TextCharacter rInstance)
        {
            if (object.ReferenceEquals(rInstance, null)) { return; }

            // Set values
            rInstance.Character = (char)0;
            rInstance.Pixels = null;
            rInstance.Width = 0;
            rInstance.Height = 0;

            // Make it available to others.
            sPool.Release(rInstance);
        }
    }
}
