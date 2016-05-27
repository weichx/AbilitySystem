
//everything defined in a template is implicit to that entity and not 
//applied via a real context, probably use Entity.Implicit or Entity.System
//to refer to it in Entity references

public class EntityTemplate {
    public string name;
    public string parentTemplate; //templates can be inherited
    public string factionId;
    //Talent[] talents -- todo implement talents as basically a static modifier set with tags
    public Resource[] resources;
    public Ability[] abilities;
    //should basically never be used since these should almost always be added via status
    //or something like a talent
    public AbilityModifier[] abilityModifiers;
    public StatusEffectTemplate[] statusEffectTemplates;
    //public StatusEffectDescriptor

    public virtual void Apply(Entity entity) {
        entity.name = name;
        entity.factionId = factionId;
        AddResources(entity);
        AddStatusEffects(entity);
        AddAbilities(entity);
    }

    protected virtual void AddAbilities(Entity entity) {
        if (abilities != null) {
            for (int i = 0; i < abilities.Length; i++) {
                entity.abilityManager.AddAbility(abilities[i].Id);
            }
        }

        if (abilityModifiers != null) {
            for (int i = 0; i < abilityModifiers.Length; i++) {
                entity.abilityManager.AddAbilityModifier(abilityModifiers[i]);
            }
        }
    }

    protected virtual void AddResources(Entity entity) {
        if (resources == null) return;
        for (int i = 0; i < resources.Length; i++) {
            entity.resourceManager.AddResource(resources[i].resourceId, resources[i]);
        }
    }

    protected virtual void AddStatusEffects(Entity entity) {
        if (statusEffectTemplates == null) return;
        for (int i = 0; i < statusEffectTemplates.Length; i++) {
            entity.statusManager.AddStatusEffectFromTemplate(statusEffectTemplates[i]);
        }
    }

    protected virtual void AddTalents(Entity entity) {

    }
}