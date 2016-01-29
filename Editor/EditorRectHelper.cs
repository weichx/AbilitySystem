using UnityEngine;


public class EditorRect {

    public Rect baseRect;
    public Rect currentRect;

    public EditorRect(Rect baseRect) {
        this.baseRect = baseRect;
        this.currentRect = baseRect;
    }

    public Rect HorizontalProperty(float percentage) {
        Rect retn = new Rect(currentRect) {
            width = baseRect.width * percentage
        };
        currentRect = new Rect(retn) {
            x = retn.x + retn.width,
            width = baseRect.width - retn.width
        };
        return retn;
    }

    public Rect Width(float amount) {
        amount = Mathf.Clamp(amount, 0, currentRect.width);
        float newX = currentRect.x + amount;
        float newWidth = currentRect.width - newX;

        Rect retn = new Rect(currentRect) {
            width = amount
        };

        currentRect = new Rect(retn) {
            x = retn.x + amount,
            width = baseRect.width - amount //newWidth
        };
        return retn;
    }

    //public Rect StretchWidth(float min, float max) {

    //}

    public Rect WidthMinus(float amount) {
        amount = Mathf.Clamp(amount, 0, baseRect.width);
        Rect rect = new Rect(currentRect) {
            width = currentRect.width - amount
        };
        currentRect = new Rect(rect) {
            x = rect.x + rect.width,
            width = amount
        };
        return rect;
    }

    public static implicit operator Rect(EditorRect editorRect) {
        return editorRect.currentRect;
    }
}