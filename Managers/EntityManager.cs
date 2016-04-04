using UnityEngine;
using AbilitySystem;
using System.Collections.Generic;

public class EntityManager {

    public List<Entity> entities = new List<Entity>();
    private static EntityManager instance;

    public void Register(Entity entity) {
        if (string.IsNullOrEmpty(entity.id)) {
            entity.id = Random.Range(0, 99999999).ToString();
        }
        if (!entities.Contains(entity)) {
            entities.Add(entity);
        }
    }
    
    //todo mask out factions
    public List<Entity> FindEntitiesInRange(Vector3 origin, float radius, int factionMask = -1) {
        List<Entity> retn = new List<Entity>();
        radius *= radius;
        for(int i = 0; i < entities.Count; i++) {
            if(entities[i].transform.position.DistanceToSquared(origin) <= radius) {
                retn.Add(entities[i]);
            }
        }
        return retn;
    }

    public Entity FindEntity(string entityId) {
        for(int i = 0; i < entities.Count; i++) {
           // Debug.Log(entities[i].gameObject.name + " -> " + entityId);
            if (entities[i].gameObject.name == entityId) return entities[i];
        }
        return null;
    }

    public List<Entity> NearestHostiles(Entity source, float range) {
        List<Entity> retn = new List<Entity>();

        range *= range;
        Transform sourceLocation = source.transform;

        for (int i = 0; i < entities.Count; i++) {
            var ent = entities[i];
            if (ent == source) continue; //todo -- this is all fake until we implement factions
            if(ent.isActiveAndEnabled && ent.transform.DistanceToSquared(sourceLocation) <= range) {
                retn.Add(ent);
            }
        }

        return retn;
    }

    public static EntityManager Instance {
        get {
            instance = instance ?? new EntityManager();
            return instance;
        }
    }

}