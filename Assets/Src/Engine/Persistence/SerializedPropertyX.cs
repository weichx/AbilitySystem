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
    public readonly bool IsDrawable;
    protected List<SerializedPropertyX> children;
    private object originalValue;
    private object value;
    private bool changed;
    private bool isRoot;
    private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private SerializedPropertyX parent;
    private bool isCircular;
    private SerializedPropertyX circularRef;

    public SerializedPropertyX(string name, Type type, object value = null, bool isDrawable = true) {
        this.name = name;
        this.type = type;
        this.value = value ?? CreateValue(type);
        originalValue = value;
        changed = false;
        displayName = Util.SplitAndTitlize(name);
        children = new List<SerializedPropertyX>();
        isExpanded = false;
        label = new GUIContent(displayName);
        IsDrawable = isDrawable;
        isRoot = true;
        isCircular = false;
        BuildProperties();
    }

    private SerializedPropertyX(SerializedPropertyX parent, string name, Type type, object value = null, bool isDrawable = true) {
        //   :this(name, type, value, isDrawable) {
        this.name = name;
        this.type = type;
        this.value = value ?? CreateValue(type);
        originalValue = value;
        changed = false;
        displayName = Util.SplitAndTitlize(name);
        children = new List<SerializedPropertyX>();
        isExpanded = false;
        label = new GUIContent(displayName);
        IsDrawable = isDrawable;
        isRoot = true;
        isCircular = false;
        this.parent = parent;
        if (value != null && parent != null) {
            SerializedPropertyX ptr = parent;
            while (ptr != null) {
                if (ptr.Value == value) {
                    isCircular = true;
                    circularRef = ptr;
                    break;
                }
                ptr = ptr.parent;
            }
        }
        BuildProperties();
        isRoot = false;
    }

    public SerializedPropertyX(FieldInfo fInfo, object value) : this(fInfo.Name, fInfo.FieldType, value) {
        IsDrawable = fInfo.GetCustomAttributes(typeof(HideInInspector), true).Length == 0;
    }

    private SerializedPropertyX CreateChildProperty(string name, Type type, object value, bool isDrawable) {
        value = value ?? CreateValue(type);
        if (value != null) {
            SerializedPropertyX ptr = this;
            while (ptr != null) {
                if (ptr.Value == value && type.IsByRef) {
                    isCircular = true;
                    circularRef = ptr;
                    return circularRef;
                }
                ptr = ptr.parent;
            }
        }
        return new SerializedPropertyX(this, name, type, value, isDrawable);
    }

    private void BuildProperties() {
        if (isCircular) return;
        for (int i = 0; i < children.Count; i++) {
            children[i].parent = null;
        }
        children = new List<SerializedPropertyX>();
        if (type.IsPrimitive || type.IsEnum || type.IsEnum || type == typeof(string) || value == null) return;

        if (type.IsArray || typeof(IList).IsAssignableFrom(type)) {
            IList list;
            Type elementType;
            if (type.IsArray) {
                if (type.GetArrayRank() != 1) return;
                list = value as IList ?? Array.CreateInstance(type.GetElementType(), 1);
                elementType = type.GetElementType();
            }
            else {
                elementType = type.GetGenericArguments()[0];
                list = value as IList ?? Activator.CreateInstance(type) as IList;
            }

            for (int i = 0; i < list.Count; i++) {
                var element = list[i] ?? CreateValue(elementType);
                list[i] = element;
                children.Add(CreateChildProperty("Element " + i, element.GetType(), element, true));
            }
        }
        else {

            FieldInfo[] fields = value.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++) {
                FieldInfo fInfo = fields[i];
                if (fInfo.IsNotSerialized) continue;
                if (!fInfo.IsPublic) {
                    var attrs = fInfo.GetCustomAttributes(true);
                    if (attrs.Length == 0) continue;
                    bool isSerialized = Array.Find(attrs, (object a) => { return a.GetType() == typeof(SerializeField); }) != null;
                    if (!isSerialized) { continue; }
                }
                if (children.Find(child => fInfo.Name == child.name) == null) {
                    bool isDrawable = fInfo.GetCustomAttributes(typeof(HideInInspector), true).Length == 0;
                    SerializedPropertyX propertyX = CreateChildProperty(fInfo.Name, fInfo.FieldType, fInfo.GetValue(value), isDrawable);
                    propertyX.parent = this;
                    children.Add(propertyX);
                }
            }
        }

    }

    public bool ApplyModifiedProperties() {
        if (!changed) return false;
        changed = false;
        if (isCircular) return circularRef.ApplyModifiedProperties();
        Type fieldType = value == null ? type : value.GetType();
        for (int i = 0; i < children.Count; i++) {
            SerializedPropertyX child = children[i];
            child.ApplyModifiedProperties();
            if (IsArrayLike) {
                IList list = value as IList;
                list[i] = child.Value;
            }
            else {
                fieldType.GetField(child.name, BindFlags).SetValue(value, child.Value);
            }
        }
        return true;
    }

    public bool IsCircular {
        get { return isCircular;}
    }

    public bool IsOrphaned {
        get { return parent == null; }
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
    public SerializedPropertyX GetParent {
        get {
            if (parent == null) return null;
            return parent;
        }
    }

    public bool IsArrayLike {
        get {
            if (isCircular) return circularRef.IsArrayLike;
            return type.IsArray || typeof(IList).IsAssignableFrom(type);
        }
    }

    public int ChildCount {
        get {
            if (isCircular) return circularRef.ChildCount;
            return children.Count;
        }
    }

    public int ArraySize {
        get {
            if (isCircular) return circularRef.ArraySize;
            if (!IsArrayLike) return 0;
            else return children.Count;
        }
        set {
            if (isCircular) {
                circularRef.ArraySize = value;
            }

            if (!IsArrayLike) {
                return;
            }
            int size = value;
            if (size < 0) size = 0;
            if (children.Count == size) return;
            changed = true;
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
                children.Add(CreateChildProperty("Element " + i, elementType, element, true));
                list.Add(element);
            }
        }
        else {
            while (list.Count != size) {
                list.RemoveAt(list.Count - 1);
            }
            for (int i = size; i < children.Count; i++) {
                children[i].parent = null;
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
                    children.Add(CreateChildProperty("Element " + i, elementType, element, true));
                    array.SetValue(element, i);
                }
            }

        }
        else {
            Array.Copy(value as Array, array, size);
            for (int i = size; i < children.Count; i++) {
                children[i].parent = null;
            }
            children.RemoveRange(size, (children.Count - size));
        }
        value = array;
        changed = true;
    }

    public bool Changed {
        get { return changed; }
        private set {
            if (changed) return;
            changed = true;
            SerializedPropertyX ptr = parent;
            while (ptr != null) {
                ptr.changed = true;
                ptr = ptr.parent;
            }
        }
    }

    public SerializedPropertyX GetChildAt(int idx) {
        if (isCircular) return circularRef.GetChildAt(idx);
        if (children == null || idx < 0 || idx >= children.Count) return null;
        return children[idx];
    }

    public SerializedPropertyX FindProperty(string propertyName) {
        if (isCircular) return circularRef.FindProperty(propertyName);
        return children.Find((child) => child.name == propertyName);
    }

    public void SwapArrayElements(int index, int direction) {
        if (isCircular) {
            circularRef.SwapArrayElements(index, direction);
        }
        if (!IsArrayLike) return;
        direction = Mathf.Clamp(direction, -1, 1);
        var tempChild = children[index];
        children[index] = children[index + direction];
        children[index + direction] = tempChild;
        IList list = value as IList;
        var temp = list[index];
        list[index] = list[index + direction];
        list[index + direction] = temp;
        changed = true;
    }

    public void DeleteArrayElementAt(int index) {
        if (isCircular) {
            circularRef.DeleteArrayElementAt(index);
            return;
        }
        if (!IsArrayLike || index < 0 || index >= children.Count) return;
        int start = index;
        IList list = value as IList;
        children[index].parent = null;
        if (list != null) {

            for (int i = start + 1; i < list.Count; i++) {
                list[i - 1] = list[i];
                children[i - 1] = children[i];
            }
        }
        if (type.IsArray) {
            ResizeArray(list.Count - 1);
        }
        else {
            ResizeList(list.Count - 1);
        }
        changed = true;
    }

    public int GetChildIndex(SerializedPropertyX child) {
        if (isCircular) return circularRef.GetChildIndex(child);
        return children.IndexOf(child);
    }

    public T GetValue<T>() {
        return isCircular ? (T)circularRef.Value : (T)value;
    }

    public object Value {
        get { return isCircular ? circularRef.Value : value; }
        set {
            if (isCircular) {
                circularRef.Value = value;
                return;
            }
            bool equal = ((value != null) && value.GetType().IsValueType)
                     ? value.Equals(this.value)
                     : (value == this.value);

            if (equal) return;

            if (value != null) {
                if (!type.IsAssignableFrom(value.GetType())) {
                    throw new ArgumentException("Unable to assign " + value.GetType().Name + " to " + type.Name);
                }
            }
            this.value = value;
            BuildProperties();
            Changed = value != originalValue;
            originalValue = this.value;
        }
    }

    public Type Type {
        get {
            if (value == null) return type;
            return value.GetType();
        }
    }

    public SerializedPropertyX this[string path] {
        get { return FindProperty(path); }
    }

    public override string ToString() {
        return name;
    }

    private static object CreateValue(Type type) {
        if (type == typeof(string)) {
            return "";
        }
        //we need these to be null on initialization since there isnt a default constructor 
        //and they are not value types. Type is weird since its tied to the runtime so tightly
        else if (typeof(Type) == type || type.IsSubclassOf(typeof(UnityEngine.Object))) {
            return null;
        }
        if (type.IsArray) {
            return Array.CreateInstance(type.GetElementType(), 0);
        }
        var ctor = type.GetConstructor(Type.EmptyTypes);
        if (ctor != null && !type.IsAbstract) {
            return Activator.CreateInstance(type);
        }
        else if (type.IsValueType) { //todo -- maybe drop this, uninitialized things are unpredictable i think
            return FormatterServices.GetUninitializedObject(type);
        }
        else {
            return null;
        }
    }

}