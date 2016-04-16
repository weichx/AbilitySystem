using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldSpaceUIOverlay : MonoBehaviour {
    public Transform trackedTransform;
    protected Vector3? trackedPosition;
    protected Graphic[] graphics;
    protected RectTransform rectTransform;
    public Vector3 offset;

    private static float guiScale = 1.0f;
    private static bool isScaleInitialized;

    public virtual void Start() {
        rectTransform = GetComponent<RectTransform>();
        graphics = GetComponentsInChildren<Graphic>();
        if (!isScaleInitialized) {
            var scaler = GetComponentInParent<CanvasScaler>();
            Assert.IsNotNull(scaler, "Missing CanvasScaler Component");
            if (Mathf.Approximately(scaler.matchWidthOrHeight, 0.0f)) {
                guiScale = scaler.referenceResolution.x / (float)Screen.width;
            }
            else if (Mathf.Approximately(scaler.matchWidthOrHeight, 1.0f)) {
                guiScale = scaler.referenceResolution.y / (float)Screen.height;
            }
            else {
                Debug.LogWarning("Canvas scales between width and height are not supported by WorldSpaceUIOverlay");
            }
            isScaleInitialized = true;
        }
        LateUpdate();
    }

    public virtual void LateUpdate() {

        if (trackedTransform == null && trackedPosition == null) {
            for (int i = 0; i < graphics.Length; i++) {
                graphics[i].enabled = false;
            }
            return;
        }

        Vector3 position;

        if (trackedTransform) {
            position = trackedTransform.position + offset;
        }
        else {
            position = (Vector3)trackedPosition + offset;
        }

        Assert.IsNotNull(Camera.main, "Camera is null");
        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);
        bool visible = screenPos.z > 0.0f;
        for (int i = 0; i < graphics.Length; i++) {
            graphics[i].enabled = visible;
        }
        //weird interaction with canvas scaler here, things get weirdly small when scaled down
        //just keep this constant for now
        guiScale = 1;      
        rectTransform.anchoredPosition = new Vector3(guiScale * screenPos.x, guiScale * screenPos.y, 0);
    }

    public void SetTrackedPosition(Vector3? position) {
        trackedPosition = position;
        trackedTransform = null;
    }

    public void SetTrackedObject(GameObject target) {
        if (target != null && target.transform != trackedTransform) {
            trackedTransform = target.transform;
            trackedPosition = null;
        }
    }

    public void SetTrackedObject(Transform target) {
        if (target != trackedTransform) {
            trackedTransform = target;
            trackedPosition = null;
        }
    }

    public void Untrack() {
        trackedTransform = null;
        trackedPosition = null;
        for (int i = 0; i < graphics.Length; i++) {
            graphics[i].enabled = false;
        }
    }

    public void OnValidate() {
        rectTransform = GetComponent<RectTransform>();
        graphics = GetComponentsInChildren<Graphic>();
        LateUpdate();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldSpaceUIOverlay))]
public class OverlayInspector :  Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Update To Transform")) {
            (target as WorldSpaceUIOverlay).OnValidate();
        }
    }
}
#endif