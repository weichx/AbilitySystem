using UnityEngine;
using System.Collections.Generic;

namespace Intelligence {

	public class MultiPointContext : Context {

		public readonly List<Vector3> points;

		public MultiPointContext(Entity entity, List<Vector3> points) : base(entity) {
			this.points = points ?? new List<Vector3>();
		}
			
	}

}