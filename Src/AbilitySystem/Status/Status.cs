//using System;
//using System.Collections.Generic;
//using UnityEngine;

namespace AbilitySystem {
    //    public class StatusComponent : MonoBehaviour { }

    public class Status { }
}

//        public readonly Entity entity;
//        public readonly Entity caster;
//        public readonly PropertySet properties;
//        public readonly IStatusPrototype prototype;
//        public readonly Timer durationTimer;

//        public int stacks;
//        protected bool isDispelled;

//        public Status(Entity entity, Entity caster, IStatusPrototype prototype, PropertySet properties) {
//            this.entity = entity;
//            this.caster = caster;
//            this.prototype = prototype;
//            this.properties = properties;
//            durationTimer = new Timer();
//            isDispelled = false;
//        }

//        public string Name {
//            get { return prototype.Name; }
//        }

//        public bool IsDispelled {
//            get { return isDispelled; }
//        }

//        public bool IsExpired {
//            get { return durationTimer.Ready; }
//        }

//        public void Apply() {
//            prototype.OnApply(this);
//        }

//        public void Update() {
//            prototype.OnUpdate(this);
//        }

//        public void Refresh(PropertySet newPropertySet) {
//            if (stacks < prototype.MaxStacks) {
//                stacks++;
//                //OnStackAdded(); ??
//            }
//           // properties.Merge(newPropertySet);
//            durationTimer.Reset(); //todo to what value?
//        }

//        public void Dispel() {
//            if (isDispelled) return;
//            prototype.OnDispel(this);
//            isDispelled = true;
//        }

//        public void Expire() {
//            if (!durationTimer.Ready) return;
//            prototype.OnExpire(this);
//        }

//        public void Remove() {
//            prototype.OnRemove(this);
//            //properties = null;
//        }

//        public StatusComponent CreateDebugBehavior(GameObject targetObject) {
//            return null;
//        }
//    }

//}