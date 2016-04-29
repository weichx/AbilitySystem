using UnityEngine;
using AbilitySystem;

public class AIConsideration_MyProximityToEntity : AIConsideration {

    public float rangeMin;
    public float rangeMax = 100;
    public string entityId;

    public override float Score(Context context) {
        Entity target = EntityManager.Instance.FindEntity(entityId);
        if (target == null) return 0f;
        Vector3 targetPosition = target.transform.position;
        Vector3 position = context.entity.transform.position;
        float distanceSqr = targetPosition.DistanceToSquared(position);
        float min = rangeMin * rangeMin;
        float max = rangeMax * rangeMax;
        float ratio = (distanceSqr - min) / (max - min);
        return 1 - ratio;
    }

}