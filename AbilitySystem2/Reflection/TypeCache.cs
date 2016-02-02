using System;
using System.Collections.Generic;

public static class TypeCache {
    private static readonly Dictionary<string, Type> _typeCache
        = new Dictionary<string, Type>();

    public static Type GetType(string typeName) {
        Type type = null;
        if (_typeCache.ContainsKey(typeName)) {
            return _typeCache[typeName];
        }

        type = Type.GetType(typeName);
        if (type != null) {
            _typeCache.Add(typeName, type);
        }
        return type;
    }

    public static Type[] StringsToTypes(string[] typeStrings) {
        if (typeStrings == null || typeStrings.Length == 0) {
            return new Type[0];
        }
        Type[] types = new Type[typeStrings.Length];
        for (int i = 0; i < typeStrings.Length; i++) {
            types[i] = GetType(typeStrings[i]);
        }
        return types;
    }
}