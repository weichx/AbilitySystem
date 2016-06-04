using System;
using UnityEngine;

///<summary>
///All custom asset types that are persisted (Ability, Status, Decision, etc) need
///to inherit from EntitySystemBase so the peristence layer can act on them. 
///</summary>
[Serializable]
public abstract class EntitySystemBase {

    [SerializeField] protected string id;

    public virtual string Id {
        get { return (id == null) ? GetType().Name + GetHashCode() : id; }
        set { id = value; }
    }

}