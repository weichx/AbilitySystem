using System;
using UnityEngine;

[Serializable]
public class ReduceTotalHealth : StatusEffectComponent {

    public float reduceByPercent;
    [NonSerialized]
    protected Resource health;

    public override void OnEffectApplied() {
        health = target.resourceManager.GetResource("Health");
        if (health != null && reduceByPercent > 0) {
            reduceByPercent = Mathf.Clamp01(reduceByPercent);
            health.SetModifier("ReduceTotalHealth", FloatModifier.Percent(reduceByPercent));
        }
    }

    public override void OnEffectRemoved() {
        if (health != null) {
            health.RemoveModifier("ReduceTotalHealth");
        }
    }

    public class ReduceTotalHealthOverTime : ReduceTotalHealth {

        public float perTickReduction;
        [NonSerialized]
        protected float initialValue;

        public override void OnEffectApplied() {
            health = target.resourceManager.GetResource("Health");
            if (health != null && reduceByPercent > 0) {
                initialValue = health.Max;
                reduceByPercent = Mathf.Clamp01(reduceByPercent);
                health.AddWatcher(MaybeStopDraining);
            }
        }

        public override void OnEffectTick() {
            //todo do reduction here
        }

        public override void OnEffectRemoved() {
            if (health == null) return;
            health.RemoveWatcher(MaybeStopDraining);
            health = null;
        }

        protected void MaybeStopDraining(float newValue, float oldValue) {
            if (health == null) return;
            if (health.Value < initialValue * reduceByPercent) {
                health.RemoveWatcher(MaybeStopDraining);
            }
        }
    }
}