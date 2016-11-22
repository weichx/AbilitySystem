using System.Collections.Generic;
using UnityEngine;
using EntitySystem;

namespace Intelligence {

    public class PointOfInterestCollector : ContextCollector<PointContext> {

        public float radius;

        public override List<Context> Collect(CharacterAction<PointContext> action, Entity entity) {
            GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            List<Context> retn = new List<Context>(waypoints.Length);

            for (int i = 0; i < waypoints.Length; i++) {
                retn.Add(new PointContext(entity, waypoints[i].transform.position));
            }
            return retn;
        }

    }

}
