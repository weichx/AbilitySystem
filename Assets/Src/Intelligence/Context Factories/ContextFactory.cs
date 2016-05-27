using System;
using System.Collections.Generic;
using UnityEngine;

namespace Intelligence {

	[Serializable]
	public abstract class ContextFactory<T> where T : Context {
	
		public abstract T[] CreateContexts(Entity entity);

	}

	[Serializable]
	public class SingleTargetContextFactory : ContextFactory<SingleTargetContext> {
	
		public int factionMask = -1; // todo this is temporary until factions are real
		public float searchRange = 40f;

		public override SingleTargetContext[] CreateContexts(Entity entity) {
			List<Entity> targets = EntityManager.Instance.FindEntitiesInRange(entity.transform.position, searchRange, factionMask);
			SingleTargetContext[] retn = new SingleTargetContext[targets.Count];
			for(int i = 0; i < retn.Length; i++) {
				retn[i] = new SingleTargetContext(entity, targets[i]);
			}
			return retn;
		}

	}

	[Serializable]
	public abstract class PointContextFactory : ContextFactory<PointContext> {}

	[Serializable]
	public abstract class MultiPointContextFactory : ContextFactory<MultiPointContext> {}

}

/*

//does a skill require a context of a given type? leaning towards yes but maybe its assigned as a field
//if I go with the field approach I need to re-validate all context related things when context type
//changes if the context types are not compatible

SkillSet

	Skill : Factory : DSE

*/