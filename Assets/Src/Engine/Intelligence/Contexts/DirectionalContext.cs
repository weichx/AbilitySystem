using UnityEngine;

namespace Intelligence {

	public class DirectionalContext : Context {
	
		public readonly Vector3 direction; //assume normalized

		public DirectionalContext(Entity entity, Vector3 normalizedDirection) : base(entity) {
			direction = normalizedDirection;
		}

	}

}