using UnityEngine;

namespace AbilitySystem {

    [RequireComponent(typeof(SingleTargetStrategy))]
    public class LaunchChanneled : AbilityAction {

        public PointToPointBeam beamPrefab;
        protected PointToPointBeam spellInstance;
        protected Entity target;

        public override void OnCastStarted() {
            SingleTargetStrategy strategy = ability.TargetingStrategy as SingleTargetStrategy;
            spellInstance = Instantiate(beamPrefab, caster.transform.position, Quaternion.identity) as PointToPointBeam;
            spellInstance.Initialize(caster, strategy.target);
        }

        public override void OnCastInterrupted() {
            DestroySpell();
        }

        public override void OnCastCancelled() {
            DestroySpell();
        }

        public override void OnCastCompleted() {
            DestroySpell();
        }

        public void DestroySpell() {
            if (spellInstance != null) {
                Destroy(spellInstance.gameObject);
            }
        }
    }

}