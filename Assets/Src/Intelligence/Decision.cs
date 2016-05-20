using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelligence {

    public class Decision {
        public float score;
        public ContextBuilder contextBuilder;
        public CharacterAction action;
        public DecisionScoreEvaluator dse;
    }

}
