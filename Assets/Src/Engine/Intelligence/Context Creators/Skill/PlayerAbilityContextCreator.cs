namespace Intelligence {

    public abstract class PlayerAbilityContextCreator : PlayerContextCreator {

        protected Ability ability;

        public virtual void Setup(Entity entity, Ability ability) {
            this.entity = entity;
            this.ability = ability;
        }

    }

}