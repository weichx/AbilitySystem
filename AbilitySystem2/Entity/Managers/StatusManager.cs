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

    public class StatusManager : MonoBehaviour {

        public readonly List<Status> statusList;
        public Entity entity;

        public void AddStatus(Entity caster, string statusName, float duration = -1f, PropertySet properties = null) {
            var proto = StatusDatabase.GetPrototype(statusName);
            if (proto == null) throw new StatusNotFoundException(statusName);
            var existingStatus = GetStatus(statusName, caster);
            if (existingStatus != null) {
                existingStatus.Refresh(properties);
            }
            else {
                var status = new Status(entity, caster, proto, properties);
                status.Apply();
                statusList.Add(status);
            }
        }

        public void Update() {
            return;
            //for (int i = 0; i < statusList.Count; i++) {
            //    var status = statusList[i];
            //    status.Update();
            //    if (status.IsDispelled) {
            //        status.Dispel();
            //        status.Remove();
            //        statusList.RemoveAt(--i);
            //    }
            //    else if (status.IsExpired) {
            //        status.Expire();
            //        status.Remove();
            //        statusList.RemoveAt(--i);
            //    }
            //}
        }

        public Status GetStatus(string statusName, Entity caster) {
            for (int i = 0; i < statusList.Count; i++) {
                if (statusList[i].caster == caster && statusList[i].Name == statusName) {
                    return statusList[i];
                }
            }
            return null;
        }

        public List<Status> GetStatuses(string statusName) {
            var retn = new List<Status>();
            for (int i = 0; i < statusList.Count; i++) {
                if (statusList[i].Name == statusName) {
                    retn.Add(statusList[i]);
                }
            }
            return retn;
        }

        public bool HasStatus(string statusName, Entity caster) {
            for (int i = 0; i < statusList.Count; i++) {
                if (statusList[i].caster == caster && statusList[i].Name == statusName) {
                    return true;
                }
            }
            return false;
        }

        public bool HasStatus(string statusName) {
            for (int i = 0; i < statusList.Count; i++) {
                if (statusList[i].Name == statusName) {
                    return true;
                }
            }
            return false;
        }

        protected void Refresh(Status status, PropertySet propertySet) {

            if (status.prototype.IsRefreshable) {

            }
            else {

            }
        }
    }

    public class StatusNotFoundException : System.Exception {
        public StatusNotFoundException(string statusName) : base(statusName) { }
    }
}