using UnityEngine;
using System.Collections.Generic;

namespace AbilitySystem {
    public class StatusDatabase {

        static Dictionary<string, IStatusPrototype> database = new Dictionary<string, IStatusPrototype>();

        public static bool AddPrototype(string name, IStatusPrototype prototype) {
            if (database.ContainsKey(name)) {
                return false;
            }
            database[name] = prototype;
            return true;
        }

        public static IStatusPrototype GetPrototype(string name) {
            return null;
        }

    }

    public enum StatusState {
        Expired, Dispelled, Active
    }

    public class StatusManager : MonoBehaviour {

        public List<StatusEffect> statusList;
        public Entity entity;

        public void Start() {
            entity = GetComponent<Entity>();
            statusList = new List<StatusEffect>();
        }

        public void Update() {
            for (int i = 0; i < statusList.Count; i++) {
                StatusEffect status = statusList[i];
                status.Update();
                if (status.ReadyForRemoval) {
                    status.Remove();
                    statusList.RemoveAt(--i);
                }
            }
        }

        public void AddStatus(Ability ability, StatusEffectPrototype prototype) {
            StatusEffect status = prototype.CreateStatus(ability, entity);
            StatusEffect existing = statusList.Find((StatusEffect s) => {
                return s.name == prototype.name && s.caster == ability.caster || s.IsUnique;
            });
            if(existing != null && existing.IsRefreshable) {
                existing.Refresh();
            }
            else if(existing != null) {
                existing.Remove();
                statusList.Remove(existing);
                status.Apply();
                statusList.Add(status);
            }
            else {
                status.Apply();
                statusList.Add(status);
            }
        }

        public bool DispelStatus(Entity caster, string statusName) {
            StatusEffect effect = statusList.Find((status) => {
                return status.caster == caster && status.name == statusName;
            });
            if (effect != null) {
                effect.Dispel();
                if(effect.state != StatusState.Active) {
                    statusList.Remove(effect);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveStatus(Entity caster, string statusName) {
            StatusEffect effect = statusList.Find((status) => {
                return status.caster == caster && status.name == statusName;
            });
            if(effect != null) {
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

    }

    public class StatusNotFoundException : System.Exception {
        public StatusNotFoundException(string statusName) : base(statusName) { }
    }
}