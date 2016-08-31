/// Tim Tryzbiak, ootii, LLC
using System;
using UnityEngine;

namespace com.ootii.Helpers
{
    /// <summary>
    /// Static functions to help us
    /// </summary>
    public class NumberHelper
    {
        /// <summary>
        /// Create a single instance that we can reuse as needed
        /// </summary>
        public static System.Random Randomizer = new System.Random();

        /// <summary>
        /// Grabs a random value between min (inclusive) and max (inclusive)
        /// </summary>
        public static float GetRandom(float rMin, float rMax)
        {
            return UnityEngine.Random.Range(rMin, rMax);
        }

        /// <summary>
        /// Grabs a random position based on the center and radius
        /// </summary>
        /// <param name="rCenter"></param>
        /// <param name="rRadius"></param>
        /// <param name="rRandomizeY"></param>
        /// <returns></returns>
        public static Vector3 GetRandom(Vector3 rCenter, float rRadius, bool rRandomizeY)
        {
            float lDelta = (rRadius * 2f * (float)Randomizer.NextDouble()) - rRadius;
            rCenter.x = rCenter.x + lDelta;

            if (rRandomizeY)
            {
                lDelta = (rRadius * 2f * (float)Randomizer.NextDouble()) - rRadius;
                rCenter.y = rCenter.y + lDelta;
            }

            lDelta = (rRadius * 2f * (float)Randomizer.NextDouble()) - rRadius;
            rCenter.z = rCenter.z + lDelta;

            return rCenter;
        }
        
        /// <summary>
        /// Returns the smooth and eased value over time (0 to 1)
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rTime"></param>
        /// <returns></returns>
        public static float SmoothStep(float rStart, float rEnd, float rTime)
        {
            if (rTime <= 0f) { return rStart; }
            if (rTime >= 1f) { return rEnd; }

            rTime = rTime * rTime * rTime * (rTime * (6f * rTime - 15f) + 10f);

            float lDelta = rEnd - rStart;
            float lDistance = lDelta * rTime;

            return rStart + lDistance;
        }

        /// <summary>
        /// Clamps the angle (in degrees) between the min and max
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="rAngle">Angle to clamp</param>
        /// <param name="rMin">Minimum value</param>
        /// <param name="rMax">Maximum value</param>
        public static float ClampAngle(float rAngle, float rMin, float rMax)
        {
            if (rAngle < -360) { rAngle += 360; }
            if (rAngle > 360) { rAngle -= 360; }
            return Mathf.Clamp(rAngle, rMin, rMax);
        }

        /// <summary>
        /// Ensure the angle stays within the 360 range
        /// </summary>
        /// <returns>The angle cleaned up angle</returns>
        /// <param name="rAngle">The initial angle</param>
        public static float NormalizeAngle(float rAngle)
        {
            if (rAngle < -360f) { rAngle += 360f; }
            if (rAngle > 360f) { rAngle -= 360f; }
            return rAngle;
        }

        /// <summary>
        /// Gets the vector difference between two vectors minus the
        /// height component
        /// </summary>
        /// <returns>The horizontal difference.</returns>
        /// <param name="rFrom">R from.</param>
        /// <param name="rTo">R to.</param>
        /// <param name="rResult">R result.</param>
        public static float GetHorizontalDistance(Vector3 rFrom, Vector3 rTo)
        {
            rFrom.y = 0;
            rTo.y = 0;
            return (rTo - rFrom).magnitude;
        }

        /// <summary>
        /// Gets the vector difference between two vectors minus the
        /// height component
        /// </summary>
        /// <returns>The horizontal difference.</returns>
        /// <param name="rFrom">R from.</param>
        /// <param name="rTo">R to.</param>
        /// <param name="rResult">R result.</param>
        public static void GetHorizontalDifference(Vector3 rFrom, Vector3 rTo, ref Vector3 rResult)
        {
            rFrom.y = 0;
            rTo.y = 0;
            rResult = rTo - rFrom;
        }

        /// <summary>
        /// Gets the horizontal angle between two vectors. The calculation
        /// removes any y components before calculating the angle.
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="rFrom">Angle representing the starting vector</param>
        /// <param name="rTo">Angle representing the resulting vector</param>
        public static float GetHorizontalAngle(Vector3 rFrom, Vector3 rTo)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(rFrom, rTo)), Vector3.Dot(rFrom, rTo));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001f) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Gets the horizontal angle between two vectors. The calculation
        /// removes any y components before calculating the angle.
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="rFrom">Angle representing the starting vector</param>
        /// <param name="rTo">Angle representing the resulting vector</param>
        public static float GetHorizontalAngle(Vector3 rFrom, Vector3 rTo, Vector3 rUp)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(rUp, Vector3.Cross(rFrom, rTo)), Vector3.Dot(rFrom, rTo));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001f) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Gets the quaternion that represents the normalized rotation between
        /// the two vectors (minus the height component).
        /// </summary>
        /// <param name="rFrom">Source vector</param>
        /// <param name="rTo">Destination vector</param>
        /// <param name="rResult">Rotation result</param>
        public static void GetHorizontalQuaternion(Vector3 rFrom, Vector3 rTo, ref Quaternion rResult)
        {
            rFrom.y = 0;
            rTo.y = 0;
            rResult = Quaternion.LookRotation(rTo - rFrom);
        }

        /// <summary>
        /// Returns the base rased the specified power
        /// </summary>
        /// <param name="rBase"></param>
        /// <param name="rExponent"></param>
        /// <returns></returns>
        public static float Pow(float rBase, int rExponent)
        {
            if (rExponent == 0) { return 0f; }
            else if (rExponent == 1) { return rBase; }

            while (rExponent > 1)
            {
                rBase = rBase * rBase;
                rExponent--;
            }
            
            return rBase;
        }
    }
}

