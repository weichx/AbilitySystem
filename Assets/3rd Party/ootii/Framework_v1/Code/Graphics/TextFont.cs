using System.Collections.Generic;
using UnityEngine;
using com.ootii.Collections;

namespace com.ootii.Graphics
{
    /// <summary>
    /// Provides a way to store characters for our graphics rendering
    /// </summary>
    public class TextFont
    {
        /// <summary>
        /// Character this class represents
        /// </summary>
        public Font Font = null;

        /// <summary>
        /// Colors representing the extracted pixels
        /// </summary>
        public Texture2D Texture = null;

        /// <summary>
        /// Mininum start value of all characters
        /// </summary>
        public int MinX = 0;

        /// <summary>
        /// Maximum end value of all characters
        /// </summary>
        public int MaxX = 0;

        /// <summary>
        /// Mininum start value of all characters
        /// </summary>
        public int MinY = 0;

        /// <summary>
        /// Maximum end value of all characters
        /// </summary>
        public int MaxY = 0;

        /// <summary>
        /// Represents the character pixels for each used character
        /// </summary>
        public Dictionary<char, TextCharacter> Characters = new Dictionary<char, TextCharacter>();

        // ******************************** OBJECT POOL ********************************

        /// <summary>
        /// Allows us to reuse objects without having to reallocate them over and over
        /// </summary>
        private static ObjectPool<TextFont> sPool = new ObjectPool<TextFont>(20, 5);

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
        public static TextFont Allocate()
        {
            // Grab the next available object
            TextFont lInstance = sPool.Allocate();

            // Return the allocated instance (should be cleaned before
            // being put back into the pool)
            return lInstance;
        }

        /// <summary>
        /// Returns an element back to the pool.
        /// </summary>
        /// <param name="rEdge"></param>
        public static void Release(TextFont rInstance)
        {
            if (object.ReferenceEquals(rInstance, null)) { return; }

            if (rInstance.Texture != null)
            {
                Texture2D.Destroy(rInstance.Texture);
            }

            foreach (TextCharacter lCharacter in rInstance.Characters.Values)
            {
                TextCharacter.Release(lCharacter);
            }

            // Set values
            rInstance.Font = null;
            rInstance.Texture = null;
            rInstance.Characters.Clear();
            rInstance.MinX = 0;
            rInstance.MaxX = 0;
            rInstance.MinY = 0;
            rInstance.MaxY = 0;

            // Make it available to others.
            sPool.Release(rInstance);
        }
    }
}
