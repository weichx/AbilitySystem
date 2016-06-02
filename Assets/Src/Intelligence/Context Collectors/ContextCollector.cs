using System.Collections.Generic;

namespace Intelligence {


    public abstract class ContextCollector {

        public abstract List<Context> Collect(CharacterAction<Context> action, Entity entity);

    };

    public abstract class ContextCollector<T> : ContextCollector where T : Context {

        public override List<Context> Collect(CharacterAction<Context> action, Entity entity) {
            return Collect(action, entity) as List<Context>;
        }

        public abstract List<T> Collect(CharacterAction<T> action, Entity entity);

    }

}