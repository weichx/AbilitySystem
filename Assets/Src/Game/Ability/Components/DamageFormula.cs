using Intelligence;
using EntitySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class DamageFormula : AbilityComponent<Context> {
    public bool debugMode;
    public List<Modifier> modifiers;
    public DiceBase inputValue;
    public float outputValue;

    public override void OnUse() {
        var diceCreator = new DiceCreator();
        var sum = (float)diceCreator[inputValue].Result ;
        for(int i = 0; i < modifiers.Count; i++) {
            var j = sum;
            modifiers[i].SetContext(this.context);
            modifiers[i].ApplyModifier(ref sum);
            if(debugMode) Debug.Log("Apply modifier:"+ modifiers[i].GetType() + " [ input value: " + j + " => " + sum + "]");
        }
        outputValue = sum;
    }
}
