using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public static class SerializerUtil {

    private static Dictionary<Type, TypeSerializer> TypeSerializers;

    static SerializerUtil() {
        TypeSerializers = new Dictionary<Type, TypeSerializer>();
        List<Type> typeSerializerTypes = Reflector.FindSubClassesWithAttribute<TypeSerializer, TypeSerializerAttribute>(true);
        for (int i = 0; i < typeSerializerTypes.Count; i++) {
            Type typeSerializerType = typeSerializerTypes[i];
            TypeSerializer serializer = Activator.CreateInstance(typeSerializerType) as TypeSerializer;
            TypeSerializerAttribute[] attrs = typeSerializerType.GetCustomAttributes(typeof(TypeSerializerAttribute), true) as TypeSerializerAttribute[];
            for (int j = 0; j < attrs.Length; j++) {
                TypeSerializers[attrs[j].type] = serializer;
            }
        }
    }

    public static string GetCreationType(Type type, object target) {
        return "new";
    }

    public static string GetUUID(Type type, GameObject target) {
        return "-1";
    }

    public static TypeSerializer GetTypeSerializer(Type type) {
        TypeSerializer serializer = TypeSerializers.Get(type);
        if (serializer != null) {
            return serializer;
        }
        else if (type.IsGenericType && type != type.GetGenericTypeDefinition()) {
            serializer = GetTypeSerializer(type.GetGenericTypeDefinition());
            if (serializer == null) {
                return GetTypeSerializer(type.BaseType);
            }
            return serializer;
        }
        else {
            return GetTypeSerializer(type.BaseType);
        }
    }

    public static string GetTypeName(Type type) {
        if(type == null) {
            return "NULL";
        }
        if (type.IsGenericType) {
            Type[] argTypes = type.GetGenericArguments();
            StringBuilder builder = new StringBuilder(10);
            string typeName = type.Name;
            int tickIdx = typeName.IndexOf('`');
            if (tickIdx != -1) {
                typeName = typeName.Substring(0, tickIdx);
            }
            builder.Append(typeName);
            builder.Append("<");
            for (int i = 0; i < argTypes.Length; i++) {
                builder.Append(GetTypeName(argTypes[i]));
                if (i != argTypes.Length - 1) {
                    builder.Append(',');
                }
            }
            builder.Append(">");
            return builder.ToString();
        }
       
        return type.Name;
    }

    public static string GetTypeName(object value) {
        if(value == null) {
            return "NULL";
        }
        return GetTypeName(value.GetType());
    }

    public static string EscapeString(string str) {
        StringBuilder builder = new StringBuilder(str.Length);

        char[] charArray = str.ToCharArray();
        foreach (var c in charArray) {
            switch (c) {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126)) {
                        builder.Append(c);
                    }
                    else {
                        builder.Append("\\u");
                        builder.Append(codepoint.ToString("x4"));
                    }
                    break;
            }
        }

        return builder.ToString();
    }

}