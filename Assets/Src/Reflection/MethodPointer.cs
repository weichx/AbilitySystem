using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public abstract class AbstractMethodPointer : ISerializationCallbackReceiver {

    public string signature = "";
    protected Delegate original;

    public AbstractMethodPointer() { }

    public AbstractMethodPointer(MethodInfo info) {
        signature = MethodPointerUtils.CreateSignature(info);
        original = Reflector.FindDelegateWithSignature(signature);
        SetFromDelegate(original);
    }

    public AbstractMethodPointer(AbstractMethodPointer copy) {
        signature = copy.signature;
        SetMethod(copy.original);
    }

    public bool PointsToMethod {
        get { return original != null; }
    }

    public abstract void SetFromDelegate(Delegate del);

    public void SetMethod(Delegate newMethod) {
        original = newMethod;
        SetFromDelegate(original);
    }

    public Delegate GetMethod() {
        return original;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
        original = Reflector.FindDelegateWithSignature(signature);
        SetFromDelegate(original);
    }

    public override string ToString() {
        return signature;
    }

    public string ShortSignature {
        get {
            if (signature == null) return "";
            var parts = signature.Split(' ');
            return parts[1].Split('(')[0];
        }
    }
}

[Serializable]
public class MethodPointer : AbstractMethodPointer {

    protected Action fn;

    public MethodPointer(MethodInfo info) : base(info) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public void Invoke() {
        if (fn == null) return;
        fn();
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Action;
    }

}

public class MethodPointer<T> : AbstractMethodPointer {

    protected Func<T> fn;

    public MethodPointer(MethodInfo info) : base(info) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public T Invoke() {
        if (fn == null) return default(T);
        return fn();
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Func<T>;
    }
}

[Serializable]
public class MethodPointer<T, U> : AbstractMethodPointer {

    protected Func<T, U> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public U Invoke(T arg0) {
        if (fn == null) return default(U);
        return fn(arg0);
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Func<T, U>;
    }
}

[Serializable]
public class MethodPointer<T, U, V> : AbstractMethodPointer, ISerializationCallbackReceiver {

    protected Func<T, U, V> fn = null;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public V Invoke(T arg0, U arg1) {
        if (fn == null) return default(V);
        return fn(arg0, arg1);
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Func<T, U, V>;
    }

}

[Serializable]
public class MethodPointer<T, U, V, W> : AbstractMethodPointer, ISerializationCallbackReceiver {

    protected Func<T, U, V, W> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public W Invoke(T arg0, U arg1, V arg2) {
        if (fn == null) return default(W);
        return fn(arg0, arg1, arg2);
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Func<T, U, V, W>;
    }
}

[Serializable]
public class MethodPointer<T, U, V, W, X> : AbstractMethodPointer, ISerializationCallbackReceiver {

    protected Func<T, U, V, W, X> fn;

    public MethodPointer(MethodInfo methodInfo) : base(methodInfo) { }
    public MethodPointer(AbstractMethodPointer ptr) : base(ptr) { }

    public X Invoke(T arg0, U arg1, V arg2, W arg3) {
        if (fn == null) return default(X);
        return fn(arg0, arg1, arg2, arg3);
    }

    public override void SetFromDelegate(Delegate del) {
        fn = del as Func<T, U, V, W, X>;
    }
}

