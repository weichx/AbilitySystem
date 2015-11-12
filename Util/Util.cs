using System.Collections.Generic;
using UnityEngine;

public static class Util {

    public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 normal) {
        return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    public static float ClampAngle(float angle, float min, float max) {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


    public static float WrapAngle180(float angle) {
        if (angle > 180f) {
            return -(360f - angle);
        }
        else if (angle < -180f) {
            return angle + 360f;
        }
        else if (angle == -180) {
            return 180f;
        }
        return angle;
    }

    public static float WrapRadianPI(float radians) {
        if (radians > Mathf.PI) {
            return -((Mathf.PI * 2) - radians);
        }
        else if (radians < -Mathf.PI) {
            return radians + (Mathf.PI * 2f);
        }
        return radians;
    }

    public static void LogAll(params object[] list) {
        var str = "";
        for (int i = 0; i < list.Length; i++) {
            str += list[i].ToString() + " ";
        }
        Debug.Log(str);
    }

    public static bool Between(float a, float compare, float b) {
        return (a < compare && compare < b);
    }
}

public static class DictionaryExtensions {

    public static U Get<T, U>(this Dictionary<T, U> dict, T key) where U : class {
        U val;
        dict.TryGetValue(key, out val);
        return val;
    }
}

public static class Vector3Extensions {

    public static float AnglePreNormalized(this Vector3 self, Vector3 to) {
        return Mathf.Acos(Mathf.Clamp(Vector3.Dot(self, to), -1f, 1f)) * 57.29578f;
    }

    public static Vector3 From(this Vector3 self, Vector3 other) {
        return self - other;
    }

    public static Vector3 DirectionFrom(this Vector3 self, Vector3 other) {
        return (self - other).normalized;
    }

    public static Vector3 To(this Vector3 self, Vector3 other) {
        return other - self;
    }

    public static Vector3 DirectionTo(this Vector3 self, Vector3 other) {
        return (other - self).normalized;
    }

    public static float Dot(this Vector3 self, Vector3 other) {
        return Vector3.Dot(self, other);
    }
}

public static class CircularLinkedList {
    public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> current) {
        if (current.Next == null)
            return current.List.First;
        return current.Next;
    }

    public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> current) {
        if (current.Previous == null)
            return current.List.Last;
        return current.Previous;
    }
}
