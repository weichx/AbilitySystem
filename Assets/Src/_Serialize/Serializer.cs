using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class Serializer {

    protected static BindingFlags FieldBindFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
    protected static BindingFlags PropertyBindFlags = FieldBindFlags | BindingFlags.GetProperty | BindingFlags.SetProperty;

    protected const string ValueSymbol = ":";
    protected const string ObjectOpenSymbol = "{";
    protected const string ObjectCloseSymbol = "}";
    protected const string ListOpenSymbol = "[";
    protected const string ListCloseSymbol = "]";

    protected List<object> toSerialize;
    protected Dictionary<object, bool> serializedMap;
    protected Dictionary<object, int> refMap;

    protected TypeSerializer<object> defaultSerializer;
    protected StringBuilder header; //todo replace with abstract writer
    protected StringBuilder body;   //todo replace with abstract writer
    protected int refId;

    public static Dictionary<Type, object> serializerLookup;

    public Serializer(params object[] inputs) {
        toSerialize = new List<object>(inputs);
        serializedMap = new Dictionary<object, bool>();
        refMap = new Dictionary<object, int>();
        header = new StringBuilder(100);
        body = new StringBuilder(500);
        refId = 0;
        header.AppendLine("Refs");
        body.AppendLine("Data");
    }

    public void AddItem<T>(T obj) where T : class, new() {
        if (!serializedMap.ContainsKey(obj) && !toSerialize.Contains(obj)) {
            toSerialize.Add(obj);
        }
    }

    public void Write(string filePath) {
        File.WriteAllText(filePath, Write());
    }

    public string Write() {
        for (int i = 0; i < toSerialize.Count; i++) {
            if (serializedMap.ContainsKey(toSerialize[i])) {
                continue;
            }
            Serialize(toSerialize[i]);
        }
        header.AppendLine();
        return header.ToString() + body.ToString();
    }

    public void WriteDefault(object obj) {
        Type type = obj.GetType();
        FieldInfo[] fieldInfoList = type.GetFields(FieldBindFlags);
        WriteFields(obj, fieldInfoList);
    }

    private void WriteFields(object origin, FieldInfo[] fields) {
        for (int i = 0; i < fields.Length; i++) {
            FieldInfo fInfo = fields[i];
            if (fInfo.IsNotSerialized) {
                continue;
            }

            Type fieldType = fInfo.FieldType;
            object val = fInfo.GetValue(origin);

            body.Append(fInfo.Name);
            body.Append(" F ");
            body.Append(GetTypeName(fieldType));

            DoWrite(fieldType, val, origin);
        }
    }

    private void DoWrite(Type type, object val, object origin) {
        if (val == null) {
            body.Append(ValueSymbol);
            body.AppendLine("null");
        }
        else if (type.IsPrimitive) {
            body.Append(ValueSymbol);
            body.AppendLine(val.ToString());
        }
        else if (type == typeof(string)) {
            body.Append(ValueSymbol);
            body.AppendLine(val as string);
        }
        else if (type.IsEnum) { //todo this is a dummy
            body.Append(ValueSymbol);
            body.AppendLine("1");
        }
        else if (type.IsArray) {   //todo this is a dummy
            body.Append(ListOpenSymbol);
            body.AppendLine(ListCloseSymbol);
        }
        else if (type.IsValueType) {
            body.Append(ObjectOpenSymbol);
            GetSerializer(type).Serialize(val, null);
            body.AppendLine(ObjectCloseSymbol);
        }
        else if (type.IsClass) {
            //an id of -1 == null
            int id = GetRefId(val);
            if (id != -1 && !serializedMap.ContainsKey(id) && !toSerialize.Contains(val)) {
                toSerialize.Add(val);
            }
            body.Append(ValueSymbol);
            body.AppendLine(id.ToString());
        }
    }

    private void Serialize(object obj) {
        if (obj == null) return; //todo -- this is wrong
        Type type = obj.GetType();

        WriteHeader(type, obj);
        body.AppendLine(GetRefId(obj).ToString());
        GetSerializer(type).Serialize(obj, null);

    }

    private void WriteHeader(Type type, object obj) {
        if (obj != null && !serializedMap.ContainsKey(obj)) {
            header.Append(GetRefId(obj).ToString());
            header.Append(GetReferenceSymbol(type, obj));
            header.Append(GetTypeName(obj.GetType()));
        }
    }

    private string GetReferenceSymbol(Type type, object obj) {
        if (type == typeof(Transform)) {
            return " T (" + GetUniqueGameObjectId(obj) + ") ";
        }
        else if (type.IsSubclassOf(typeof(Component)) || typeof(Component) == type) {
            return " C (" + GetUniqueGameObjectId(obj) + ") ";
        }
        else if (type == typeof(GameObject)) { //todo check for prefab
            return " G ";
        }
        //else do binary case
        return " R ";
    }

    private string GetUniqueGameObjectId(object obj) {
        var go = (obj as Component).gameObject;
        UniqueIdComponent saveComponent = go.GetComponent<UniqueIdComponent>();
        if (saveComponent == null) {
            saveComponent = go.AddComponent<UniqueIdComponent>();
            saveComponent.OnAfterDeserialize();
        }
        if (go.GetComponent<UniqueIdComponent>() == null) {
            go.GetComponent<UniqueIdComponent>().OnAfterDeserialize();
        }
        return saveComponent.UniqueId;
    }

    private TypeSerializer<object> GetSerializer(Type type) {
        var attrs = type.GetCustomAttributes(false);
        for(int i = 0; i < attrs.Length; i++) {
            //if(attrs[i].GetType() == ty)
        }
        return defaultSerializer;
    }

    private string GetTypeName(Type type) {
        if (type.IsGenericType) {
            Type[] argTypes = type.GetGenericArguments();
            StringBuilder builder = new StringBuilder(10);
            builder.Append(type.Name);
            builder.Append("<");
            for (int i = 0; i < argTypes.Length; i++) {
                builder.Append(GetTypeName(argTypes[i]));
            }
            builder.Append("> ");
            return builder.ToString();
        }
        return type.Name;
    }

    private int GetRefId(object obj) {
        if (obj == null) return -1;
        int id;
        if (refMap.TryGetValue(obj, out id)) {
            return id;
        }
        id = refId++;
        refMap[obj] = id;
        return id;
    }

    public string NextLine() {
        //todo build out tabs here based on depth
        return "\n";
    }

    //todo handle UnityEngine.Object type, unsure how that goes
    private bool IsUnityType(Type type) {
        return type == typeof(GameObject) || type.IsSubclassOf(typeof(Component)) || type == typeof(Component);
    }

}

/*
Type = Root
RootId 

*/