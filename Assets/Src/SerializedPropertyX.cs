using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;

public class SerializedPropertyX {

    public bool isExpanded;
    public readonly string name;
    public readonly string displayName;
    public readonly GUIContent label;
    public readonly Type type;
    protected List<SerializedPropertyX> children;
    protected SerializedObjectX serializedObjectX;
    private object originalValue;
    private object value;
    private bool changed;
    private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static object CreateValue(Type type) {
        if (type == typeof(string)) {
            return "";
        }
        else if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
            return null;
        }
        var ctor = type.GetConstructor(Type.EmptyTypes);
        if (ctor == null) {
            return FormatterServices.GetUninitializedObject(type);
        }
        return Activator.CreateInstance(type);
    }

    public SerializedPropertyX(SerializedObjectX serializedObjectX, string fieldName, Type type, object value) {
        this.serializedObjectX = serializedObjectX;
        this.type = type;
        this.value = value ?? CreateValue(type);
        originalValue = value;
        changed = false;
        name = fieldName;
        displayName = Util.SplitAndTitlize(fieldName);
        isExpanded = true; //todo find a way to get a unique id for these so we can save isExpanded between loads
        label = new GUIContent(displayName);
        BuildProperties(true);
    }

    private void BuildProperties(bool nukeChildren) {
        if (nukeChildren) {
            children = new List<SerializedPropertyX>();
        }
        if (type.IsPrimitive || type.IsEnum || type.IsEnum || type == typeof(string) || value == null) return;

        if (type.IsArray) {
            if (type.GetArrayRank() != 1) return;
            Array array = value as Array ?? Array.CreateInstance(type.GetElementType(), 1);
            Type elementType = type.GetElementType();
            for (int i = 0; i < array.Length; i++) {
                var element = array.GetValue(i);
                if (element != null) {
                    children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, element.GetType(), element));
                }
                else {
                    element = CreateValue(elementType);
                    children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, elementType, CreateValue(elementType)));
                    array.SetValue(element, i);
                }
            }
        }
        else if (typeof(IList).IsAssignableFrom(type)) {
            IList list = value as IList;
            Type elementType = type.GetGenericArguments()[0];
            for (int i = 0; i < list.Count; i++) {
                var element = list[i];
                if (element != null) {
                    children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, element.GetType(), element));
                }
                else {
                    element = CreateValue(elementType);
                    children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, elementType, CreateValue(elementType)));
                    list[i] = element;
                }
            }
        }
        else {
            FieldInfo[] fields = value.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++) {
                FieldInfo fInfo = fields[i];
                if (fInfo.IsNotSerialized) continue;
                if (fInfo.IsPrivate) {
                    var attrs = fInfo.GetCustomAttributes(false);
                    if (attrs.Length == 0) continue;
                    bool isSerialized = Array.Find(attrs, (object a) => { return a.GetType() == typeof(SerializeField); }) != null;
                    if (!isSerialized) continue;
                }
                if (children.Find(child => fInfo.Name == child.name) == null) {
                    SerializedPropertyX propertyX = new SerializedPropertyX(serializedObjectX, fInfo.Name, fInfo.FieldType, fInfo.GetValue(value));
                    children.Add(propertyX);
                }
            }
        }

    }

    public void ApplyModifiedProperties(SerializedPropertyX parent) {
        Type fieldType = value == null ? type : value.GetType();
        for (int i = 0; i < children.Count; i++) {
            SerializedPropertyX child = children[i];
            child.ApplyModifiedProperties(this);
            if (IsArrayLike) {
                IList list = value as IList;
                list[i] = child.Value;
            }
            else {
                fieldType.GetField(child.name, BindFlags).SetValue(value, child.Value);
            }

        }
    }

    public void Update() {
        if (value == null) {
            children = new List<SerializedPropertyX>();
        }
        else {
            Type fieldType = value.GetType();
            if (IsArrayLike) {
                IList list = value as IList;
                for (int i = 0; i < children.Count; i++) {
                    children[i].Value = list[i];
                }
            }
            else {
                for (int i = 0; i < children.Count; i++) {
                    SerializedPropertyX child = children[i];
                    child.Value = fieldType.GetField(child.name).GetValue(value);
                }
            }
        }

    }

    public bool IsArrayLike {
        get { return type.IsArray || typeof(IList).IsAssignableFrom(type); }
    }

    public int ChildCount {
        get {
            return children.Count;
        }
    }

    public int ArraySize {
        get {
            if (!IsArrayLike) return 0;
            else return children.Count;
        }
        set {
            if (!IsArrayLike) {
                return;
            }
            int size = value;
            if (size < 0) size = 0;
            if (children.Count == size) return;
            Changed = true;
            if (type.IsArray) {
                ResizeArray(size);
            }
            else {
                ResizeList(size);
            }

        }
    }

    private void ResizeList(int size) {
        IList list = value as IList;
        if (size > children.Count) {
            Type elementType = type.GetGenericArguments()[0];
            for (int i = children.Count; i < size; i++) {
                var element = CreateValue(elementType);
                children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, elementType, element));
                list.Add(element);
            }
        }
        else {
            int i = children.Count - 1;
            while (i != size) {
                list.RemoveAt(i);
                i--;
            }
            children.RemoveRange(size, (children.Count - size));
        }
    }

    private void ResizeArray(int size) {
        Array array = Array.CreateInstance(type.GetElementType(), size);
        if (size > children.Count) {
            Type elementType = type.GetElementType();
            int oldSize = children.Count;
            for (int i = 0; i < size; i++) {
                if (i < oldSize) {
                    array.SetValue(GetChildAt(i).Value, i);
                }
                else {
                    var element = CreateValue(elementType);
                    children.Add(new SerializedPropertyX(serializedObjectX, "Element " + i, elementType, element));
                    array.SetValue(element, i);
                }
            }

        }
        else {
            children.RemoveRange(size, (children.Count - size));
        }
        value = array;
    }

    public bool Changed {
        get { return changed; }
        private set {
            changed = value;
        }
    }

    public SerializedPropertyX GetChildAt(int idx) {
        if (children == null || idx < 0 || idx >= children.Count) return null;
        return children[idx];
    }

    public SerializedPropertyX FindProperty(string propertyName) {
        return children.Find((child) => child.name == propertyName);
    }

    public void SwapArrayElements(int index, int direction) {
        if (!IsArrayLike) return;
        direction = Mathf.Clamp(direction, -1, 1);
        var tempChild = children[index];
        children[index + direction] = children[index];
        children[index] = tempChild;
        IList list = value as IList;
        var temp = list[index];
        list[index] = list[index + direction];
        list[index + direction] = temp;
    }

    public void DeleteArrayElementAt(int index) {
        if (!IsArrayLike || index < 0 || index >= children.Count) return;
        int start = index;
        if (start == 0) start = 1;
        if (type.IsArray) {
            Array array = value as Array;
            for (int i = start; i < array.Length; i++) {
                var element = array.GetValue(i);
                array.SetValue(element, i - 1);
            }
            ResizeArray(array.Length - 1);
        }
        else {
            IList list = value as IList;
            if (list != null) {
                
                for (int i = start; i < list.Count; i++) {
                    var element = list[i];
                    list[i - 1] = element;
                }
                ResizeList(list.Count - 1);
            }

        }
    }

    public T GetValue<T>() {
        return (T)value;
    }

    public object Value {
        get { return value; }
        set {
            if (value != null) {
                if (!type.IsAssignableFrom(value.GetType())) {
                    throw new Exception("Unable to assign " + value.GetType().Name + " to " + type.Name);
                }
                Type oldValueType = this.value == null ? null : this.value.GetType();
                this.value = value;
                BuildProperties(true);//oldValueType != value.GetType()););
            }
            else {
                BuildProperties(true);
            }
            Changed = value != originalValue;
        }
    }

}