using System.Collections.Generic;

public class StatusEffectManager {

    public List<StatusEffect> statusList;
    public Entity entity;

    public StatusEffectManager() {
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
        Entity caster = context.Caster;
        StatusEffect existing = statusList.Find((StatusEffect s) => {
            return s.statusEffectId == effectId && s.caster == caster || s.IsUnique;
        });

        if (existing != null && existing.IsRefreshable) {
            existing.Refresh(context);
            return existing;
        }

        StatusEffect statusEffect = database.Create(effectId);

        if (existing != null) {
            existing.Remove();
            statusList.Remove(existing);
        }

        statusEffect.Apply(entity, context);
        statusList.Add(statusEffect);

        return statusEffect;
    }

    public bool DispelStatus(Entity caster, string statusId) {
        StatusEffect effect = statusList.Find((status) => {
            return status.caster == caster && status.statusEffectId == statusId;
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
            return status.caster == caster && status.statusEffectId == statusId;
        });
        if (effect != null) {
            statusList.Remove(effect);
            effect.Remove();
            return true;
        }
        return false;
    }

    public bool HasStatus(Entity caster, string statusName) {
        return true;
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

    private static JSONDatabase<StatusEffect> database;

    static StatusEffectManager() {
        database = new JSONDatabase<StatusEffect>("Status Effects");
    }
}

public class StatusNotFoundException : System.Exception {
    public StatusNotFoundException(string statusName) : base(statusName) { }
}