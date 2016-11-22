using UnityEngine;
using EntitySystem;

namespace Intelligence {

    public class MyDistanceFromPoint : Consideration<PointContext> {

        public float range = 200;

        public override float Score(PointContext context) {
            Entity entity = context.entity;
            Vector3 point = context.point;
            float dist = Vector3.Distance(point, entity.transform.position);
            if (dist > range) dist = range;
            dist = Mathf.Clamp(dist, 1, range);
            return Mathf.Clamp01(dist / range);
        }

    }

}