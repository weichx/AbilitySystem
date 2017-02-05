using System;
using UnityEngine;
using System.Collections.Generic;


///<summary>
/// Entity the root of the system, any game object that interacts with
/// Abilities, Status Effects, or AI needs to have an entity component
/// </summary>
namespace EntitySystem {
		
	[SelectionBase]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(EventEmitter))]
	public partial class Entity : MonoBehaviour {

	    public string factionId;

	    [HideInInspector] public string id;
	    [HideInInspector] public AbilityManager abilityManager;
	    [HideInInspector] public ResourceManager resourceManager;
	    [HideInInspector] public StatusEffectManager statusManager;
            [HideInInspector] public InventoryItemManager itemManager;

	    private Vector3 lastPosition;
	    private bool movedThisFrame = false;
		protected EventEmitter emitter;

		//handle progression of entity, attributes, and resources
	    public void Awake() {
	        if (!string.IsNullOrEmpty(source)) {
	            new AssetDeserializer(source, false).DeserializeInto("__default__", this);
	        } else {
	            new SerializedObjectX(this).ApplyModifiedProperties();
	        }
	        resourceManager = resourceManager ?? new ResourceManager(this);
	        statusManager = statusManager ?? new StatusEffectManager(this);
	        abilityManager = abilityManager ?? new AbilityManager(this);
            itemManager = itemManager ?? new InventoryItemManager(this);
	        emitter = GetComponent<EventEmitter>();
	        EntityManager.Instance.Register(this);

	       // gameObject.layer = LayerMask.NameToLayer("Entity");
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
        	if (itemManager != null) {
           	     itemManager.Update();
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

}
