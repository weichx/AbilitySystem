using UnityEngine;

namespace AbilitySystem {
    public class PlayerManager {

        public readonly static Entity playerEntity;
        public readonly static GameObject playerRoot;
        public readonly static Transform playerTransform;

        public static Entity target;

        static PlayerManager() {
            playerRoot = GameObject.FindGameObjectWithTag("Player");
            playerEntity = playerRoot.GetComponent<Entity>();
            playerTransform = playerRoot.transform;
        }
    }
}