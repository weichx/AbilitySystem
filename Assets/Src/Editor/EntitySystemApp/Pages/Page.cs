using UnityEngine;
using UnityEditor;

public interface IRectDrawable {
    void Render(Rect rect);
    float GetHeight();
}

public abstract class Page {

    public virtual void OnEnter() { }
    public abstract void Render(Rect rect);
    public virtual void Update() { }
    public virtual void OnExit() { }
    public static float LabelWidth = EditorGUIUtility.labelWidth;
    public static float LineHeight = EditorGUIUtility.singleLineHeight;
}