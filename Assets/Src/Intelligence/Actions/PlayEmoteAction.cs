using System;
using UnityEngine;

namespace Intelligence {
	
	[Serializable]
	public class PlayEmote : CharacterAction<PointContext> {
		public GameObject emote;
	}

}