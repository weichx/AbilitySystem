using UnityEngine;
using EntitySystem;
using System.Collections.Generic;

namespace Intelligence {
	
	public class Context {

		public readonly Entity entity;

		public Context(Entity entity) {
			this.entity = entity;
		}

	}

}