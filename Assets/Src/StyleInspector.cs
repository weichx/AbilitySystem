using  UnityEngine;

[ExecuteInEditMode]
public class StyleInspector : MonoBehaviour {

    public GUIStyle style;
    public GUISkin skin;
    public Bounds bounds;
    public Vector3 position;
    public Rect rect;
    public string[] strArray;

    private static GUIStyle _style;
    public static GUIStyle Style {
        get
        {
            return _style;
        }
        set {
            _style = value;
        }
    }

    private static StyleInspector instance;

    public void Awake() {
        if (instance == null) {
            instance = this;
            instance.style = _style;
        }
    }

    public void Update() {
        if (instance == null) instance = this;
        instance.style = _style;
    }

}