using System;
using System.Collections.Generic;
using UnityEngine;


public class VerticalLayoutGroup : IRectDrawable {

    public List<IRectDrawable> drawables = new List<IRectDrawable>();

    public void AddDrawable(IRectDrawable drawable) {
        drawables.Add(drawable);
    }

    public void Render(Rect rect) {
        FlexibleHeightRect r = new FlexibleHeightRect(rect);
        for (int i = 0; i < drawables.Count; i++) {
            float height = drawables[i].GetHeight();
            drawables[i].Render(r.Flex(height));
        }

    }

    public float GetHeight() {
        float height = 0;
        for(int i = 0; i < drawables.Count; i++) {
            height += drawables[i].GetHeight();
        }
        return height;
    }

}