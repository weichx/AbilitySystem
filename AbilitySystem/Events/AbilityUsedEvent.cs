public class AbilityUsedEvent : GameEvent {
    public AbstractAbility ability;
    
    public AbilityUsedEvent(AbstractAbility ability) {
        this.ability = ability;
    }
}