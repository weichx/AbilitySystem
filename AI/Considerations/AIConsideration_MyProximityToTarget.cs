using UnityEngine;
using AbilitySystem;

public class AIConsideration_MyProximityToTarget : AIConsideration {
    public float rangeMin;
    public float rangeMax = 100;

    public override float Score(Context context) {
        Entity target = context.Get<Entity>("target");
        if (target == null) return 0;

        Vector3 targetPosition = target.transform.position;
        Vector3 position = context.entity.transform.position;
        float distanceSqr = targetPosition.DistanceToSquared(position);
        float min = rangeMin * rangeMin;
        float max = rangeMax * rangeMax;

        float ratio = (distanceSqr - min) / (max - min);
        return ratio;
    }

}
