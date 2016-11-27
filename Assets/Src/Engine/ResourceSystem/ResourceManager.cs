using System.Collections.Generic;
using EntitySystem.Resources;

namespace EntitySystem {
		
	public class ResourceManager {

	    //todo consider making these matchable / taggable / etc

	    protected Dictionary<string, Resource> resources;
	    protected Entity entity;

	    public ResourceManager(Entity entity) {
	        this.entity = entity;
	        resources = new Dictionary<string, Resource>();
	        //these are temporary, should be configured elsewhere
	        AddResource("Health", new Resource());
	        AddResource("Mana", new Resource());
	    }

	    public bool AddResource(string id, Resource resource) {
	        if (resources.ContainsKey(id)) return false;
	        resources[id] = resource;
	        return true;
	    }

	    public Resource GetResource(string resourceId) {
	        return resources.Get(resourceId);
	    }

	    public bool HasResource(string resourceId) {
	        return resources.ContainsKey(resourceId);
	    }

	    public bool HasResource(Resource resource) {
	        return resources.ContainsValue(resource);
	    }

	    public bool RemoveResource(string resourceId) {
	        return resources.Remove(resourceId);
	    }

	    public bool AddAdjuster(string resourceId, ResourceAdjuster adjuster) {
	        Resource resource = resources.Get(resourceId);
	        if (resource == null) return false;
	        resource.AddAdjuster(adjuster);
	        return true;
	    }

	    public bool HasAdjuster(string resourceId, ResourceAdjuster adjuster) {
	        Resource resource = resources.Get(resourceId);
	        if (resource == null) return false;
	        return resource.HasAdjuster(adjuster);
	    }

	    public bool RemoveAdjuster(string resourceId, ResourceAdjuster adjuster) {
	        Resource resource = resources.Get(resourceId);
	        if (resource == null) return false;
	        return resource.RemoveAdjuster(adjuster);
	    }

	    public Resource this[string resourceId] {
	        get { return resources[resourceId]; }
	        set { resources[resourceId] = value; }
	    }
	}

}