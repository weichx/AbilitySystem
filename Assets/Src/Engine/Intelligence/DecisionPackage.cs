using System.Collections.Generic;
using EntitySystem;

namespace Intelligence {

    public class DecisionPackage : EntitySystemBase {

        public string name;
        public Decision[] decisions;

        public DecisionPackage() {
            decisions = new Decision[0];
        }

    }

}
