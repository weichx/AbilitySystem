using UnityEngine;
using UnityEditor;

public class HorizontalRectLayout {

    int divisions;
    int currentDivision;
    float divisionWidth;
    Rect current;
    float lastX;
    public HorizontalRectLayout(Rect rect, int divisions) {
        this.divisions = Mathf.Clamp(divisions, 1, 100);
        currentDivision = 0;
        divisionWidth = rect.width * (1f / divisions);
        current = new Rect(rect) {
            width = divisionWidth
        };
    }

    public Rect Next() {
        if (currentDivision == divisions) {
            throw new System.Exception("Trying to use rect division more times than it was divided");
        }
        currentDivision++;

        Rect retn = current;
        current = new Rect(current) {
            x = retn.x + divisionWidth
        };
        return retn;
    }

    public static implicit operator Rect(HorizontalRectLayout d) {
        return d.Next();
    }
}
