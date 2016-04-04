
using System;

using System.Collections.Generic;
using UnityEngine;
using AbilitySystem;

public class AIConsideration_MyEntitiesInRadius : AIConsideration {

    public float radius;
    public int maxEntities;
    public int minEntities;
    public int factionMask;

    public override float Score(AIDecisionContext context) {
        Vector3 position = context.entity.transform.position;
        List<Entity> entities = EntityManager.Instance.FindEntitiesInRange(position, radius, factionMask);
        return 1f - ((entities.Count - minEntities) / (maxEntities - minEntities));
    }

}