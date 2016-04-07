using System;

namespace SkillSystem {

    public abstract class AttributeFormla {

        public abstract float GetValue();

    }

    [Serializable]
    public class FlatValueFormula : AttributeFormla {

        public float baseValue;

        public override float GetValue() {
            return baseValue;    
        }

    }

}