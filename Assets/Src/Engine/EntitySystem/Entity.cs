using System;
using UnityEngine;
using System.Collections.Generic;


///<summary>
/// Entity the root of the system, any game object that interacts with
/// Abilities, Status Effects, or AI needs to have an entity component
/// </summary>

[SelectionBase]
[DisallowMultipleComponent]
public partial class Entity : MonoBehaviour {

    public string factionId;
    //temp
    [NonSerialized] public List<Vector3> vectors = new List<Vector3>();

    [HideInInspector]
    public string id;
    public AbilityManager abilityManager;
    public ResourceManager resourceManager;
    public StatusEffectManager statusManager;

    [SerializeField]
    protected Vector3 castPoint;
    [SerializeField]
    protected Vector3 castTarget;

    private Vector3 lastPosition;
    private bool movedThisFrame = false;
	protected EventEmitter emitter;

    //temp -- move to AI
    [NonSerialized] public Dictionary<string, object> blackboard = new Dictionary<string, object>();

    //temp
    public FloatRange attr;
    public FloatRange attr2;
	//handle progression of entity, attributes, and resources
    public void Awake() {
        attr = new FloatRange(0);
        attr.SetModifier("Mod1", FloatModifier.Value(1));
        attr.SetModifier("Mod2", FloatModifier.Value(3));
        attr.SetModifier("Mod3", FloatModifier.Value(6));
        attr2 = new FloatRange(0);
        attr2.SetModifier("Mod1", FloatModifier.Value(5));
        attr2.SetModifier("Mod2", FloatModifier.Percent(0.2f));
        attr2.SetModifier("Mod3", FloatModifier.Value(5));

        resourceManager = new ResourceManager(this);
        statusManager = new StatusEffectManager(this);
        abilityManager = new AbilityManager(this);
		emitter = new EventEmitter();
        EntityManager.Instance.Register(this);
        //gameObject.layer = LayerMask.NameToLayer("Entity");
    }

    public virtual void Update() {
        lastPosition = transform.position;
        if (abilityManager != null) {
            abilityManager.Update();
        }
        if (statusManager != null) {
            statusManager.Update();
        }
        if (resourceManager != null) {
            //resourceManager.Update();
        }
        if (emitter != null) {
            emitter.FlushQueue();
        }
    }

    public void LateUpdate() {
        movedThisFrame = lastPosition != transform.position;
    }

    #region Properties

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
		
	public EventEmitter EventEmitter {
		get { 
			return emitter;
		}
	}
    #endregion
}
