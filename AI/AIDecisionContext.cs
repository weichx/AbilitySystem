using AbilitySystem;
using System.Collections.Generic;
using UnityEngine;

//todo this should be extended somehow to handle other "focus" objects than target
public class AIDecisionContext {
    public Entity target;
    [HideInInspector] public Entity entity;


    //todo pool these
    public static AIDecisionContext[] CreateFromEntityHostileList(Entity source, float range) {
        List<Entity> hostiles = EntityManager.Instance.NearestHostiles(source, range);
        var retn = new AIDecisionContext[hostiles.Count];
        for(int i = 0; i < retn.Length; i++) {
            var ctx = new AIDecisionContext();
            ctx.entity = source;
            ctx.target = hostiles[i];
            retn[i] = ctx;
        }
        
        return retn;
    }
}
