namespace AbilitySystem {
    public class AbilityUsedEvent : GameEvent {
        public Ability ability;

        public AbilityUsedEvent(Ability ability) {
            this.ability = ability;
        }
    }
}