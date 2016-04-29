using UnityEngine;

public class FlexibleSpaceDrawable : IRectDrawable {

    public void Render(Rect rect) {
        GUILayout.FlexibleSpace();
    }

    public float GetHeight() {
        return 0;
    }

}