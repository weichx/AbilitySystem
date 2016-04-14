using AbilitySystem;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AIAction_UseAbility : AIAction {

    public string abilityId;
    public float range;

    public override void OnStart() {
        entity.GetComponent<NavMeshAgent>().ResetPath();
        entity.Target = context.target;
        entity.abilityManager.Cast(abilityId);
        var animator = entity.GetComponent<Animator>();
        animator.SetInteger("PhaseId", 100);
    }

    public override ActionStatus OnUpdate() {
        if (entity.abilityManager.IsCasting) {
            return ActionStatus.Running;
        }
        return ActionStatus.Success;
    }

    public override AIDecisionContext[] GetContexts() {
        return AIDecisionContext.CreateFromEntityHostileList(entity, 200);
    }

}

public class ActionContext {

    public readonly List<Entity> targets;
    public readonly List<Vector3> positions;

    protected Ability ability;
    protected Entity caster;
    protected Dictionary<string, object> properties;

    public ActionContext() {
        properties = new Dictionary<string, object>();
        targets = new List<Entity>();
        positions = new List<Vector3>();
    }

    public void Initialize(Ability ability) {
        this.ability = ability;
        caster = ability.caster;
    }

    public void Clear() {
        ability = null;
        caster = null;
        properties.Clear();
        targets.Clear();
        positions.Clear();
    }

    public Entity Caster {
        get { return caster; }
    }

    public Ability Ability {
        get { return ability; }
    }

    public Entity MainTarget {
        get { return targets.First(); }
    }

    public Vector3 MainPosition {
        get { return positions.Last(); }
    }

    public object Get(string propertyName) {
        return properties.Get(propertyName);
    }

    public T Get<T>(string propertyName) {
        return (T)properties.Get(propertyName);
    }

    public void Set(string propertyName, object value) {
        properties[propertyName] = value;
    }

    public void Set<T>(string propertyName, T value) {
        properties[propertyName] = value;
    }

    public bool Has(string propertyName) {
        return properties.ContainsKey(propertyName);
    }
}
