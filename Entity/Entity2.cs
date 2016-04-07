//using System;
//using UnityEngine;
//using System.Collections.Generic;

//namespace Entity {

//    public class Ability { }

//    public abstract class AbilityModifier {
//        public abstract void Apply(Ability ability);
//        public abstract void Restore(Ability ability);
//    }

//    public abstract class AbilityMatcher {

//        public Ability[] Match(Ability[] abilitySet) {
//            int count = 0;
//            Ability[] retn = new Ability[abilitySet.Length];
//            for(int i = 0; i < abilitySet.Length; i++) {
//                if(IsMatch(abilitySet[i])) {
//                    retn[count++] = abilitySet[i];
//                }
//            }
//            Array.Resize(ref retn, count);
//            return retn;
//        }

//        protected abstract bool IsMatch(Ability ability);
//    }

//    public abstract class AttributeModifier { }

//    public partial class Entity : MonoBehaviour {

//        protected KeyValuePair<AbilityMatcher, AbilityModifier> modifierPairs;

//        public void AddAbilityModifier(AbilityMatcher matcher, AbilityModifier modifier) {
//            var abilities = matcher.Match(new Ability[0]);
//            for(int i = 0; i < abilities.Length; i++) {
//                modifier.Apply(abilities[i]);
//            }
//            modifierPairs = new KeyValuePair<AbilityMatcher, AbilityModifier>(matcher, modifier);
//        }

//        public void AddAttributeModifier(string attribute, AttributeModifier modifier) {

//        }
//    }

//}