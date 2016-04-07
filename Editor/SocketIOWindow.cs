using UnityEditor;
using UnityEngine;
using AbilitySystem;
using System;
using System.Collections.Generic;
using SocketIO;

public class SocketIOWindow : EditorWindow {

    [MenuItem("SocketIO/Editor Window")]
    public static void Open() {
        GetWindow<SocketIOWindow>();
    }
    private static SocketInterface instance;

    public void OnEnable() {
        bool wasNull = instance == null;
        if (instance == null) {
            instance = new SocketInterface();
        }
        instance.Connect();
        EditorApplication.update += OnUpdate;
        if (wasNull) {
            instance.On("AIConsiderationTypes_Request", EmitConsiderationTypes);
        }
        Debug.Log("Connected!");
    }

    public void EmitConsiderationTypes(SocketIOEvent evt) {
        Debug.Log("got event");
        List<Type> types = Reflector.FindSubClasses<AIConsideration>();
        string output = "";
        for (int i = 0; i < types.Count - 1; i++) {
            output += types[i].Name;
            output += ",";
        }
        output += types[types.Count - 1].Name;
        Debug.Log(output);
        instance.Emit("AIConsiderationTypes_Response", JSONObject.StringObject(output));
    }

    public void OnUpdate() {
        if (instance != null) instance.Update();
    }

    public void OnDisable() {
        if (instance != null) {
            instance.Disconnect();
            instance.Off("AIConsiderationTypes_Request", EmitConsiderationTypes);
        }
        EditorApplication.update -= OnUpdate;
    }

    public void OnDestroy() {
        if (instance != null) {
            instance.Disconnect();
        }
        EditorApplication.update -= OnUpdate;
    }

    public void OnApplicationQuit() {
        if (instance != null) {
            instance.Disconnect();
        }
        EditorApplication.update -= OnUpdate;
    }

    public void OnGUI() {
        if (instance == null) return;
        if (instance.IsConnected) {
            EditorGUILayout.LabelField("Socket connected");
        }
        else {
            EditorGUILayout.LabelField("Socket not connected");
            
        }
        if (GUILayout.Button("Reconnect")) {
            instance.Disconnect();
            OnEnable();
        }
    }
}

