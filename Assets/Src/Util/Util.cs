using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

//todo method from vector -- use dotpeek to copy
public struct Vector2I {
    public int x;
    public int y;
}

public struct Vector3I {
    public int x;
    public int y;
    public int z;
}

public static class Util {

    public static string SplitAndTitlize(string input) {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(input, "(\\B[A-Z])", " $1"));
    }

    //public static string Split
    public static string RandomString(int charCount = 8) {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";// 0123456789";
        var stringChars = new char[charCount];
        var random = new System.Random();

        for (int i = 0; i < stringChars.Length; i++) {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

    public static GameObject FindOrCreateByName(string name, Type[] components = null) {
        GameObject retn = GameObject.Find(name);
        if (retn == null) {
            retn = new GameObject(name);
        }
        if (components != null) {
            for (int i = 0; i < components.Length; i++) {
                Type componentType = components[i];
                if (componentType == null) continue;
                if (componentType.IsSubclassOf(typeof(MonoBehaviour))) {
                    if (retn.GetComponent(componentType) == null) {
                        retn.AddComponent(componentType);
                    }
                }
            }
        }
        return retn;
    }

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

    public static float WrapAngle180Offset(float angle, float offset) {
        if (angle > 180f + offset) {
            return -(360f - angle);
        }
        else if (angle < -180f + offset) {
            return angle + 360f;
        }
        else if (angle == -180 + offset) {
            return 180f + offset;
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

public static class TransformExtensions {

    public static void Reset(this Transform t) {
        t.rotation = Quaternion.identity;
        t.position = Vector3.zero;
        t.localScale = Vector3.one;
    }

    public static void ResetLocal(this Transform t) {
        t.localRotation = Quaternion.identity;
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;
    }

    public static float DistanceTo(this Transform self, Transform other) {
        return (other.position - self.position).magnitude;
    }

    public static float DistanceToSquared(this Transform self, Transform other) {
        return (other.position - self.position).sqrMagnitude;
    }

    public static float DistanceToSquared(this Transform self, Vector3 other) {
        return (other - self.position).sqrMagnitude;
    }
}


public static class DictionaryExtensions {

    public static U Get<T, U>(this Dictionary<T, U> dict, T key) {
        U val;
        dict.TryGetValue(key, out val);
        return val;
    }

    public static U Get<T, U>(this Dictionary<T, U> dict, T key, U defaultValue) {
        U val;
        if (!dict.TryGetValue(key, out val)) {
            val = defaultValue;
        }
        return val;
    }

    public static U AddAndReturn<T, U>(this Dictionary<T, U> dict, T key, U value) {
        dict.Add(key, value);
        return value;
    }

}

public static class ListExtensions {

    public static T First<T>(this List<T> list) {
        return list.Count > 0 ? list[0] : default(T);
    }

    public static T Last<T>(this List<T> list) {
        return list.Count > 0 ? list[list.Count - 1] : default(T);
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

    public static float DistanceTo(this Vector3 self, Vector3 other) {
        return (other - self).magnitude;
    }

    public static float DistanceToSquared(this Vector3 self, Vector3 other) {
        return (other - self).sqrMagnitude;
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
