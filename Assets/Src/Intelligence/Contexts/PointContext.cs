using UnityEngine;

namespace Intelligence {
	
	public class PointContext : Context {

		public readonly Vector3 point;

		public PointContext(Entity entity, Vector3 point) : base(entity) {
			this.point = point;
		}

	}

}