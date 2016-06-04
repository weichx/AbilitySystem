using System;
using UnityEngine;

[Serializable]
public abstract class EntitySystemBase {

    [SerializeField] protected string id;

    public virtual string Id {
        get { return (id == null) ? GetType().Name + GetHashCode() : id; }
        set { id = value; }
    }

}