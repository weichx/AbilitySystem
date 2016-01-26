using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

public static class Reflector {

    private static readonly List<Assembly> filteredAssemblies;
    private static List<MethodInfo> methodSet;

    static Reflector() {
        methodSet = new List<MethodInfo>();
        filteredAssemblies = new List<Assembly>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++) {
            var assembly = assemblies[i];
            if (FilterAssembly(assembly)) {
                filteredAssemblies.Add(assembly);
            }
        }
        FindPublicStaticMethods();
    }

    private static void FindPublicStaticMethods() {
        methodSet = new List<MethodInfo>();
        for (int i = 0; i < filteredAssemblies.Count; i++) {
            var types = filteredAssemblies[i].GetTypes();
            for (int typeIndex = 0; typeIndex < types.Length; typeIndex++) {
                var methods = types[typeIndex].GetMethods(BindingFlags.Static | BindingFlags.Public);
                methodSet.AddRange(methods);
            }
        }
    }

    public static Action<T> FindActionWithAttribute<T>(Type attrType, string typeName, string methodName, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T>;
    }

    public static Action<T, U> FindActionWithAttribute<T, U>(Type attrType, string typeName, string methodName, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T, U>;
    }

    public static Action<T, U, V> FindActionWithAttribute<T, U, V>(Type attrType, string typeName, string methodName, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T, U, V>;
    }

    public static Func<T> FindFuncWithAttribute<T>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T>;
    }

    public static Func<T, U> FindFuncWithAttribute<T, U>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T, U>;
    }

    public static Func<T, U, V> FindFuncWithAttribute<T, U, V>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
        return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T, U, V>;
    }

    public static Delegate FindMethodWithAttribute(Type attrType, string typeName, string methodName, Type retnType = null, params Type[] parameters) {
        if (retnType == null) retnType = typeof(void);
        for (int i = 0; i < methodSet.Count; i++) {
            var method = methodSet[i];
            if (method.DeclaringType.Name == typeName && method.Name == methodName) {
                if (HasAttribute(method, attrType) && MatchesSignature(method, retnType, parameters)) {
                    return CreateDelegate(method);
                }
            }
        }
        return null;
    }

    public static List<MethodPointer> FindMethodPointersWithAttribute(Type attrType, Type retnType = null, params Type[] parameters) {
        var list = new List<MethodPointer>();
        for (int i = 0; i < methodSet.Count; i++) {
            var method = methodSet[i];
            if (HasAttribute(method, attrType) && MatchesSignature(method, retnType, parameters)) {
                list.Add(new MethodPointer(method.DeclaringType.Name, method.Name));
            }
        }
        return list;
    }

    private static bool MatchesSignature(MethodInfo methodInfo, Type retnType, Type[] parameters) {
        if (retnType == null) retnType = typeof(void);
        if (methodInfo.ReturnType != retnType) return false;
        var methodParameters = methodInfo.GetParameters();
        if (methodParameters.Length != parameters.Length) return false;
        for (int i = 0; i < methodParameters.Length; i++) {
            if (methodParameters[i].ParameterType != parameters[i]) {
                return false;
            }
        }
        return true;
    }

    private static bool HasAttribute(MethodInfo methodInfo, Type attrType) {
        var attrs = methodInfo.GetCustomAttributes(attrType, false);
        return (attrs != null && attrs.Length > 0);
    }

    static Delegate CreateDelegate(MethodInfo method) {
        List<Type> args = new List<Type>(method.GetParameters().Select(p => p.ParameterType));
        Type delegateType;
        if (method.ReturnType == typeof(void)) {
            delegateType = Expression.GetActionType(args.ToArray());
        }
        else {
            args.Add(method.ReturnType);
            delegateType = Expression.GetFuncType(args.ToArray());
        }
        return Delegate.CreateDelegate(delegateType, null, method);
    }

    [Serializable]
    public class MethodPointer {
        public string type;
        public string method;

        public MethodPointer(string type, string method) {
            this.type = type;
            this.method = method;
        }
    }

    private static bool FilterAssembly(Assembly assembly) {
        return assembly.ManifestModule.Name != "<In Memory Module>"
        && !assembly.FullName.StartsWith("System")
        && !assembly.FullName.StartsWith("Mono")
        && !assembly.FullName.StartsWith("Syntax")
        && !assembly.FullName.StartsWith("I18N")
        && !assembly.FullName.StartsWith("Boo")
        && !assembly.FullName.StartsWith("mscorlib")
        && !assembly.FullName.StartsWith("nunit")
        && !assembly.FullName.StartsWith("ICSharp")
        && !assembly.FullName.StartsWith("Unity")
        && !assembly.FullName.StartsWith("Microsoft")
        && assembly.FullName.IndexOf("CSharp-Editor") == -1
        && assembly.Location.IndexOf("App_Web") == -1
        && assembly.Location.IndexOf("App_global") == -1
        && assembly.FullName.IndexOf("CppCodeProvider") == -1
        && assembly.FullName.IndexOf("WebMatrix") == -1
        && assembly.FullName.IndexOf("SMDiagnostics") == -1
        && !string.IsNullOrEmpty(assembly.Location);
    }
}