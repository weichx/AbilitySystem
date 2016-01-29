using System;
using System.Reflection;
using UnityEngine;

public class DecomposedMethodSignature {
    
}

[Serializable]
public class MethodPointer {
    public string type;
    public string method;
    public string retnType;
    public string[] argumentTypes;

    public string signature;

    public MethodPointer(MethodInfo info) {
        if(info != null) {
            type = info.DeclaringType.Name;
            method = info.Name;
            retnType = info.ReturnType.Name;
            ParameterInfo[] parameters = info.GetParameters();
            argumentTypes = new string[parameters.Length];
            for(int i = 0; i < parameters.Length; i++) {
                argumentTypes[i] = parameters[i].ParameterType.Name;
            }
        }
    }

    public override string ToString() {
        if (signature != null) return signature;
        signature = CreateSignature(type, method, retnType, argumentTypes);
        return signature;
    }

    public static string CreateSignature(string type, string method, string retnType, string[] parameters) {
        string rType = null;
        rType = FilterFloatTypeName(retnType);
        string signature = rType + " " + type + "." + method + "(";
        if (parameters != null) {
            for (int i = 0; i < parameters.Length; i++) {
                signature += FilterFloatTypeName(parameters[i]);
                if (i != parameters.Length - 1) {
                    signature += ", ";
                }
            }
        }
        signature += ")";
        return signature;
    }

    public DecomposedMethodSignature DecomposeSignature() {
        return null;
    }

    public void OnDeserialized() {
        Reflector.FindDelegateWithSignature(signature);
    }

    private static string FilterFloatTypeName(string parameterType) {
        if (parameterType == "Single") return "float";
        return parameterType;
    }
}

public class MethodPointer<T> : MethodPointer, ISerializationCallbackReceiver {

    private Func<T> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) {
        fn = Reflector.FindMethodForPointer(this) as Func<T>;
    }

    public T Invoke() {
        return fn();
    }

    public void OnBeforeSerialize() {
        if (fn == null) return;
        type = fn.Method.DeclaringType.Name;
        method = fn.Method.Name;
        retnType = fn.Method.ReturnType.Name;
        var parameters = fn.Method.GetParameters();
        argumentTypes = new string[parameters.Length];
        for (int i = 0; i < argumentTypes.Length; i++) {
            argumentTypes[i] = parameters[i].ParameterType.Name;
        }
    }

    public void OnAfterDeserialize() {
        fn = Reflector.FindMethodForPointer(this) as Func<T>;
    }
}

public class MethodPointer<T,U> : MethodPointer, ISerializationCallbackReceiver {
    private Func<T, U> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U>;
    }

    public U Invoke(T arg0) {
        return fn(arg0);
    }

    public void OnBeforeSerialize() {
        if (fn == null) return;
        type = fn.Method.DeclaringType.Name;
        method = fn.Method.Name;
        retnType = fn.Method.ReturnType.Name;
        var parameters = fn.Method.GetParameters();
        argumentTypes = new string[parameters.Length];
        for (int i = 0; i < argumentTypes.Length; i++) {
            argumentTypes[i] = parameters[i].ParameterType.Name;
        }
    }

    public void OnAfterDeserialize() {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U>;
    }
}

[Serializable]
public class MethodPointer<T, U, V> : MethodPointer, ISerializationCallbackReceiver {
    private Func<T, U, V> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U, V>;
    }


    public V Invoke(T arg0, U arg1) {
        return fn(arg0, arg1);
    }

    public void OnBeforeSerialize() {
        if (fn == null) return;
        type = fn.Method.DeclaringType.Name;
        method = fn.Method.Name;
        retnType = fn.Method.ReturnType.Name;
        var parameters = fn.Method.GetParameters();
        argumentTypes = new string[parameters.Length];
        for (int i = 0; i < argumentTypes.Length; i++) {
            argumentTypes[i] = parameters[i].ParameterType.Name;
        }
    }

    public void OnAfterDeserialize() {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U, V>;
    }
}

[Serializable]
public class MethodPointer<T, U, V, W> : MethodPointer, ISerializationCallbackReceiver {
    private Func<T, U, V, W> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U, V, W>;
    }

    public W Invoke(T arg0, U arg1, V arg2) {
        return fn(arg0, arg1, arg2);
    }

    public void OnBeforeSerialize() {
        if (fn == null) return;
        type = fn.Method.DeclaringType.Name;
        method = fn.Method.Name;
        retnType = fn.Method.ReturnType.Name;
        var parameters = fn.Method.GetParameters();
        argumentTypes = new string[parameters.Length];
        for(int i = 0; i < argumentTypes.Length; i++) {
            argumentTypes[i] = parameters[i].ParameterType.Name;
        }
    }

    public void OnAfterDeserialize() {
        fn = Reflector.FindMethodForPointer(this) as Func<T, U, V, W>;
    }
}