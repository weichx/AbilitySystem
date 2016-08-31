using System;

namespace com.ootii.Collections
{
    /// <summary>
    /// Provides extensions to arrays
    /// </summary>
    public static class ArrayExt
    {
        /// <summary>
        /// CAUSES GC. Use non-ref version.
        /// Updates the array by removing the specified index
        /// </summary>
        /// <typeparam name="T">Type of array to modify</typeparam>
        /// <param name="rSource">Source that is being modified</param>
        /// <param name="rIndex">Index ot remove</param>
        /// <returns>New array with the index removed</returns>
        public static void RemoveAt<T>(ref T[] rSource, int rIndex)
        {
            if (rSource.Length == 0) { return; }
            if (rIndex < 0 || rIndex >= rSource.Length) { return; }

            T[] lNewArray = new T[rSource.Length - 1];

            if (rIndex > 0)
            {
                Array.Copy(rSource, 0, lNewArray, 0, rIndex);
            }

            if (rIndex < rSource.Length - 1)
            {
                Array.Copy(rSource, rIndex + 1, lNewArray, rIndex, rSource.Length - rIndex - 1);
            }

            rSource = lNewArray;
        }

        /// <summary>
        /// Updates the array by removing the specified index
        /// </summary>
        /// <typeparam name="T">Type of array to modify</typeparam>
        /// <param name="rSource">Source that is being modified</param>
        /// <param name="rIndex">Index ot remove</param>
        /// <returns>New array with the index removed</returns>
        public static T[] RemoveAt<T>(T[] rSource, int rIndex)
        {
            int lLength = rSource.Length;

            if (lLength == 0) { return null; }
            if (rIndex < 0 || rIndex >= lLength) { return null; }

            int i = 0;
            int j = 0;
            T[] lNewArray = new T[lLength - 1];

            while (i < lLength)
            {
                if (i != rIndex)
                {
                    lNewArray[j] = rSource[i];
                    j++;
                }

                i++;
            }

            return lNewArray;
        }

        /// <summary>
        /// Provides an abstracted way to sort. We do it this way so we can change the method
        /// (ie using Linq) if we need to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rSource"></param>
        /// <param name="rComparison"></param>
        public static void Sort<T>(this T[] rSource, Comparison<T> rComparison)
        {
            if (rSource.Length <= 1) { return; }

            Array.Sort(rSource, rComparison);
        }

        /// <summary>
        /// Array extension for finding an item in an array
        /// </summary>
        /// <typeparam name="T">Type of object being test for</typeparam>
        /// <param name="rArray">Array to test</param>
        /// <param name="rValue">Item to search for</param>
        /// <returns></returns>
        public static bool Contains<T>(this T[] rArray, T rValue)
        {
            return Array.Exists<T>(rArray, item => item.Equals(rValue));
        }
    }
}
