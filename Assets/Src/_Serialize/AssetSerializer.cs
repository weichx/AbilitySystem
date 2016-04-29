using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

//todo lots of boxing going on here
public class EntityAssetSerializer<T> : IWriter where T : new() {

    protected const string ValueSymbol = ": ";
    protected const string StringSymbol = "\" ";
    protected const string ObjectSymbol = "{ ";
    protected const string ListSymbol = "[ ";
    protected const string EmptyListSymbol = "] ";
    protected const string RawSymbol = "> ";
    protected const string AssetSymbol = "& ";
    protected const string ErrorSymbol = "x ";

    protected static BindingFlags FieldBindFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

    protected List<object> toSerialize;
    protected Dictionary<object, string> serializedMap;
    protected Dictionary<object, int> refMap;
    protected int refId;
    protected T asset;
    protected StringBuilder builder;
    protected object context;
    protected static string[] EmptyStringArray = new string[0];

    public EntityAssetSerializer(T asset) {
        this.asset = asset;
        toSerialize = new List<object>();
        serializedMap = new Dictionary<object, string>();
        refMap = new Dictionary<object, int>();
        builder = new StringBuilder(100);
        refId = 0;
        //compile
        Serialize(asset);

        for (int i = 0; i < toSerialize.Count; i++) {
            if (serializedMap.ContainsKey(toSerialize[i])) {
                continue;
            }
            Serialize(toSerialize[i]);
        }

    }

    public void Write(string path) {
        builder.Length = 0;
        builder.AppendLine("FileType : Asset");
        builder.AppendLine();

        foreach (string val in serializedMap.Values) {
            builder.AppendLine(val);
        }

        File.WriteAllText(path, builder.ToString());
    }

    public void WriteDefault() {
        WriteDefaultExcept(EmptyStringArray);
    }

    public void WriteDefaultExcept(string[] exceptions) {
        FieldInfo[] fields = context.GetType().GetFields(FieldBindFlags);
        for (int i = 0; i < fields.Length; i++) {
            object[] attrs = fields[i].GetCustomAttributes(typeof(SerializeField), true);
            FieldInfo fieldInfo = fields[i];
            if (fieldInfo.IsNotSerialized || Array.IndexOf(exceptions, fieldInfo.Name) != -1) { 
                continue;
            }
            
            WriteField(fieldInfo);
        }
    }

    public void WriteField(FieldInfo fInfo) {
        object value = fInfo.GetValue(context);
        WriteField(fInfo.Name, (value != null) ? value.GetType() : null, value);
    }

    public void WriteField(string fieldId, object value) {
        WriteField(fieldId, (value != null) ? value.GetType() : null, value);
    }
    
    public void WriteField(string fieldId, Type type, object value) {
        //todo asserts around type, value matching
        builder.Append(fieldId);
        builder.Append(" ");
        builder.Append(SerializerUtil.GetTypeName(type));
        builder.Append(" ");
        builder.AppendLine(GetSerializedValue(value));
    }

    protected string GetSerializedValue(object value) {
        if (value == null) {
            return "-1";
        }
        Type type = value.GetType();
        if (type.IsPrimitive) {
            if (value is float) {
                return ValueSymbol + ((float)value).ToString("R");
            }
            else if (type == typeof(double) || type == typeof(decimal)) {
                return ValueSymbol + Convert.ToDouble(value).ToString("R");
            }
            else {
                return ValueSymbol + value.ToString();
            }
        }
        else if (type.IsEnum) {
            return ValueSymbol + ((int)value).ToString();
        }
        else if (type == typeof(string)) {
            return StringSymbol + SerializerUtil.EscapeString(value as string);
        }
        else if (type.IsArray) {
            object[] valArray = value as object[];
            if (valArray.Length == 0) {
                return EmptyListSymbol;
            } else {
                return ListSymbol + EnsureSerialized(value).ToString();
            }
        }
        else if (type.IsValueType) {
            return ObjectSymbol + EnsureSerialized(value).ToString();
        }
        else if (type.IsClass) {
            //todo check if is asset (prefab | texture | scripatable | mesh | etc)
            return ObjectSymbol + EnsureSerialized(value).ToString();
        }
        else {
            return ErrorSymbol + SerializerUtil.GetTypeName(type);
        }
    }

    protected string Serialize(object source) {
        //refId creationType uuid Type version
        Type type = source.GetType();
        TypeSerializer typeSerializer = SerializerUtil.GetTypeSerializer(type);
        context = source;
        string id = GetRefId(source).ToString();
        string creationType = SerializerUtil.GetCreationType(type, source);
        string uuid = SerializerUtil.GetUUID(type, source as GameObject);
        string typeName = SerializerUtil.GetTypeName(type);
        string version = typeSerializer.GetVersion();
        string header = string.Join(" ", new string[] {
            id, creationType, uuid, typeName, version, "\n"
        });
        serializedMap[id] = string.Empty;
        builder.Length = 0;
        typeSerializer.Serialize(source, this);
        string final = header + builder.ToString();
        serializedMap[id] = final;
        return final;
    }

    protected int EnsureSerialized(object value) {
        if (!serializedMap.ContainsKey(value) && !toSerialize.Contains(value)) {
            toSerialize.Add(value);
        }
        return GetRefId(value);
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

    private bool InvalidFieldId(string fieldId) {
        return false;
    }

}

enum CreationType {
    New, Find, GetComponent, AddComponent, LoadAsset
}