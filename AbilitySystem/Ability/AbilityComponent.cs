using System;
using UnityEngine;

namespace AbilitySystem {

    public abstract class AbilityComponent :  MonoBehaviour {

        void Reset() {
            //hideFlags = HideFlags.HideInInspector;
        }

        public new GameObject gameObject {
            get { throw new Exception("Ability gameObject should never be accessed"); }
        }

        public new Transform transform {
            get { throw new Exception("Ability transform should never be accessed"); }
        }

    }    
}