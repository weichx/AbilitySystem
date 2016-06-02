namespace Intelligence {

    public class TargetsInDirection : Consideration<DirectionalContext> {

        public override float Score(DirectionalContext context) {
            return 1;
        }

    }

}