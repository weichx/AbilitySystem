using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EntityManager {

    public List<Entity> entities = new List<Entity>();
    private static EntityManager instance;
    private static Entity implicitEntity;

    //awake is not called on disable entities, might need to do a FindGameObjectWithComponent<Entity> or something to 
    //get all the entities in a scene
    public static Entity ImplicitEntity {
        get {
            if(implicitEntity == null) {
                implicitEntity = Util.FindOrCreateByName("SystemEntity", new Type[] {
                    typeof(Entity)
                }).GetComponent<Entity>();
            }
            return implicitEntity;
        }
    }

    public void Register(Entity entity) {
        if (string.IsNullOrEmpty(entity.id)) {
            entity.id = Random.Range(0, 99999999).ToString();
        }
        if (!entities.Contains(entity)) {
//            EntityTemplate template = EntitySystemLoader.Instance.Create<EntityTemplate>(entity.TemplatePath);
//            if(template != null) {
//                template.Apply(entity);
//                template = null;
//            }
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
            instance.Register(ImplicitEntity);
            EntitySystemLoader.Instance.StartLoad();
            return instance;
        }
    }

}