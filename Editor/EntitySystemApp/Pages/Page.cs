using UnityEngine;
using UnityEditor;

public interface IRectDrawable {
    void Render(Rect rect);
    float GetHeight();
}

public abstract class Page : IRectDrawable {

    public virtual void Initialize() { }
    public abstract void Render(Rect rect);
    public abstract float GetHeight();

    public static float LabelWidth = EditorGUIUtility.labelWidth;
    public static float LineHeight = EditorGUIUtility.singleLineHeight;
}