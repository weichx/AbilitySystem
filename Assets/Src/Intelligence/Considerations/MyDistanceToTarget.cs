using System;

namespace Intelligence {

    [Serializable]
    public class MyDistanceToTarget : Consideration {

        public float maxDistance;

        public override float Score(Context context) {
            return 11;
        }

    }
}