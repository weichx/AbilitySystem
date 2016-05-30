using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using System.Text.RegularExpressions;

public class TypeReadResult {
    public string creationMethod;
    public string uuid;
    public string version;
    public string refId;
    public string structId;
    public object instance;
    public Type type;
    public FieldReadResult[] fields;
}

public class FieldReadResult {
    public Type type;
    public string fieldId;
    public string strVal;
    public char symbol;
    public object value;
}

public class AssetDeserializer : IReader {

    protected static BindingFlags FieldBindFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
    protected static char[] SpaceArray = new char[] { ' ' };
    private static string[] EmptyStringArray = new string[0];

    protected const char TypeSymbol = 'T';
    protected const char ValueSymbol = ':';
    protected const char ReferenceSymbol = '*';
    protected const char StructSymbol = '{';
    protected const char AssetSymbol = '&';
    protected const char StringSymbol = '"';
    protected const char ListSymbol = '[';

    protected string[] lines;
    protected Dictionary<string, TypeReadResult> resultMap;
    protected Dictionary<string, TypeReadResult> structMap;
    protected Dictionary<string, Type> typeMap;
    protected Dictionary<string, TypeReadResult> tagMap;
    protected List<TypeReadResult> toClear;
    protected TypeReadResult context;
    protected int currentIdx;

    public AssetDeserializer(string source, bool fromFile = true) {
        typeMap = new Dictionary<string, Type>();
        resultMap = new Dictionary<string, TypeReadResult>();
        structMap = new Dictionary<string, TypeReadResult>();
        tagMap = new Dictionary<string, TypeReadResult>();
        toClear = new List<TypeReadResult>();
        if (fromFile && File.Exists(source)) {
            lines = File.ReadAllLines(source);
        }
        else if (!fromFile) {
            Regex regex = new Regex("(\r\n|\r|\n)");
            source = regex.Replace(source, "\n");
            lines = source.Split('\n');
        }
        if (lines != null) {
            Deserialize();
        }
    }

    public T CreateItem<T>(string id) where T : class, new() {
        TypeReadResult init = tagMap.Get(id);
        if (init == null) {
            return null;
        }
        if (!init.type.IsAssignableFrom(typeof(T))) {
            return null;
        }
        T retn = GetInstance(init) as T;
        for (int i = 0; i < toClear.Count; i++) {
            IEntityDeserializable d = toClear[i].instance as IEntityDeserializable;
            if (d != null) {
                d.AfterDeserialize();
            }
            toClear[i].instance = null;
        }
        toClear.Clear();
        return retn;
    }

    public T CreateItem<T>() where T : class, new() {
        return CreateItem<T>("__default__");
    }

    private void Deserialize() {
        currentIdx = 1;
        while (lines[currentIdx] != "--refs") {
            CreateType(lines[currentIdx]);
            currentIdx++;
        }
        currentIdx++;
        while (lines[currentIdx] != "--structs") {
            ReadRefLine(lines[currentIdx]);
            currentIdx++;
        }
        currentIdx++;
        while (lines[currentIdx] != "--fields") {
            ReadStructLine(lines[currentIdx]);
            currentIdx++;
        }
        currentIdx++;
        while (lines[currentIdx] != "--tags") {
            ReadTypeFieldLine(lines[currentIdx]);
            currentIdx++;
        }
        currentIdx++;
        while (currentIdx < lines.Length) {
            string line = lines[currentIdx];
            if (line == "") {
                currentIdx++;
                continue;
            }
            string[] split = line.Split(SpaceArray);
            tagMap[split[0]] = resultMap.Get(split[1]);
            currentIdx++;
        }
    }

    private void CreateType(string line) {
        typeMap[typeMap.Count.ToString()] = TypeCache.GetType(line);
    }

    private void ReadTypeFieldLine(string line) {
        TypeReadResult result = resultMap[line];
        for (int i = 0; i < result.fields.Length; i++) {
            currentIdx++;
            result.fields[i] = ReadFieldLine(lines[currentIdx]);
        }
    }

    private FieldReadResult ReadFieldLine(string line) {
        FieldReadResult result = new FieldReadResult();
        string[] segments = line.Split(SpaceArray, 4);
        result.fieldId = segments[0];
        if (segments[1] == "NULL") {
            result.value = null;
            result.symbol = 'x';
            result.strVal = "-1";
        }
        else {
            Type type = typeMap.Get(segments[1]);
            result.type = type;
            result.symbol = segments[2][0];
            result.strVal = segments[3];
        }
        return result;
    }

