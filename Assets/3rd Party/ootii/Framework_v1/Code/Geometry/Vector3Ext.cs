using System;
using UnityEngine;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Extension for the standard Vector3 that allows us to add functions
    /// </summary>
    public static class Vector3Ext
    {
        /// <summary>
        /// Used when we need to return an empty vector
        /// </summary>
        public static Vector3 Null = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        /// <summary>
        /// Used to determine the angle of two vectors in relation to 
        /// the bone. This is important since the 'forward' direction of the
        /// bone can change
        /// </summary>
        /// <param name="rBone"></param>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <returns></returns>
        public static float SignedAngle(Vector3 rFrom, Vector3 rTo, Vector3 rAxis)
        {
            if (rTo == rFrom) { return 0f; }

            Vector3 lCross = Vector3.Cross(rFrom, rTo);
            float lDot = Vector3.Dot(rFrom, rTo);
            float lSign = (Vector3.Dot(rAxis, lCross) < 0 ? -1 : 1);

            return lSign * Mathf.Atan2(lCross.magnitude, lDot) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Determines the signed angle required to go from one vector to another.
        /// </summary>
        /// <param name="rThis">The source vector</param>
        /// <param name="rTarget">The destination vector</param>
        /// <returns></returns>
        public static float SignedAngle(Vector3 rFrom, Vector3 rTo)
        {
            if (rTo == rFrom) { return 0f; }

            Vector3 lCross = Vector3.Cross(rFrom, rTo);
            float lSign = (lCross.y < 0 ? -1 : 1);

            float lDot = Vector3.Dot(rFrom, rTo);

            return lSign * Mathf.Atan2(lCross.magnitude, lDot) * Mathf.Rad2Deg;
        }
        
        /// <summary>
        /// Determines the signed angle required to go from one vector to another.
        /// </summary>
        /// <param name="rThis">The source vector</param>
        /// <param name="rTarget">The destination vector</param>
        /// <returns></returns>
        public static float AngleTo(this Vector3 rFrom, Vector3 rTo)
        {
            return Vector3Ext.SignedAngle(rFrom, rTo);
        }

        /// <summary>
        /// Extract out the yaw and pitch required to get us from one vector direction to another. Note
        /// that we may need to account for the object's rotation.
        /// </summary>
        /// <param name="rFrom"></param>
        /// <param name="rTo"></param>
        /// <param name="rYaw"></param>
        /// <param name="rPitch"></param>
        public static void DecomposeYawPitch(Transform rOwner, Vector3 rFrom, Vector3 rTo, ref float rYaw, ref float rPitch)
        {
            // Determine the rotations required to view the target
            Vector3 lDelta = rTo - rFrom;
            rPitch = (-Mathf.Atan2(lDelta.y, Mathf.Sqrt((lDelta.x * lDelta.x) + (lDelta.z * lDelta.z))) * Mathf.Rad2Deg) + rOwner.rotation.eulerAngles.x;
            rYaw = (-Mathf.Atan2(lDelta.z, lDelta.x) * Mathf.Rad2Deg) + 90f - rOwner.rotation.eulerAngles.y;
        }

        /// <summary>
        /// Search the dictionary based on value and return the key
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="rDictionary">Object the extension is tied to</param>
        /// <param name="rValue">Value that we are searching for</param>
        /// <returns>Returns the first key associated with the value</returns>
        public static float HorizontalMagnitude(this Vector3 rVector)
        {
            return Mathf.Sqrt((rVector.x * rVector.x) + (rVector.z * rVector.z));
        }

        /// <summary>
        /// Search the dictionary based on value and return the key
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="rDictionary">Object the extension is tied to</param>
        /// <param name="rValue">Value that we are searching for</param>
        /// <returns>Returns the first key associated with the value</returns>
        public static float HorizontalSqrMagnitude(this Vector3 rVector)
        {
            return (rVector.x * rVector.x) + (rVector.z * rVector.z);
        }

        /// <summary>
        /// Gets the angle required to reach the target direction vector
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="rFrom">Starting direction vector</param>
        /// <param name="rTo">Resulting direction vector</param>
        /// <remarks>
        /// In unity:
        /// Rotation angle is posative when going clockwise
        /// Rotation angle is negative when going counter-clockwise
        /// 
        /// When using trig functions:
        /// Rotation angle is negative when going clockwise
        /// Rotation angle is posative when going counter-clockwise
        /// 
        ///   0 angle is to the right (+x)
        /// -90 angle is to the back (-z)
        /// 180 angle is to the left (-x)
        ///  90 angle is to the forward (+z)
        /// </remarks>
        public static float HorizontalAngleTo(this Vector3 rFrom, Vector3 rTo)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(rFrom, rTo)), Vector3.Dot(rFrom, rTo));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Gets the angle required to reach the target direction vector
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="rFrom">Starting direction vector</param>
        /// <param name="rTo">Resulting direction vector</param>
        /// <remarks>
        /// In unity:
        /// Rotation angle is posative when going clockwise
        /// Rotation angle is negative when going counter-clockwise
        /// 
        /// When using trig functions:
        /// Rotation angle is negative when going clockwise
        /// Rotation angle is posative when going counter-clockwise
        /// 
        ///   0 angle is to the right (+x)
        /// -90 angle is to the back (-z)
        /// 180 angle is to the left (-x)
        ///  90 angle is to the forward (+z)
        /// </remarks>
        public static float HorizontalAngleTo(this Vector3 rFrom, Vector3 rTo, Vector3 rUp)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(rUp, Vector3.Cross(rFrom, rTo)), Vector3.Dot(rFrom, rTo));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Gets the angle required to reach this direction vector
        /// </summary>
        /// <returns>The signed horizontal angle (in degrees).</returns>
        /// <param name="rTo">Resulting direction vector</param>
        /// <param name="rFrom">Starting direction vector</param>
        public static float HorizontalAngleFrom(this Vector3 rTo, Vector3 rFrom)
        {
            float lAngle = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(rFrom, rTo)), Vector3.Dot(rFrom, rTo));
            lAngle *= Mathf.Rad2Deg;

            if (Mathf.Abs(lAngle) < 0.0001) { lAngle = 0f; }

            return lAngle;
        }

        /// <summary>
        /// Find the distance to the specified vector given a specific amount of vertical tolerance 
        /// to remove. In this way, we can get rid of any reasonable height differences.
        /// </summary>
        /// <param name="rFrom">Current position</param>
        /// <param name="rTo">Position we're measuring to</param>
        /// <param name="rYTolerance">Amount to reduce the Y diffence by</param>
        /// <returns></returns>
        public static float DistanceTo(this Vector3 rFrom, Vector3 rTo, float rYTolerance)
        {
            float lDiffY = rTo.y - rFrom.y;
            if (lDiffY > 0) { lDiffY = Mathf.Max(lDiffY - rYTolerance, 0f); }
            else if (lDiffY < 0) { lDiffY = Mathf.Min(lDiffY + rYTolerance, 0f); }

            rTo.y = rFrom.y + lDiffY;
            return Vector3.Distance(rFrom, rTo);
        }

        /// <summary>
        /// Returns a normalized vector that represents the direction required to
        /// get from the 'From' position to the 'To' position..
        /// </summary>
        /// <param name="rFrom">Originating position</param>
        /// <param name="rTo">Target position we're heading to</param>
        /// <returns>Normalized vector direction</returns>
        public static Vector3 DirectionTo(this Vector3 rFrom, Vector3 rTo)
        {
            Vector3 lDifference = rTo - rFrom;
            return lDifference.normalized;
        }

        /// <summary>
        /// Normalizes the rotational values from -180 to 180. This is important for things
        /// like averaging where 0-to-360 gives different values than -180-to-180. For example:
        // (-10 + 10) / 2 = 0 = what we're expecting
        // (350 + 10) / 2 = 180 = not what we're expecting 
        /// </summary>
        /// <param name="rSum"></param>
        /// <returns></returns>
        public static Vector3 NormalizeRotations(this Vector3 rThis)
        {
            Vector3 lResult = rThis;

            rThis.x = (rThis.x < -180 ? rThis.x + 360f : (rThis.x > 180f ? rThis.x - 360f : rThis.x));
            rThis.y = (rThis.y < -180 ? rThis.y + 360f : (rThis.y > 180f ? rThis.y - 360f : rThis.y));
            rThis.z = (rThis.z < -180 ? rThis.z + 360f : (rThis.z > 180f ? rThis.z - 360f : rThis.z));

            return lResult;
        }

        /// <summary>
        /// Add angular rotations (pitch, yaw, and roll). We add them from a range
        /// of (-180 to 180). This way a rotation of -10 + 10 will cancel each other out.
        /// </summary>
        /// <param name="rFrom">Originating position</param>
        /// <param name="rTo">Target position we're heading to</param>
        /// <returns>Normalized vector direction</returns>
        public static Vector3 AddRotation(this Vector3 rFrom, Vector3 rTo)
        {
            Vector3 lResult = rFrom;

            //rFrom.x = (rFrom.x < -180 ? rFrom.x + 360f : (rFrom.x > 180f ? rFrom.x - 360f : rFrom.x));
            //rFrom.y = (rFrom.y < -180 ? rFrom.y + 360f : (rFrom.y > 180f ? rFrom.y - 360f : rFrom.y));
            //rFrom.z = (rFrom.z < -180 ? rFrom.z + 360f : (rFrom.z > 180f ? rFrom.z - 360f : rFrom.z));

            //rTo.x = (rTo.x < -180 ? rTo.x + 360f : (rTo.x > 180f ? rTo.x - 360f : rTo.x));
            //rTo.y = (rTo.y < -180 ? rTo.y + 360f : (rTo.y > 180f ? rTo.y - 360f : rTo.y));
            //rTo.z = (rTo.z < -180 ? rTo.z + 360f : (rTo.z > 180f ? rTo.z - 360f : rTo.z));

            lResult = lResult + rTo;

            return lResult;
        }

        /// <summary>
        /// Add angular rotations (pitch, yaw, and roll). We add them from a range
        /// of (-180 to 180). This way a rotation of -10 + 10 will cancel each other out.
        /// </summary>
        /// <param name="rFrom">Originating position</param>
        /// <param name="rTo">Target position we're heading to</param>
        /// <returns>Normalized vector direction</returns>
        public static Vector3 AddRotation(this Vector3 rFrom, float rX, float rY, float rZ)
        {
            Vector3 lResult = rFrom;

            //rFrom.x = (rFrom.x < -180 ? rFrom.x + 360f : (rFrom.x > 180f ? rFrom.x - 360f : rFrom.x));
            //rFrom.y = (rFrom.y < -180 ? rFrom.y + 360f : (rFrom.y > 180f ? rFrom.y - 360f : rFrom.y));
            //rFrom.z = (rFrom.z < -180 ? rFrom.z + 360f : (rFrom.z > 180f ? rFrom.z - 360f : rFrom.z));

            //rX = (rX < -180 ? rX + 360f : (rX > 180f ? rX - 360f : rX));
            //rY = (rY < -180 ? rY + 360f : (rY > 180f ? rY - 360f : rY));
            //rZ = (rZ < -180 ? rZ + 360f : (rZ > 180f ? rZ - 360f : rZ));

            lResult.x = lResult.x + rX;
            lResult.y = lResult.y + rY;
            lResult.z = lResult.z + rZ;

            return lResult;
        }

        /// <summary>
        /// Find the two vectors that are orthogonal to the normal. These vectors
        /// can be used to define the plane the original vector is the normal of.        /// 
        /// </summary>
        /// <param name="rNormal"></param>
        /// <param name="rOrthoUp"></param>
        /// <param name="rOrthoRight"></param>
        public static void FindOrthogonals(Vector3 rNormal, ref Vector3 rOrthoUp, ref Vector3 rOrthoRight)
        {
            rNormal.Normalize();

            rOrthoRight = Quaternion.AngleAxis(90, Vector3.right) * rNormal;
            if (Mathf.Abs(Vector3.Dot(rNormal, rOrthoRight)) > 0.6f)
            {
                rOrthoRight = Quaternion.AngleAxis(90, Vector3.up) * rNormal;
            }

            rOrthoRight.Normalize();

            rOrthoRight = Vector3.Cross(rNormal, rOrthoRight).normalized;
            rOrthoUp = Vector3.Cross(rNormal, rOrthoRight).normalized;
        }

        //Convert a plane defined by 3 points to a plane defined by a vector and a point. 
        //The plane point is the middle of the triangle defined by the 3 points.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rVertexA"></param>
        /// <param name="rVertexB"></param>
        /// <param name="rVertexC"></param>
        /// <returns></returns>
        public static Vector3 PlaneNormal(Vector3 rVertexA, Vector3 rVertexB, Vector3 rVertexC)
        {
            // Make two vectors from the 3 input points, originating from point A
            Vector3 lAB = rVertexB - rVertexA;
            Vector3 lAC = rVertexC - rVertexA;

            //Calculate the normal
            return Vector3.Cross(lAC, lAB).normalized;
        }

        /// <summary>
        /// Convert a plane defined by 3 points to a plane defined by a vector and a point. 
        /// The plane point is the middle of the triangle defined by the 3 points.
        /// </summary>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="pointC"></param>
        public static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {

            planeNormal = Vector3.zero;
            planePoint = Vector3.zero;

            //Make two vectors from the 3 input points, originating from point A
            Vector3 AB = pointB - pointA;
            Vector3 AC = pointC - pointA;

            //Calculate the normal
            planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));

            //Get the points in the middle AB and AC
            Vector3 middleAB = pointA + (AB / 2.0f);
            Vector3 middleAC = pointA + (AC / 2.0f);

            //Get vectors from the middle of AB and AC to the point which is not on that line.
            Vector3 middleABtoC = pointC - middleAB;
            Vector3 middleACtoB = pointB - middleAC;

            //Calculate the intersection between the two lines. This will be the center 
            //of the triangle defined by the 3 points.
            //We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors 
            //this sometimes doesn't work.
            Vector3 temp;
            ClosestPointsOnTwoLines(out planePoint, out temp, middleAB, middleABtoC, middleAC, middleACtoB);
        }

        //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        //to each other. This function finds those two points. If the lines are not parallel, the function 
        //outputs true, otherwise false.
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {

                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// Linearly moves the value to the target.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="currentVelocity"></param>
        /// <param name="smoothTime"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static Vector3 MoveTo(Vector3 rValue, Vector3 rTarget, float rVelocity, float rDeltaTime)
        {
            if (rValue == rTarget) { return rTarget; }

            Vector3 lStep = (rTarget - rValue).normalized * rVelocity;
            Vector3 lNewValue = rValue + (lStep * rDeltaTime);

            if (lNewValue.sqrMagnitude > rTarget.sqrMagnitude) { return rTarget; }
            return lNewValue;
        }

        /// <summary>
        /// Parses out the vector values given a string
        /// </summary>
        /// <param name="rThis">Vector we are filling</param>
        /// <param name="rString">String containing the vector values. In the form of "(0,0)"</param>
        public static Vector2 FromString(this Vector2 rThis, string rString)
        {
            string[] lTemp = rString.Substring(1, rString.Length - 2).Split(',');
            if (lTemp.Length != 2) { return rThis; }

            rThis.x = float.Parse(lTemp[0]);
            rThis.y = float.Parse(lTemp[1]);
            return rThis;
        }

        /// <summary>
        /// Parses out the vector values given a string
        /// </summary>
        /// <param name="rThis">Vector we are filling</param>
        /// <param name="rString">String containing the vector values. In the form of "(0,0,0)"</param>
        public static Vector3 FromString(this Vector3 rThis, string rString)
        {
            string[] lTemp = rString.Substring(1, rString.Length - 2).Split(',');
            if (lTemp.Length != 3) { return rThis; }

            rThis.x = float.Parse(lTemp[0]);
            rThis.y = float.Parse(lTemp[1]);
            rThis.z = float.Parse(lTemp[2]);
            return rThis;
        }

        /// <summary>
        /// Parses out the vector values given a string
        /// </summary>
        /// <param name="rThis">Vector we are filling</param>
        /// <param name="rString">String containing the vector values. In the form of "(0,0,0)"</param>
        public static Vector4 FromString(this Vector4 rThis, string rString)
        {
            string[] lTemp = rString.Substring(1, rString.Length - 2).Split(',');
            if (lTemp.Length != 4) { return rThis; }

            rThis.x = float.Parse(lTemp[0]);
            rThis.y = float.Parse(lTemp[1]);
            rThis.z = float.Parse(lTemp[2]);
            rThis.w = float.Parse(lTemp[3]);
            return rThis;
        }

        /// <summary>
        /// Vector dot product
        /// </summary>
        public static float Dot(this Vector3 rThis, Vector3 rTarget)
        {
            return (rThis.x * rTarget.x) + (rThis.y * rTarget.y) + (rThis.z * rTarget.z);
        }

        /// <summary>
        /// Returns the smooth and eased value over time (0 to 1)
        /// </summary>
        /// <param name="rStart"></param>
        /// <param name="rEnd"></param>
        /// <param name="rTime"></param>
        /// <returns></returns>
        public static Vector3 SmoothStep(Vector3 rStart, Vector3 rEnd, float rTime)
        {
            if (rTime <= 0f) { return rStart; }
            if (rTime >= 1f) { return rEnd; }

            rTime = rTime * rTime * rTime * (rTime * (6f * rTime - 15f) + 10f);

            Vector3 lDelta = rEnd - rStart;
            float lDistance = lDelta.magnitude * rTime;

            return rStart + (lDelta.normalized * lDistance);
        }
    }
}
