using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using Unity.IO.Compression;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetSerializer : IWriter {

    public class WriteTarget {

        public object item;
        public string tagId;
        public int fieldCount;
        public string fields;
        public string version;
        public int id;

        public WriteTarget(object item) {
            this.item = item;
        }

        public WriteTarget(string id, object item) {
            this.tagId = id;
            this.item = item;
        }

        public string GetRefLine(Dictionary<Type, int> typeMap) {
            //Type type = item.GetType();
            //string typeName = type.Name + "." + type.Namespace;
            string[] header = new string[] {
            id.ToString(), "new", "-1", fieldCount.ToString(), version, typeMap.Get(item.GetType()).ToString()
        };
            return string.Join(" ", header);
        }
    }

    protected static BindingFlags FieldBindFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
    private static string[] EmptyStringArray = new string[0];

    protected const string ValueSymbol = ": ";
    protected const string StringSymbol = "\" ";
    protected const string ObjectSymbol = "* ";
    protected const string ListSymbol = "[ ";
    protected const string EmptyListSymbol = "] ";
    protected const string RawSymbol = "> ";
    protected const string StructSymbol = "{ ";
    protected const string AssetSymbol = "& ";
    protected const string ErrorSymbol = "x ";

    protected int RefId;
    protected int TypeId;
    protected int writeCalls;
    protected object context;
    protected StringBuilder builder;
    protected List<WriteTarget> inputItems;
    protected Queue<WriteTarget> queue;
    protected Dictionary<Type, int> typeMap;
    protected Dictionary<object, int> refMap;
    protected List<WriteTarget> structTargets;
    protected Dictionary<object, WriteTarget> outputTargets;

    public AssetSerializer() {
        builder = new StringBuilder(500);
        typeMap = new Dictionary<Type, int>();
        refMap = new Dictionary<object, int>();
        inputItems = new List<WriteTarget>();
        outputTargets = new Dictionary<object, WriteTarget>();
        structTargets = new List<WriteTarget>();
        RefId = 0;
    }

    public bool AddItem<T>(T item) where T : class {
        return AddItem("__default__", item);
    }

    public bool AddItem<T>(string itemId, T item) where T : class {
        if (item == null) {
            return false;
        }
        //todo containment check
        WriteTarget target = new WriteTarget(itemId, item);
        GetTypeId(item.GetType());
        inputItems.Add(target);
        return true;
    }

    public void RemoveItem<T>(string itemId) where T : class, new() {
        throw new NotImplementedException();
    }

    public void WriteToFile(string path) {
        File.WriteAllText(path, WriteToString());
    }

    public string WriteToString() {
        RefId = 0;
        structTargets.Clear();
        outputTargets.Clear();
        queue = new Queue<WriteTarget>(inputItems);
        while (queue.Count > 0) {
            Serialize(queue.Dequeue());
        }
        StringBuilder refBuilder = new StringBuilder(outputTargets.Count);
        StringBuilder fieldBuilder = new StringBuilder(500);
        StringBuilder structBuilder = new StringBuilder(500);
        StringBuilder tagBuilder = new StringBuilder(100);
        StringBuilder typeBuilder = new StringBuilder(100);
        typeBuilder.AppendLine("--types");
        foreach (var kvp in typeMap) {
            typeBuilder.AppendLine(kvp.Key.AssemblyQualifiedName);
        }
        refBuilder.AppendLine("--refs");
        fieldBuilder.AppendLine("--fields");
        structBuilder.AppendLine("--structs");
        tagBuilder.AppendLine("--tags");

        foreach (var kvp in outputTargets) {
            WriteTarget target = kvp.Value;
            refBuilder.AppendLine(target.GetRefLine(typeMap));
            fieldBuilder.AppendLine(target.id.ToString());
            fieldBuilder.Append(target.fields);
            if (target.tagId != null) {
                tagBuilder.Append(target.tagId);
                tagBuilder.Append(" ");
                tagBuilder.AppendLine(target.id.ToString());
            }
        }
        for (int i = 0; i < structTargets.Count; i++) {
            Serialize(structTargets[i]);
        }
        for (int i = 0; i < structTargets.Count; i++) {
            WriteTarget target = structTargets[i];
            target.id = i;
            structBuilder.AppendLine(target.GetRefLine(typeMap));
            structBuilder.Append(target.fields);
        }
        string output = typeBuilder.ToString()
            + refBuilder.ToString()
            + structBuilder.ToString()
            + fieldBuilder.ToString()
            + tagBuilder.ToString();
        return output;
        // File.WriteAllBytes(path + ".gzip", AssetSerializerHelper.Zip(refBuilder.ToString() + fieldBuilder.ToString() + structBuilder.ToString() + tagBuilder.ToString()));
    }

    public void WriteDefault() {
        WriteDefaultExcept(EmptyStringArray);
    }

    public void WriteDefaultExcept(string[] exceptions) {
        FieldInfo[] fields = context.GetType().GetFields(FieldBindFlags);
        for (int i = 0; i < fields.Length; i++) {
            //object[] attrs = fields[i].GetCustomAttributes(typeof(SerializeField), true);
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

    //todo better off just calling getType() here and not taking a parameter
    public void WriteField(string fieldId, Type type, object value) {
        writeCalls++;
        //todo asserts around type, value matching
        builder.Append(fieldId);
        builder.Append(" ");
        if (value == null) {
            builder.Append("NULL");
        }
        else {
            builder.Append(GetTypeId(type));
        }
        builder.Append(" ");
        builder.AppendLine(GetSerializedValue(value));
    }

    protected void Serialize(WriteTarget target) {
        Type type = target.item.GetType();
        context = target.item;
        int id;
        if (type.IsClass) {
            id = GetRefId(target.item);
            outputTargets[id] = target;
        }
        else {
            id = -1;
        }
        //string creationType = SerializerUtil.GetCreationType(type, context);
        TypeSerializer serializer = SerializerUtil.GetTypeSerializer(type);
        builder.Length = 0;
        writeCalls = 0;
        serializer.Serialize(target.item, this);
        target.id = id;
        target.fieldCount = writeCalls;
        target.fields = builder.ToString();
        target.version = serializer.GetVersion();
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
            IList list = value as IList;
           // object[] valArray = value as object[]; //todo this may not work for structs?
            if (list == null || list.Count == 0) {
                return EmptyListSymbol;
            }
            else {
                return ListSymbol + GetWriteTargetId(value).ToString();
            }
        }
        //consider not allowing structs since they are problematic, override a class serializer if you want them
        else if (type.IsValueType) {
            return StructSymbol + GetWriteTargetId(value);
        }
        else if (type.IsClass) {
            UnityEngine.Object uObj = value as UnityEngine.Object;
            if (uObj == null) {
                return ObjectSymbol + GetWriteTargetId(value).ToString();
            }
            else {
#if UNITY_EDITOR
                string assetPath = AssetDatabase.GetAssetPath(uObj);
                if (string.IsNullOrEmpty(assetPath)) {
                    if (PrefabUtility.GetPrefabType(uObj) == PrefabType.None) {
                        //todo write out game object heirarchy
                        //traverse gameobject to root
                        //ensure each gameobject + transform is serialized
                        //return a ref to parent component
                        return " { SOMEKINDAGAMEOBJECT";
                    }
                    return " { Not Asset";
                }
                else {
                    return AssetSymbol + AssetDatabase.AssetPathToGUID(assetPath);
                }
#else
                return "??";
#endif
            }
            //if is unityobject && !subclass of component || gameobject -> serialize
            //todo check if is asset (prefab | texture | scripatable | mesh | etc)
        }
        else {
            return ErrorSymbol + SerializerUtil.GetTypeName(type);
        }
    }

    protected int GetWriteTargetId(object value) {
        WriteTarget target = null;
        if (value.GetType().IsValueType) {
            target = new WriteTarget(value);
            target.id = structTargets.Count;
            structTargets.Add(target);
            return target.id;
        }
        int refId = GetRefId(value);
        target = outputTargets.Get(refId);
        if (target == null) {
            target = new WriteTarget(value);
            target.id = refId;
            outputTargets.Add(refId, target);
            queue.Enqueue(target);
        }
        return refId;
    }

    private int GetTypeId(Type type) {
        if (type == null) {
            return -1;
        }
        int id;
        if (typeMap.TryGetValue(type, out id)) {
            return id;
        }
        id = TypeId++;
        typeMap[type] = id;
        return id;
    }

    private int GetRefId(object obj) {
        if (obj == null) return -1;
        int id;
        if (refMap.TryGetValue(obj, out id)) {
            return id;
        }
        id = RefId++;
        refMap[obj] = id;
        return id;
    }
}

//[CLSCompliant(true)]
static class AssetSerializerHelper {

    //public static object GetValueForValueType<T>(this FieldInfo field, ref T item) where T : struct {
    //    return field.GetValueDirect(__makeref(item));
    //}

    //public static void SetValueForValueType<T>(this FieldInfo field, ref T item, object value) where T : struct {
    //    field.SetValueDirect(__makeref(item), value);
    //}

    public static void CopyTo(Stream src, Stream dest) {
        byte[] bytes = new byte[4096];

        int cnt;

        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
            dest.Write(bytes, 0, cnt);
        }
    }

    public static byte[] Zip(string str) {
        var bytes = Encoding.UTF8.GetBytes(str);

        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream()) {
            using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
                //msi.CopyTo(gs);
                CopyTo(msi, gs);
            }

            return mso.ToArray();
        }
    }

    public static string Unzip(byte[] bytes) {
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream()) {
            using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
                //gs.CopyTo(mso);
                CopyTo(gs, mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }
}
