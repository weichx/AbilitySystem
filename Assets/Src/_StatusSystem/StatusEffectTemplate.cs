public class StatusEffectTemplate {
    public string statusEffectId;
    public StatusEffectComponent[] additionalComponents;
    public OldContext context;
    public string[] tags;
    public FloatAttribute duration;

    public virtual StatusEffect Create(Entity target) {
        StatusEffect statusEffect = EntitySystemLoader.Instance.Create<StatusEffect>(statusEffectId);

        statusEffect.AddStatusComponentsFromTemplate(this);
        if (duration != null) {
            statusEffect.duration = duration;
        }
        if (tags != null) {
            for (int i = 0; i < tags.Length; i++) {
                statusEffect.tags.Add(new Tag(tags[i]));
            }
        }
        return statusEffect;
    }

    public virtual OldContext GetContext(Entity target) {
        if (context == null) {
            context = new OldContext();
        }
        context["target"] = target;
        context.entity = EntityManager.ImplicitEntity;

        return context;
    }
}