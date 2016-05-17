using System.Collections.Generic;

public class StatusEffectManager {

    protected List<StatusEffect> statusList;
    protected Entity entity;

    public StatusEffectManager(Entity entity) {
        this.entity = entity;
        statusList = new List<StatusEffect>();
    }

    public void Update() {
        for (int i = 0; i < statusList.Count; i++) {
            StatusEffect status = statusList[i];
            status.UpdateComponents();
            if (status.ReadyForRemoval) {
                status.Remove();
                statusList.RemoveAt(--i);
            }
        }
    }

    public StatusEffect AddStatusEffect(string effectId, Context context) {
        Entity caster = context.entity;
        StatusEffect existing = statusList.Find((StatusEffect s) => {
            return s.Id == effectId && s.caster == caster || s.IsUnique;
        });

        if (existing != null && existing.IsRefreshable) {
            existing.Refresh(context);
            return existing;
        }

        StatusEffect statusEffect = EntitySystemLoader.Instance.Create<StatusEffect>(effectId);

        if (existing != null) {
            existing.Remove();
            statusList.Remove(existing);
        }

        statusEffect.Apply(entity, context);
        statusList.Add(statusEffect);

        return statusEffect;
    }

    //this is pseudo private
    public void AddStatusEffectFromTemplate(StatusEffectTemplate template) {
        StatusEffect effect = template.Create(entity);
        Context context = template.GetContext(entity);
        effect.Apply(entity, context);
        statusList.Add(effect);
    }

    public bool DispelStatus(Entity caster, string statusId) {
        StatusEffect effect = statusList.Find((status) => {
            return status.caster == caster && status.Id == statusId;
        });
        if (effect != null) {
            effect.Dispel();
            if (effect.state != StatusState.Active) {
                statusList.Remove(effect);
                return true;
            }
        }
        return false;
    }

    public bool RemoveStatus(string statusId, Entity caster) {
        StatusEffect effect = statusList.Find((status) => {
            return status.caster == caster && status.Id == statusId;
        });
        if (effect != null) {
            statusList.Remove(effect);
            effect.Remove();
            return true;
        }
        return false;
    }

    public bool HasStatus(string statusName) {
        for (var i = 0; i < statusList.Count; i++) {
            if (statusList[i].Id == statusName) {
                return true;
            }
        }
        return false;
    }

    public bool HasStatus(StatusEffectCreator creator) {
        for(var i = 0; i < statusList.Count; i++) {
            if(statusList[i].Creator == creator) {
                return true;
            }
        }
        return false;
    }

    public bool HasStatus(Entity caster, string statusName) {
        for (var i = 0; i < statusList.Count; i++) {
            if (statusList[i].Id == statusName) {
                if (statusList[i].caster == caster) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasStatusWithTag(Tag tag) {
        return true;
    }

    public StatusEffect GetStatus(Entity caster, string statusName) {
        return null;
    }

    public StatusEffect GetStatus(string statusName) {
        return null;
    }

    public StatusEffect[] GetAllStatusesWithTag(Tag tag) {
        return null;
    }

}

public class StatusNotFoundException : System.Exception {
    public StatusNotFoundException(string statusName) : base(statusName) { }
}