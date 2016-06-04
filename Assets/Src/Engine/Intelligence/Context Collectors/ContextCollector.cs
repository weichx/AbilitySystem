using System.Collections.Generic;

namespace Intelligence {


    public abstract class ContextCollector {

        public abstract List<Context> Collect(CharacterAction action, Entity entity);

    };

    public abstract class ContextCollector<T> : ContextCollector where T : Context {

        public override List<Context> Collect(CharacterAction action, Entity entity) {
            return Collect(action as CharacterAction<T>, entity);
        }

        public abstract List<Context> Collect(CharacterAction<T> action, Entity entity);

    }

}