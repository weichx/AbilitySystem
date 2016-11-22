using EntitySystem;
using System.Collections.Generic;

namespace Intelligence {


    public class CollectEntitiesInRadius : ContextCollector<SingleTargetContext> {

        public MethodPointer<Entity, float> SearchRange;
        public MethodPointer<Entity, int> FactionMask;

        public override List<Context> Collect(CharacterAction<SingleTargetContext> action, Entity entity) {
            float radius = SearchRange.Invoke(entity);
            int factionMask = FactionMask.Invoke(entity);
            List<Entity> targets = EntityManager.Instance.FindEntitiesInRange(entity.transform.position, radius, factionMask);
            List<Context> retn = new List<Context>(targets.Count);
            for (int i = 0; i < retn.Count; i++) {
                retn.Add(new SingleTargetContext(entity, targets[i]));
            }
            return retn;
        }

    }

}
