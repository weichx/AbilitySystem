using UnityEngine;

public class EditorRect {

    public Rect baseRect;
    public Rect currentRect;
    public float widthMargin;
    public float heightMargin;

    public EditorRect(Rect baseRect, float widthMargin = 0, float heightMargin = 0) {
        this.baseRect = baseRect;
        this.currentRect = baseRect;
        this.widthMargin = widthMargin;
        this.heightMargin = heightMargin;
    }

    public EditorRect(float width, float height, Rect input) {
        baseRect = new Rect(input) {
            width = width,
            height = height
        };
        currentRect = baseRect;
    }

    public Rect VerticalSlice(float slice) {
        Rect retn = new Rect(currentRect) {
            height = slice
        };
        currentRect = new Rect(currentRect) {
            y = currentRect.y + slice + heightMargin,
            height = currentRect.height - slice - heightMargin
        };
        return retn;
    }

    public Rect VerticalSliceTo(float blockSize) {
        return VerticalSlice(currentRect.height - blockSize);
    }

    public Rect HorizontalSlice(float slice) {
        Rect retn = new Rect(currentRect) {
            width = slice
        };
        currentRect = new Rect(currentRect) {
            x = currentRect.x + slice + widthMargin,
            width = currentRect.width - slice - widthMargin
        };
        return retn;
    }

    public Rect HorizontalSlicePercent(float slice) {
        return HorizontalSlice(slice * currentRect.width);
    }

    //todo deprecate
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
        amount = Mathf.Clamp01(amount);

        Rect retn = new Rect(currentRect) {
            width = amount * baseRect.width
        };

        currentRect = new Rect(retn) {
            x = retn.x + amount,
            width = baseRect.width - amount
        };
        return retn;
    }

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

    public Rect ShrinkBottom(float amount) {
        currentRect = new Rect(currentRect) {
            height = currentRect.height - amount
        };
        return currentRect;
    }

    public Rect ShrinkLeftRight(float amountLeft, float amountRight = 0) {
        if (amountRight == 0) {
            amountRight = amountLeft;
        }
        currentRect = new Rect(currentRect) {
            x = currentRect.x + amountLeft,
            width = currentRect.width - amountLeft - amountRight
        };
        return currentRect;
    }

    public Rect Shrink(float amount) {
        currentRect = new Rect(currentRect) {
            x = currentRect.x + amount,
            y = currentRect.y + amount,
            width = currentRect.width - amount - amount,
            height = currentRect.height - amount - amount
        };
        return currentRect;
    }

    public static implicit operator Rect(EditorRect editorRect) {
        return editorRect.currentRect;
    }
}