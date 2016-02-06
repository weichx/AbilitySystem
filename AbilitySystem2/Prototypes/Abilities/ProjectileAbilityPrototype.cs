using UnityEngine;

namespace AbilitySystem {

    public class ProjectileAbilityPrototype : AbilityPrototype {

        public GameObject projectile;

        public override void OnCastCompleted(Ability ability, PropertySet properties) {
            Entity caster = ability.caster;
            GameObject obj = Instantiate(projectile, caster.transform.position, caster.transform.rotation) as GameObject;
            var proj = obj.GetComponent<Projectile>();
            proj.speed = ability.GetAttribute("ProjectileSpeed").UpdateValue(ability);
            ability.GetAttribute("ProjectileSpeed").AddModifier(new AttributeModifier<Ability>("ProjectileSpeedBonus", 2));
        }
    }
}