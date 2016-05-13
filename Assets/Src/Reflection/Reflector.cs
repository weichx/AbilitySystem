using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


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
                for (int methodIdx = 0; methodIdx < methods.Length; methodIdx++) {
                    MethodInfo info = methods[methodIdx];
                    if (info.GetCustomAttributes(true).Length > 0 && !info.IsDefined(typeof(ExtensionAttribute), true) && !info.IsGenericMethod && !info.IsGenericMethodDefinition) {
                        pointerLookup[MethodPointerUtils.CreateSignature(info)] = CreateDelegate(info);
                    }
                    methodSet.AddRange(methods);
                }
            }
        }
    }

    public static Delegate FindDelegateWithSignature(string signature) {
        Delegate fn;
        if (signature == null) return null;
        if (pointerLookup.TryGetValue(signature, out fn)) {
            return fn;
        }
        else {
            return null;// throw new Exception("Method not found with signature: " + signature); d
        }
    }

    public static FieldInfo GetProperty(object obj, string property) {
        return obj.GetType().GetField(property);
    }

    public static List<MethodPointer> FindMethodPointersWithAttribute(Type attrType, Type retnType, params Type[] parameterTypes) {
        var list = new List<MethodPointer>();
        for (int i = 0; i < methodSet.Count; i++) {
            MethodInfo methodInfo = methodSet[i];
            if (HasAttribute(methodInfo, attrType) && MatchesSignature(methodInfo, retnType, parameterTypes)) {
                list.Add(new MethodPointer(methodInfo));
            }
        }
        return list;
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

    public static List<MethodPointer> FindMethodPointersWithAttribute<T>(SignatureAttribute signatureAttr) where T : Attribute {
        var list = new List<MethodPointer>();
        var attrType = typeof(T);
        for (int i = 0; i < methodSet.Count; i++) {
            MethodInfo methodInfo = methodSet[i];
            if (HasAttribute(methodInfo, attrType) && MatchesSignature(methodInfo, signatureAttr.retnType, signatureAttr.parameterTypes)) {
                list.Add(new MethodPointer(methodInfo));
            }
        }
        return list;
    }

    public static List<Type> FindSubClasses<T>(bool includeInputType = false) {
        var retn = new List<Type>();
        var type = typeof(T);
        for (int i = 0; i < filteredAssemblies.Count; i++) {
            var assembly = filteredAssemblies[i];
            var types = assembly.GetTypes();
            for (int t = 0; t < types.Length; t++) {
                if (types[t].IsGenericTypeDefinition) {
                    continue;
                }
                if (types[t].IsSubclassOf(type) || includeInputType && types[t] == type) {
                    retn.Add(types[t]);
                }
            }
        }
        return retn;
    }

    public static List<Type> FindSubClassesWithAttribute<T, U>(bool includeInputType = false) where T : class where U : Attribute {
        return FindSubClasses<T>(includeInputType).FindAll((type) => {
            return type.GetCustomAttributes(typeof(U), false).Length > 0;
        });
    }

    private static Delegate FindMethod(Type type, string methodName, Type retnType = null, params Type[] arguements) {
        if (retnType == null) retnType = typeof(void);
        //todo not sure if its faster to user cached method types..probably is
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < methods.Length; i++) {
            var method = methods[i];
            if (method.Name == methodName && MatchesSignature(method, retnType, arguements)) {
                return null;// CreateDelegate(method);
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

    public static Delegate CreateDelegate(MethodInfo method) {
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

#if UNITY_EDITOR
    private static List<Meta> customPropertyDrawerTypes;
    private static Dictionary<Type, UnityEditor.PropertyDrawer> drawerCache;

    private struct Meta {
        public Type attributeArgumentType;
        public Type attributeDrawerType;

        public Meta(Type attributeDrawerType, Type attributeArgumentType) {
            this.attributeDrawerType = attributeDrawerType;
            this.attributeArgumentType = attributeArgumentType;
        }
    }

    private static List<Meta> GetPropertyDrawerTypes(Assembly assembly) {
        if (customPropertyDrawerTypes != null) return customPropertyDrawerTypes;

        customPropertyDrawerTypes = new List<Meta>();
        BindingFlags bindFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        Type[] assemblyTypes = assembly.GetTypes();

        for (int i = 0; i < assemblyTypes.Length; i++) {
            object[] attributes = assemblyTypes[i].GetCustomAttributes(typeof(UnityEditor.CustomPropertyDrawer), true);
            if (attributes.Length > 0) {
                FieldInfo m_TypeFieldInfo = attributes[0].GetType().GetField("m_Type", bindFlags);
                Type m_Type = m_TypeFieldInfo.GetValue(attributes[0]) as Type;
                customPropertyDrawerTypes.Add(new Meta(assemblyTypes[i], m_Type));
            }
        }
        return customPropertyDrawerTypes;
    }

    public static UnityEditor.PropertyDrawer GetCustomPropertyDrawerFor(Type type, params Assembly[] assemblies) {
        if (drawerCache == null) {
            drawerCache = new Dictionary<Type, UnityEditor.PropertyDrawer>();
        }
        UnityEditor.PropertyDrawer drawer;
        if (drawerCache.TryGetValue(type, out drawer)) {
            return drawer;
        }

        if (type == typeof(UnityEngine.RangeAttribute)) {

            drawer = Activator.CreateInstance(typeof(UnityEditor.EditorGUI).Assembly.GetType("UnityEditor.RangeDrawer")) as UnityEditor.PropertyDrawer;
            drawerCache[type] = drawer;
            return drawer;
        }

        for (int a = 0; a < assemblies.Length; a++) {
            List<Meta> metaList = GetPropertyDrawerTypes(assemblies[a]);
            for (int i = 0; i < metaList.Count; i++) {
                Meta drawerMeta = metaList[i];
                Type attrArgument = drawerMeta.attributeArgumentType;
                if (type == attrArgument || type.IsSubclassOf(attrArgument)) {
                    drawer = Activator.CreateInstance(drawerMeta.attributeDrawerType) as UnityEditor.PropertyDrawer;
                    drawerCache[type] = drawer;
                    return drawer;
                }
            }
        }
        return null;
    }
#endif
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

