
namespace Intelligence {

    public class WaypointNotVisited : Requirement<PointContext> {

        public override bool Check(PointContext context) {
            return true;
        }

    }

}