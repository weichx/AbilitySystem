using UnityEngine;
using System;
using System.Reflection;

namespace AbilitySystem {
    public abstract class EditorCallableAttribute : PropertyAttribute {
        public string fnName;

        private Delegate displayFn;
        private bool? resultOverride;

        public EditorCallableAttribute(string displayFnName, int order) {
            fnName = displayFnName;
            resultOverride = null;
        }

        public EditorCallableAttribute(bool resultOverride, int order) {
            this.resultOverride = resultOverride;
        }

        public bool Result(object target) {
            if (resultOverride != null) return (bool)resultOverride;
            if (displayFn == null) {
                displayFn = GetDelegate(target, fnName);
            }
            if (displayFn == null) {
                Debug.Log("FUCK");
                return false;
            }
            else {
                return (bool)displayFn.DynamicInvoke(target);
            }
        }

        public static Delegate GetDelegate(object target, string fnName) {

            if (target == null) {
                Debug.LogError("Target cannot be null");
                return null;
            }

            string extensionTypeName = target.GetType().AssemblyQualifiedName;// + "EditorExtensions";
            int saftey = 0;
            Type declType = target.GetType();
            MethodInfo info = declType.GetMethod(fnName);
            if (info == null) {
                while (saftey < 10) {
                    declType = declType.BaseType;
                    if (declType == null) return null;
                    info = declType.GetMethod(fnName);
                    if (info != null) break;
                    saftey++;
                }
            }

            Type type = TypeCache.GetType(extensionTypeName);
            if (declType == null) {
                Debug.LogError("Cannot find type: " + extensionTypeName);
                return null;
            }

            if (info == null) {
                Debug.LogWarning("Unable to find a static method on " + type +
                    " with signature `bool " + fnName + "(" + type.Name + ")`");
                return null;
            }

            return Reflector.CreateDelegate(info);
        }

    }
}