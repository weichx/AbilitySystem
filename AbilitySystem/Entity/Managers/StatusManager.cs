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
                status.UpdateActions();
                if (status.ReadyForRemoval) {
                    status.Remove();
                    statusList.RemoveAt(--i);
                    Debug.Log("NUKED");
                }
            }
        }

        public void AddStatus(StatusEffect statusPrefab, Entity caster) {
            StatusEffect status = Instantiate(statusPrefab) as StatusEffect;
            status.name = statusPrefab.name;
            //status.transform.parent = transform;
            StatusEffect existing = statusList.Find((StatusEffect s) => {
                return s.name == statusPrefab.name && s.caster == caster || s.IsUnique;
            });
            if(existing != null && existing.IsRefreshable) {
                existing.Refresh();
            }
            else if(existing != null) {
                existing.Remove();
                statusList.Remove(existing);
                status.Initialize(caster, entity);
                status.Apply();
                statusList.Add(status);
            }
            else {
                status.Initialize(caster, entity);
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

        public bool RemoveStatus(string statusName, Entity caster) {
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