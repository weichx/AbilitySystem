using System;
using System.Collections.Generic;
using AbilitySystem;
using Intelligence;

namespace EntitySystem {
    public class CharacterManager {

        protected List<Character> characters;
        protected Entity entity;

        public CharacterManager(Entity entiy) {
            this.entity = entity;
            characters = new List<Character>();
        }

        public void Update() {

        }
    }
}