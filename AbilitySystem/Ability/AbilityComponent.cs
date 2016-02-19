using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
#endif

namespace AbilitySystem {

    public abstract class AbilitySystemComponent : MonoBehaviour {

#if UNITY_EDITOR

        private class AbilityComponentException : Exception {
            public AbilityComponentException(string componentName) : base(componentName + " should never be called on an Ability Component instance.") { }
        }

        private static string[] methodBlackList = {
            "Update", "Start", "Awake", "OnEnable", "OnDisable"
        };

        private static Dictionary<Type, bool> checkMap = new Dictionary<Type, bool>();

        public AbilitySystemComponent() {
            Type type = GetType();
            if (!checkMap.ContainsKey(type)) {
                checkMap[type] = true;
                for (int i = 0; i < methodBlackList.Length; i++) {
                    var methodName = methodBlackList[i];
                    if (type.GetMethod(methodName) != null && type != typeof(Ability)) {
                        throw new Exception("You cannot implement `" + methodName + "` in AbilitySystem Components! " + type.Namespace + "." + type.Name + " implements " + methodName);
                    }
                }
            }
        }

        public new GameObject gameObject {
            get {
                if (GetType() == typeof(Ability)) {
                    return base.gameObject;
                }
                else {
                    throw new Exception("AbilityComponent gameObject should never be accessed");
                }
            }
        }

        public new Transform transform {
            get {
                if (GetType() == typeof(Ability)) {
                    return base.transform;
                }
                else {
                    throw new Exception("AbilityComponent transform should never be accessed");
                }
            }
        }

        public new Rigidbody rigidbody {
            get { throw new Exception("AbilityComponent rigidbody should never be accessed"); }
        }

        public new Rigidbody2D rigidbody2D {
            get { throw new Exception("AbilityComponent rigidbody2D should never be accessed"); }
        }

        public new Camera camera {
            get { throw new Exception("AbilityComponent camera should never be accessed"); }
        }

        public new Light light {
            get { throw new Exception("AbilityComponent light should never be accessed"); }
        }

        public new Animation animation {
            get { throw new Exception("AbilityComponent animation should never be accessed"); }
        }

        public new ConstantForce constantForce {
            get { throw new Exception("AbilityComponent constantForce should never be accessed"); }
        }

        public new Renderer renderer {
            get { throw new Exception("AbilityComponent renderer should never be accessed"); }
        }

        public new AudioSource audio {
            get { throw new Exception("AbilityComponent audio should never be accessed"); }
        }

        public new GUIText guiText {
            get { throw new Exception("AbilityComponent guiText should never be accessed"); }
        }

        public new NetworkView networkView {
            get { throw new Exception("AbilityComponent networkView should never be accessed"); }
        }

        public new GUIElement guiElement {
            get { throw new Exception("AbilityComponent guiElement should never be accessed"); }
        }

        public new GUITexture guiTexture {
            get { throw new Exception("AbilityComponent guiTexture should never be accessed"); }
        }

        public new Collider collider {
            get { throw new Exception("AbilityComponent collider should never be accessed"); }
        }

        public new Collider2D collider2D {
            get { throw new Exception("AbilityComponent collider2D should never be accessed"); }
        }

        public new HingeJoint hingeJoint {
            get { throw new Exception("AbilityComponent hingeJoint should never be accessed"); }
        }

        public new ParticleEmitter particleEmitter {
            get { throw new Exception("AbilityComponent particleEmitter should never be accessed"); }
        }

        public new ParticleSystem particleSystem {
            get { throw new Exception("AbilityComponent particleSystem should never be accessed"); }
        }

        public new Component GetComponent(Type type) {
            throw new AbilityComponentException("GetComponent");
        }

        public new T GetComponent<T>() {
            if (GetType() == typeof(Ability)) return base.GetComponent<T>();
            throw new AbilityComponentException("GetComponent");
        }

        public new Component GetComponent(string type) {
            throw new AbilityComponentException("GetComponent");
        }

        public new Component GetComponentInChildren(Type t) {
            throw new AbilityComponentException("GetComponentInChildren");
        }

        public new T GetComponentInChildren<T>() {
            throw new AbilityComponentException("GetComponentInChildren");
        }

        public new Component[] GetComponentsInChildren(Type t) {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new Component[] GetComponentsInChildren(Type t, bool includeInactive = false) {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new T[] GetComponentsInChildren<T>(bool includeInactive) {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new void GetComponentsInChildren<T>(bool includeInactive, List<T> result) {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new T[] GetComponentsInChildren<T>() {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new void GetComponentsInChildren<T>(List<T> results) {
            throw new AbilityComponentException("GetComponentsInChildren");
        }

        public new Component GetComponentInParent(Type t) {
            throw new AbilityComponentException("GetComponentInParent");
        }

        public new T GetComponentInParent<T>() {
            throw new AbilityComponentException("GetComponentInParent");
        }

        public new Component[] GetComponentsInParent(Type t) {
            throw new AbilityComponentException("GetComponentsInParent");
        }

        public new Component[] GetComponentsInParent(Type t, bool includeInactive = false) {
            throw new AbilityComponentException("GetComponentsInParent");
        }

        public new T[] GetComponentsInParent<T>(bool includeInactive) {
            throw new AbilityComponentException("GetComponentsInParent");
        }

        public new T[] GetComponentsInParent<T>() {
            throw new AbilityComponentException("GetComponentsInParent");
        }

        public new Component[] GetComponents(Type type) {
            throw new AbilityComponentException("GetComponents");
        }

        public new T[] GetComponents<T>()  {
            if (GetType() == typeof(Ability)) return base.GetComponents<T>();
            throw new AbilityComponentException("GetComponents");
        }

        public new void GetComponents(Type type, List<Component> results) {
            throw new AbilityComponentException("GetComponents");
        }

        public new void GetComponents<T>(List<T> results)  {
            throw new AbilityComponentException("GetComponents");
        }

        public new void SendMessageUpwards(string methodName, object value = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) {
            throw new AbilityComponentException("SendMessageUpwards");
        }

        public new void SendMessageUpwards(string methodName, object value) {
            throw new AbilityComponentException("SendMessageUpwards");
        }

        public new void SendMessageUpwards(string methodName) {
            throw new AbilityComponentException("SendMessageUpwards");
        }

        public new void SendMessageUpwards(string methodName, SendMessageOptions options) {
            throw new AbilityComponentException("SendMessageUpwards");
        }

        public new void SendMessage(string methodName, object value = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) {
            throw new AbilityComponentException("SendMessage");
        }

        public new void SendMessage(string methodName, object value) {
            throw new AbilityComponentException("SendMessage");
        }

        public new void SendMessage(string methodName) {
            throw new AbilityComponentException("SendMessage");
        }

        public new void SendMessage(string methodName, SendMessageOptions options) {
            throw new AbilityComponentException("SendMessage");
        }

        public new void BroadcastMessage(string methodName, object parameter = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) {
            throw new AbilityComponentException("BroadcastMessage");
        }

        public new void BroadcastMessage(string methodName, object parameter) {
            throw new AbilityComponentException("BroadcastMessage");
        }

        public new void BroadcastMessage(string methodName) {
            throw new AbilityComponentException("BroadcastMessage");
        }

        public new void BroadcastMessage(string methodName, SendMessageOptions options) {
            throw new AbilityComponentException("BroadcastMessage");
        }
    }
#endif
}