
public struct HorizontalMargin {
    public float left;
    public float right;
    public bool percent;

    public HorizontalMargin(float left, float right, bool percent = false) {
        this.left = left;
        this.right = right;
        this.percent = percent;
    }

}
