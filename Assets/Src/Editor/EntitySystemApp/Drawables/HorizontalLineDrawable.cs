using UnityEditor;
using UnityEngine;
using EntitySystemUtil;

public class HorizontalLineDrawable : IRectDrawable {

    public void Render(Rect rect) {
        RectGUIUtil.DrawHLine(new Vector2(0, rect.y), rect.width, Color.grey, 1f);
    }

    public float GetHeight() {
        return 20f;
    }
}