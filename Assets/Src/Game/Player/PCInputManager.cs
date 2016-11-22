using UnityEngine;

public class PCInputManager {

    public static bool RightMousePressed {
        get { return Input.GetMouseButton(1); }
    }

    public static bool LeftMousePressed {
        get { return Input.GetMouseButton(0); }
    }

    public static bool Forward {
        get { return Input.GetKey(KeyCode.W); }
    }

    public static bool Strafing {
        get { return Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E); }
    }

    public static bool StrafingLeft {
        get { return Input.GetKey(KeyCode.Q); }
    }

    public static bool StrafingRight {
        get { return Input.GetKey(KeyCode.E); }
    }

    public static bool ForwardStrafing {
        get { return Strafing && Forward; }
    }

    public static bool Backward {
        get { return Input.GetKey(KeyCode.S); }
    }

}
