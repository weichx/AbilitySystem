using UnityEngine;

namespace EntitySystemUtil {
    public static class RectUtil {

        public static Rect Shrink(this Rect rect, float amount) {
            return new Rect(rect) {
                x = rect.x + amount,
                y = rect.y + amount,
                width = rect.width - amount,
                height = rect.height - amount
            };
        }

        public static Rect ShrinkTop(this Rect rect, float amount) {
            return new Rect(rect) {
                y = rect.y + amount
            };
        }

        public static Rect ShrinkBottom(this Rect rect, float amount) {
            return new Rect(rect) {
                height = rect.height - amount
            };
        }

        public static Rect ShrinkLeft(this Rect rect, float amount) {
            return new Rect(rect) {
                x = rect.x + amount
            };
        }

        public static Rect ShrinkRight(this Rect rect, float amount) {
            return new Rect(rect) {
                width = rect.width - amount
            };
        }

        public static Rect ShrinkTopBottom(this Rect rect, float amountTop, float amountBottom = 0) {
            if (amountBottom == 0) {
                amountBottom = amountTop;
            }
            return new Rect(rect) {
                y = rect.y + amountTop,
                height = rect.height - amountBottom - amountTop
            };
        }

        public static Rect ShrinkLeftRight(this Rect rect, float amountLeft, float amountRight = 0) {
            if (amountRight == 0) {
                amountRight = amountLeft;
            }
            return new Rect(rect) {
                x = rect.x + amountLeft,
                width = rect.width - amountLeft - amountRight
            };
        }

        public static EditorRect EditorRect(this Rect rect) {
            return new global::EditorRect(rect);
        }

        public static EditorRect EditorRect(this Rect rect, float widthMargin, float heightMargin) {
            return new global::EditorRect(rect, widthMargin, heightMargin);
        }
    }
}