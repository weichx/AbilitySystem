using System;
using System.Collections.Generic;

public abstract class Modifier<T> where T : class {

    protected List<T> targets;
    public IMatcher<T> matcher;
    public string id;

    [NonSerialized]
    private bool initialized;

    public Modifier(string id) {
        this.id = id;
    }

    public Modifier() { }

    public void Initialize() {
        if (initialized) return;
        initialized = true;
        targets = new List<T>();
        OnInitialize();
    }

    public void Apply(T modified) {
        if ((matcher == null || matcher.Match(modified)) && !targets.Contains(modified)) {
            targets.Add(modified);
            OnApply(modified);
        }
    }

    public void Update() {
        OnUpdate();
    }

    public void Remove(T modified) {
        if (targets.Remove(modified)) {
            OnRemove(modified);
        }
    }

    public virtual void OnInitialize() {  }

    public virtual void OnApply(T modified) {  }

    public virtual void OnUpdate() { }

    public virtual void OnRemove(T modified) { }

}