using Intelligence;

namespace EntitySystem.Resources {
	
	//todo maybe this is an interface
	public abstract class ResourceAdjuster {

	    public string resourceId;

	    public abstract float Adjust(float delta, Resource resource, Context context);

	}

}