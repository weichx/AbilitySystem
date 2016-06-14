using System.Collections.Generic;

namespace Intelligence {

	public class SingleTargetContext : Context {

		public readonly Entity target;

		public SingleTargetContext(Entity entity, Entity target) : base(entity) {
			this.target = target;
		}

	    public bool IsReady() {
	        return entity.GetComponent<TargetManager>().currentTarget != null;
	    }
	}

}