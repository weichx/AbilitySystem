using System;
using System.Collections.Generic;
using UnityEngine;

public class FlexibleWidthRect {
    float x;
    float y;
    float width;
    float height;
    float lastX;

    public FlexibleWidthRect(Rect rect) {
        x = rect.x;
        y = rect.y;
        width = rect.width;
        height = 0;
        lastX = x;
        height = 0;
    }

    public Rect Flex(float value) {
        if (value < 0) value = 0;
        height += value;
        Rect retn = new Rect(lastX, y, value, height);
        lastX += value;
        return retn;
    }

    public float Width {
        get { return width; }
    }
}