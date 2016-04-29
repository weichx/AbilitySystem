using UnityEditor;
using UnityEngine;

public class CacheTools : ScriptableObject {

    [MenuItem("Tools/Cache/Clean")]
    public static void CleanCache() {
        if (Caching.CleanCache()) {
            Debug.LogWarning("Successfully cleaned all caches.");
        }
        else {
            Debug.LogWarning("Cache was in use.");
        }
    }
}