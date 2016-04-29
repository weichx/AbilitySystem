using UnityEngine;
using UnityEditor;

public class VerticalRectDivider {

    int divisions;
    int currentDivision;
    float height;
    Rect current;
    float lastY;

    public VerticalRectDivider(Rect rect, int divisions = 1) {
        this.divisions = Mathf.Clamp(divisions, 1, 100);
        currentDivision = 0;
        height = rect.width * (1f / divisions);
        current = new Rect(rect) {
            width = height
        };
        lastY = height;
    }

    public Rect Next() {
        if (currentDivision == divisions - 1) {
            throw new System.Exception("Trying to use rect division more times than it was divided");
        }
        currentDivision++;

        Rect retn = current;
        current = new Rect(current) {
            y = lastY
        };
        lastY += height;
        return retn;
    }

    public static implicit operator Rect(VerticalRectDivider d) {
        return d.Next();
    }
}
