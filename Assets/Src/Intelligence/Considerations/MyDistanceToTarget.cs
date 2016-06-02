
namespace Intelligence {

    public class MyDistanceToTarget : Consideration<SingleTargetContext> {

        public float maxDistance;

        public override float Score(SingleTargetContext context) {
            return 11;
        }

    }
}