using System;

[Serializable]
public class AddStatusEffect : AbilityComponent {

    public StatusEffectCreator statusCreator;
    [EnumFlag] public CastEvent applyOnEvent;

}