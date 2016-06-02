using System.Collections.Generic;

namespace Intelligence {

    public class PointOfInterestCollector : ContextCollector<PointContext> {

        public float radius;

        public override List<PointContext> Collect(CharacterAction<PointContext> action, Entity entity) {
            return new List<PointContext>();
        }

    }

}
