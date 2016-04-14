public class DamageEvent : GameEvent {

    public float baseDamage;

    public DamageEvent(float baseDamage) {
        this.baseDamage = baseDamage;

    }

    public override string ToString() {
        return baseDamage + " damage";
    }

}