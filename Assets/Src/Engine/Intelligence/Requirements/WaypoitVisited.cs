
namespace Intelligence {

    public class WaypointNotVisited : Requirement<PointContext> {

        public override bool Check(PointContext context) {
            if (context.entity.vectors.Contains(context.point)) {
                return false;
            }
            return !context.entity.vectors.Contains(context.point);
        }

    }

}