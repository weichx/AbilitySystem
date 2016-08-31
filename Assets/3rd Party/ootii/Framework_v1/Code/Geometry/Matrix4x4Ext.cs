using System;
using UnityEngine;

namespace com.ootii.Geometry
{
    /// <summary>
    /// Extension for the standard Vector3 that allows us to add functions
    /// </summary>
    public static class Matrix4x4Ext
    {
        /// <summary>
        /// Return the position of the matrix
        /// </summary>
        public static Vector3 Position(this Matrix4x4 rMatrix)
        {
            return rMatrix.GetColumn(3);
        }

        /// <summary>
        /// Return the rotation of the matrix
        /// </summary>
        public static Quaternion Rotation(this Matrix4x4 rMatrix)
        {
            return Quaternion.LookRotation(rMatrix.GetColumn(2), rMatrix.GetColumn(1));
        }

        /// <summary>
        /// Return the scale of the matrix
        /// </summary>
        public static Vector3 Scale(this Matrix4x4 rMatrix)
        {
            return new Vector3(rMatrix.GetColumn(0).magnitude, rMatrix.GetColumn(1).magnitude, rMatrix.GetColumn(2).magnitude);
        }
    }
}
