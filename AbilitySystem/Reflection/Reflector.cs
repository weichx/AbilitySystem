using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public static class Reflector {

    private static readonly List<Assembly> filteredAssemblies;
    private static List<MethodInfo> methodSet;
    private static Dictionary<string, Delegate> pointerLookup;

    //Should be able to look up a method by signature
    //Should only create 1 delegate per method pointer
    //Should be able to enumerate all methods with signature/attribute

    static Reflector() {
        pointerLookup = new Dictionary<string, Delegate>();
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

    //todo probably add an attribute requirement as well to narrow search space even more
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

    //public static Action<T> FindActionWithAttribute<T>(Type attrType, string typeName, string methodName, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T>;
    //}

    //public static Action<T, U> FindActionWithAttribute<T, U>(Type attrType, string typeName, string methodName, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T, U>;
    //}

    //public static Action<T, U, V> FindActionWithAttribute<T, U, V>(Type attrType, string typeName, string methodName, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, null, parameters) as Action<T, U, V>;
    //}

    //public static Func<T> FindFuncWithAttribute<T>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T>;
    //}

    //public static Func<T, U> FindFuncWithAttribute<T, U>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T, U>;
    //}

    //public static Func<T, U, V> FindFuncWithAttribute<T, U, V>(Type attrType, string typeName, string methodName, Type retnType, params Type[] parameters) {
    //    return FindMethodWithAttribute(attrType, typeName, methodName, retnType, parameters) as Func<T, U, V>;
    //}

    public static Delegate FindMethodForPointer(MethodPointer pointer) {
        var signature = pointer.ToString();
        var fn = pointerLookup.Get(signature);
        if (fn != null) return fn;
        Type type = TypeCache.GetType(pointer.type);
        Type retnType = TypeCache.GetType(pointer.retnType);
        Type[] parameters = TypeCache.StringsToTypes(pointer.argumentTypes);
        return FindMethod(type, pointer.method, retnType, parameters);
    }

    public static List<MethodPointer> FindMethodPointersWithAttribute<T>(Type retnType, params Type[] parameterTypes) where T : Attribute {
        var list = new List<MethodPointer>();
        var attrType = typeof(T);
        for (int i = 0; i < methodSet.Count; i++) {
            MethodInfo methodInfo = methodSet[i];
            if (HasAttribute(methodInfo, attrType) && MatchesSignature(methodInfo, retnType, parameterTypes)) {
                list.Add(new MethodPointer(methodInfo));
            }
        }
        return list;
    }

    private static Delegate FindMethod(Type type, string methodName, Type retnType = null, params Type[] arguements) {
        if (retnType == null) retnType = typeof(void);
        //todo not sure if its faster to user cached method types..probably is
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < methods.Length; i++) {
            var method = methods[i];
            if (method.Name == methodName && MatchesSignature(method, retnType, arguements)) {
                return CreateDelegate(method);
            }
        }
        return null;
    }

    #region private methods
    private static bool MatchesSignature(MethodInfo methodInfo, Type retnType, Type[] parameters = null) {
        if (retnType == null) retnType = typeof(void);
        if (methodInfo.ReturnType != retnType) return false;
        if (parameters == null) return true;
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

    private static Delegate CreateDelegate(MethodInfo method) {
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
    #endregion
}