
namespace Intelligence {
   
    public class MyTargetWithinRadius : Requirement<SingleTargetContext> {
       
        public float radius;
        public MethodPointer<float, float> ptr;

        public override bool Check(SingleTargetContext context) {

            Entity me = context.entity;
            Entity him = context.target;

            return me.transform.DistanceToSquared(him.transform.position) <= radius * radius;
        }

        [Pointable]
        public static float Test(float val) {
            return 0;
        }

    }

}