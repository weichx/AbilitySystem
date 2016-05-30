using System;
using UnityEngine;

namespace Intelligence {

    public enum TestEnumber {
        One,
        Two,
        Three
    }

    public struct Stuff {
        public float val;
        public string val2;
    }

    [Serializable]
    public class MyTargetWithinRadius : Requirement {
        public TestEnumber e;
        public float radius;
        public MethodPointer<float, float> ptr;

        public bool toggle;
        public string[] strArray;
        public Stuff[] vectors;
        public Stuff stuff = new Stuff();

        public override bool Check(Context context) {
            SingleTargetContext ctx = context as SingleTargetContext;

            Entity me = ctx.entity;
            Entity him = ctx.target;

            return me.transform.DistanceToSquared(him.transform.position) <= radius * radius;
        }

        [Pointable]
        public static float Test(float val) {
            return 0;
        }

    }

}