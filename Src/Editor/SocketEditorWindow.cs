using UnityEditor;
using UnityEngine;

public class SocketEditorWindow : EditorWindow {
	private static SocketManager socket;

	[MenuItem("Sockets/Window")]
	public static void Show(){ 
		GetWindow<SocketEditorWindow>();
	}

	public void OnEnable() {
		socket = new SocketManager();
	}

	public void Update() {
		socket.Update();
	}

	public void OnDisable() {
		socket.Disconnect();
	}

	public void OnDestroy() {
		socket.Disconnect();
	}

	public void OnApplicationQuit() {
		socket.Disconnect();
	}
}