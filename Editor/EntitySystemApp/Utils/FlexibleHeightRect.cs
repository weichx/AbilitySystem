using UnityEngine;

public class FlexibleHeightRect {

    float x;
    float y;
    float width;
    float height;
    float lastY;

    public FlexibleHeightRect(Rect rect) {
        x = rect.x;
        y = rect.y;
        width = rect.width;
        height = 0;
        lastY = 0;
        height = 0;
    }

    public Rect Flex(float value) {
        if (value < 0) value = 0;
        Rect retn = new Rect(x, lastY + value, width, value);
        height += value;
        lastY += value;
        return retn;
    }

    public float Height {
        get { return height; }
    }

}