using DiceRoll;

public class DispelResistance : StatusEffectComponent {

    public float baseResistance;

    public override bool OnDispelAttempted() {
        return DiceBag.Roll(1, Dice.D100) <= baseResistance;
    }

}