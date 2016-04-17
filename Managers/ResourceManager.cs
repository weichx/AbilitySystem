using UnityEngine;

public class ResourcePool {

    //todo pool stuff
    public static GameObject Spawn(string prefabUrl, Vector3 position, Quaternion rotation) {
        GameObject spawned = Resources.Load(prefabUrl) as GameObject;
        return Object.Instantiate(spawned, position, rotation) as GameObject;
    }

    public static GameObject Spawn(string prefabUrl, Vector3 position) {
        GameObject spawned = Resources.Load(prefabUrl) as GameObject;
        return Object.Instantiate(spawned, position, Quaternion.identity) as GameObject;
    }

    public static GameObject Spawn(string prefabUrl) {
        GameObject spawned = Resources.Load(prefabUrl) as GameObject;
        return Object.Instantiate(spawned, Vector3.zero, Quaternion.identity) as GameObject;
    }
}