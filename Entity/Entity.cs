using UnityEngine;
using AbilitySystem;

//todo -- this is a log of every action an entity has taken
//entries are in a circular buffer. this class provies
//methods to query and log any action taken by this entity
//public class EntityActionLog<T> where T : class {

//}


[SelectionBase]
[DisallowMultipleComponent]
public class Entity : MonoBehaviour {

    public string factionId;

    [HideInInspector] public string id;
    [HideInInspector] public AbilityManager abilityManager;
    [HideInInspector] public ResourceManager resourceManager;

    [SerializeField]  protected Vector3 castPoint;
    [SerializeField]  protected Vector3 castTarget;

    private Vector3 lastPosition;
    private bool movedThisFrame = false;

    public virtual void Awake() {
        EntityManager.Instance.Register(this);
        resourceManager = new ResourceManager();
        gameObject.layer = LayerMask.NameToLayer("Entity");
    }

    public Entity Target { //todo remove this after fixing requirements
        get; set;
    }

    public virtual void Update() {
        lastPosition = transform.position;
        if (abilityManager != null) {
            abilityManager.Update();
        }
    }

    public void LateUpdate() {
        movedThisFrame = lastPosition != transform.position;
    }

    #region properties
    public Vector3 CastPoint {
        get {
            return transform.TransformPoint(castPoint);
        }
    }

    public Vector3 CastTarget {
        get {
            return transform.TransformPoint(castTarget);
        }
    }

    public bool IsMoving {
        get { return movedThisFrame; }
    }

    public bool IsPlayer {
        get { return tag == "Player"; }
    }

    public bool IsCasting {
        get { return abilityManager.IsCasting; }
    }

    public Ability ActiveAbility {
        get { return abilityManager.ActiveAbility; }
    }

    public bool IsChanneling {
        get { return abilityManager.ActiveAbility != null && abilityManager.ActiveAbility.IsChanneled; }
    }
    #endregion
}

//figure out how to use formulas nicely, ie assign in editor (prefab reference? scriptable object? reflection? nothing?)
//figure out how things like damage / healing get applied in concert with abilities

//ability.GetAttribute("String");
//abilityManager.Get("BackStab").RemoveRequirement<Behind>();
/*
    var backstab = abilityManager.Get("BackStab");
    var behind = backstab.RemoveRequirement<Behind>();
    backstab.OnNextUse(() => {
        backstab.AddRequirement(behind);
    });
    entity.OnAbilityUsed("AbilityId", (Ability ability) => {) {
            if(Abilty.hasTag("tag")) {
                //re-enable requirement
            }
        });
    }

    ability.GetAttribute<Damage>(out dmgAttr);
    ability.GetAttribute("Damage")();

    ability.GetAffectedTargets(); //array of all targets effected by this ability, ability implementations are expected to append to this list or mabye return it from onCompleted

    entity.OnAbilityUsed((ability) => {
        resourceRequirements = ability.GetRequirements<ResourceRequirement>();
        resourceRequirements.ForEach((requirement) => {
            entity.resourceManager.useResources(requirement.Type, requirement.Value);
        });
    });

    //ability.OnCompleted() -> get snapshots here
    //entity.AbilityUsed(); -> adjust attributes / modifiers here

    entity.OnAbilityWithTagUsed(tagCollection, (usedAbility) => {
        
    });

    //need to be able to assign different formulas to the same prototypes
    //need to handle case where many targets are affected and all of them differently (like chain lightning)
    //attributes can handle lots of things and are really really flexible, i can use attributes to 
    //add `jumps` to chain lighting or all sorts of other things like # of targets to heal.
    //an ability can be given multiple attributes that specific to it without effecting other attributes.
    //this can be done in a type safe way too!

    class Viscious : Status {
        OnApplied() {
            entity.OnAbilityUsedWithTag(tagCollection, () => {
                entity.status.RemoveStack(this, 1);
                ability.GetAttribute<Damage>().Value
                ability.GetAttribute<Healing>().Value
                ability.GetAttribute<ManaHeal>().Value
                ability.SetAttribute<Damage>(new Damage() | value);
                target.resourceManager.useResource<Health>(ability.GetAttribute<Damage>());
            }, AbilityCallback.Once);
        }

        void Update() {
        
        }
    }

    resourceManager.OnResourceExpended<Health>(() => {});
    resourceManager.OnResourceBelowThreshold<Health>(value | () => {}, () => {});

    option 1: ability controls raw damage amount (before resist / armor / whatever)
    option 2: spawned 'thing' controls damage (but I dont know always want a spawned thing if just playing animation for melee attacks)
    option 3: add ability.GetAttributeSnapshot<Damage>() to get values at time of cast completed
*/
