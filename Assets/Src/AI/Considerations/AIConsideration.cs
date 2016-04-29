
public abstract class AIConsideration {
    public string name;
    public ResponseCurve curve;
    public abstract float Score(Context context);
}

