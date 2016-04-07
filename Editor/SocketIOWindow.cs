using UnityEditor;
using SocketIO;

public class SocketIOWindow : EditorWindow {

    [MenuItem("SocketIO/Editor Window")]
    public static void Open() {
        GetWindow<SocketIOWindow>();
    }
    private static SocketInterface instance;

    public void OnEnable() {
        if (instance == null) {
            instance = new SocketInterface();
            instance.Connect();
        }
    }

    public void Update() {
        if (instance != null) instance.Update();
    }

    public void OnDisable() {
        if (instance != null) {
            instance.Disconnect();
        }
    }

    public void OnDestroy() {
        if (instance != null) {
            instance.Disconnect();
        }
    }

    public void OnApplicationQuit() {
        if (instance != null) {
            instance.Disconnect();
        }
    }
}

