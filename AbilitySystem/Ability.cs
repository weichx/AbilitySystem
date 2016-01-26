using UnityEngine;
using System;
using System.Collections.Generic;

public class Ability : AbstractAbility {

    public Ability(Entity caster) : base(caster) {   }

    //this is as basic as it gets
    public override bool OnTargetSelectionUpdated() {
        return true;
    }

}


















