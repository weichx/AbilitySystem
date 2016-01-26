using UnityEngine;

public class ProjectileAbility : Ability {

    public HomingProjectile projectilePrefab;


    public ProjectileAbility(Entity caster, HomingProjectile projectilePrefab) : base(caster) {
        this.projectilePrefab = projectilePrefab;
    }

    public override void OnCastCompleted() {
        var prefab = Object.Instantiate(projectilePrefab, caster.transform.position + Vector3.up, caster.transform.rotation);
        var projectile = prefab as HomingProjectile;
        var target = caster.GetComponent<TargetManager>().currentTarget;
        projectile.transform.LookAt(target.transform.position, Vector3.up);
        projectile.Initialize(caster, target, GetAttributeSnapshot());
    }

}

public class SingleTargetAbilityPrototype : AbilityPrototype {

    public HomingProjectile projectilePrefab;
    public float projectileSpeed;
    public ModifiableAttribute attr;
    //public ModifiableAttributePrototype attr;
    //public List<ModifiableAttributePrototype> attrs;

    //proto = {name, fn/float}
    //can auto copy attributes, resources, requirements, tags, cast type, 
    public override Ability CreateAbility(Entity caster) {
        var ability = new ProjectileAbility(caster, projectilePrefab);
        ability.AddAttribute("ProjectileSpeed", attr);
        attr.Update();
        ability.AddRequirement(new HostileTargetRequirement());
        ability.AddRequirement(new StationaryRequirement());
        return ability;
    }
}