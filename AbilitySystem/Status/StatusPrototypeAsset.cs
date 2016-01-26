using UnityEngine;
using System;

public class StatusPrototypeAsset : ScriptableObject, IStatusPrototype {

    public string diminishCategory;
    public GameObject obj;
    [SerializeField]
    protected int maxStacks;
    [SerializeField]
    protected bool isDispellable;
    [SerializeField]
    protected bool isRefreshable;


    public StatusDelegate onApply = new StatusDelegate(typeof(OnStatusAppliedHandler));
    //public StatusDelegate onUpdate = new StatusDelegate(typeof(OnStatusUpdatedHandler));
    //public StatusDelegate onRefresh = new StatusDelegate(typeof(OnStatusRefreshedHandler));
    //public StatusDelegate onDispel = new StatusDelegate(typeof(OnStatusDispelledHandler));
    //public StatusDelegate onExpire = new StatusDelegate(typeof(OnStatusExpiredHandler));
    //public StatusDelegate onRemove = new StatusDelegate(typeof(OnStatusRemovedHandler));
    public TagCollection tags;

    public static float Stuff() { return 1; }

    public void OnValidate() {
        if (maxStacks <= 0) maxStacks = 1;
    }

    public string Name {
        get { return name; }
        set { name = value; }
    }

    public bool IsDispellable {
        get { return isDispellable; }
        set { isDispellable = value; }
    }

    public bool IsRefreshable {
        get { return isRefreshable; }
        set { isRefreshable = value; }
    }

    public int MaxStacks {
        get { return maxStacks; }
        set { maxStacks = value; }
    }

    public TagCollection Tags {
        get { return tags; }
        set { tags = value; }
    }

    public bool IsStackable {
        get { return MaxStacks > 1; }
    }

    public void OnUpdate(Status status) {
        throw new NotImplementedException();
    }

    public void OnApply(Status status) {
        throw new NotImplementedException();
    }

    public void OnRefresh(Status status) {
        throw new NotImplementedException();
    }

    public void OnDispel(Status status) {
        throw new NotImplementedException();
    }

    public void OnExpire(Status status) {
        throw new NotImplementedException();
    }

    public void OnRemove(Status status) {
        throw new NotImplementedException();
    }
}
