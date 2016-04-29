using UnityEditor;
using UnityEngine;

public class VerticalSpaceDrawable : IRectDrawable {

    protected float space;

    public VerticalSpaceDrawable(float space) {
        this.space = space;
    }

    public void Render(Rect rect) {
        GUILayout.Space(space);
    }

    public float GetHeight() {
        return space;
    }

}