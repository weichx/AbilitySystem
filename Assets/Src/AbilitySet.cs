using UnityEngine;

public class AbilitySet : MonoBehaviour {
    public AbilityCreator[] abilities;
}

public class Pointable : System.Attribute { }

public class Test {

    [Pointable]
    public static float Pointable1() { return 0; }

    [Pointable]
    public static float Pointable2() { return 0; }
}