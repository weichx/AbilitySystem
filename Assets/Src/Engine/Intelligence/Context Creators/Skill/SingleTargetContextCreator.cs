namespace Intelligence {

    public class SingleTargetContextCreator : PlayerAbilityContextCreator {

        public override Context GetContext() {
            return new SingleTargetContext(entity, entity.GetComponent<TargetManager>().currentTarget);
        }
    }

}