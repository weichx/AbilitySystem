using System;
using System.Collections.Generic;

public abstract class Modifier<T> where T : class {

    protected List<T> targets;
    public IMatcher<T> matcher;

    [NonSerialized]
    private bool initialized;

    public void Initialize() {
        if (initialized) return;
        initialized = true;
        targets = new List<T>();
        OnInitialize();
    }

    public void Apply(T thing) {
        if ((matcher == null || matcher.Match(thing)) && !targets.Contains(thing)) {
            targets.Add(thing);
            OnApply(thing);
        }
    }

    public void Update() {
        OnUpdate();
    }

    public void Remove(T thing) {
        if (targets.Remove(thing)) {
            OnRemove(thing);
        }
    }

    protected virtual void OnInitialize() {  }

    protected virtual void OnApply(T thing) {  }

    protected virtual void OnUpdate() { }

    protected virtual void OnRemove(T thing) { }

}