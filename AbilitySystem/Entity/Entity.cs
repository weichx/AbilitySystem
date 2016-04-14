using UnityEngine;

//todo -- this is a log of every action an entity has taken
//entries are in a circular buffer. this class provies
//methods to query and log any action taken by this entity
public class EntityActionLog<T> where T : class {

}

namespace AbilitySystem {
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour {

        [HideInInspector]
        public string id;

        [HideInInspector]
        public StatusManager statusManager;
        [HideInInspector]
        public EventManager eventManager;
        [HideInInspector]
        public AbilityManager abilityManager;
        [HideInInspector]
        public TargetManager targetManager;

        public ModifiableVariable movementSpeed;
        public ModifiableVariable health;
        public ModifiableVariable energy;

        public Faction faction;
        private bool movedThisFrame = false;

        private Vector3 lastPosition;

        public virtual void Awake() {
            EntityManager.Instance.Register(this);
            gameObject.layer = LayerMask.NameToLayer("Entity");
            eventManager = new EventManager();
        }

        public virtual void Start() {
            Debug.Log(typeof(EntityActionLog<object>).Name);
            statusManager = GetComponent<StatusManager>();
            abilityManager = GetComponent<AbilityManager>();
            targetManager = GetComponent<TargetManager>();
        }

        public Entity Target {
            get; set;
        }
        
        public virtual void Update() {
            lastPosition = transform.position;
            //todo find a better system for this, dont need all these updates
            //to run every frame
            health.Update();
            movementSpeed.Update();
            energy.Update();
        }

        public void LateUpdate() {
            movedThisFrame = lastPosition != transform.position;
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
    }
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