    private object GetValue(FieldReadResult result) {
        char symbol = result.symbol;
        string strVal = result.strVal;
        Type type = result.type;
        switch (symbol) {
            case TypeSymbol:
                return Type.GetType(strVal);
            case StructSymbol:
                return GetInstance(structMap[strVal]);
            case ValueSymbol:
                return ParsePrimitive(type, strVal);
            case ReferenceSymbol:
            case ListSymbol:
                return GetInstance((resultMap.Get(strVal)));
            case AssetSymbol:
                //todo - cache
                return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(strVal), result.type);
            case StringSymbol:
                return strVal;//todo unescape
        }
        return null;
    }

    private object GetInstance(TypeReadResult result) {
        if (result.instance == null) {
            //todo - struct

            TypeReadResult currentContext = context;
            object retn;
            if (result.type.IsArray) {
                //todo save this object [] and just set [0] = fieldCount
                retn = Activator.CreateInstance(result.type, new object[] { result.fields.Length });
            }
            else if (result.type.GetCustomAttributes(typeof(EntityDeserializerSkipConstructor), false).Length > 0) {
                retn = FormatterServices.GetUninitializedObject(result.type);
            }
            else {
                try {
                    retn = Activator.CreateInstance(result.type);
                }
                catch (MissingMethodException) {
                    retn = FormatterServices.GetUninitializedObject(result.type);
                }
            }
            result.instance = retn;
            context = result;
            SerializerUtil.GetTypeSerializer(result.type).Deserialize(retn, this);
            result.instance = retn;
            context = currentContext;
            toClear.Add(result);
        }
        return result.instance;
    }

    private object ParsePrimitive(Type type, string input) {
        if (type.IsEnum) {
            return int.Parse(input);
        }
        else if (type == typeof(int)) {
            return int.Parse(input);
        }
        else if (type == typeof(float)) {
            return float.Parse(input);
        }
        else if (type == typeof(double)) {
            return double.Parse(input);
        }
        else if (type == typeof(long)) {
            return long.Parse(input);
        }
        else if (type == typeof(byte)) {
            return byte.Parse(input);
        }
        else if (type == typeof(bool)) {
            return bool.Parse(input);
        }
        else if (type == typeof(char)) {
            return char.Parse(input);
        }
        else {
            Debug.LogError("Unable to parse primitive of type " + type.Name + ", from input: " + input);
            return Activator.CreateInstance(type);
        }
    }

    private void ReadRefLine(string line) {
        string[] segments = line.Split(SpaceArray, 6);
        TypeReadResult result = new TypeReadResult();
        result.refId = segments[0];
        result.creationMethod = segments[1];
        result.type = typeMap.Get(segments[5]);
        result.version = segments[4];
        result.uuid = segments[2];
        result.fields = new FieldReadResult[int.Parse(segments[3])];
        resultMap[result.refId] = result;
    }

    private void ReadStructLine(string line) {
        string[] segments = line.Split(SpaceArray);
        TypeReadResult result = new TypeReadResult();
        result.structId = segments[0];
        result.type = typeMap.Get(segments[5]);
        result.fields = new FieldReadResult[int.Parse(segments[3])];
        for (int i = 0; i < result.fields.Length; i++) {
            currentIdx++;
            result.fields[i] = ReadFieldLine(lines[currentIdx]);
        }
        structMap.Add(result.structId, result);
    }

    public void ReadDefault() {
        ReadDefaultExcept(EmptyStringArray);
    }

    public void ReadDefaultExcept(string[] exceptions) {
        object instance = context.instance;
        if (context.type.IsArray) {
            object[] array = instance as object[];
            for (int i = 0; i < context.fields.Length; i++) {
                array[i] = GetValue(context.fields[i]);
            }
        }
        else {
            for (int i = 0; i < context.fields.Length; i++) {
                int idx = Array.IndexOf(exceptions, context.fields[i].fieldId);
                if (idx != -1) {
                    continue;
                }
                FieldInfo fInfo = context.type.GetField(context.fields[i].fieldId, FieldBindFlags);
                if (fInfo == null) {
                    continue;
                }
                if (fInfo.FieldType.IsAssignableFrom(context.fields[i].type)) {
                    fInfo.SetValue(instance, GetValue(context.fields[i]));
                }
            }
        }
    }


    public object GetFieldValue(string fieldId) {
        for (int i = 0; i < context.fields.Length; i++) {
            if (context.fields[i].fieldId == fieldId) {
                return GetValue(context.fields[i]);
            }
        }
        return null;
    }

    public T GetFieldValue<T>(string fieldId) {
        for (int i = 0; i < context.fields.Length; i++) {
            if (context.fields[i].fieldId == fieldId) {
                return (T)GetValue(context.fields[i]);
            }
        }
        return default(T);
    }

    public T GetFieldValueAtIndex<T>(int index) {
        if (index < 0 || index >= context.fields.Length) {
            return default(T);
        }
        return (T)GetValue(context.fields[index]);
    }

    public object GetFieldValueAtIndex(int index) {
        if (index < 0 || index >= context.fields.Length) {
            return null;
        }
        return GetValue(context.fields[index]);
    }

    public string[] GetTags() {
        string[] retn = new string[tagMap.Count];
        int i = 0;
        foreach (var kvp in tagMap) {
            retn[i++] = kvp.Key;
        }
        return retn;
    }

    public void ReadField(string fieldId) {
        throw new NotImplementedException();
    }
}