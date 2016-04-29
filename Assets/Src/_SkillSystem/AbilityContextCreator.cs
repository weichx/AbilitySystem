
public abstract class AbilityContextCreator {

    public Context context;

    public bool IsContextReady { get; private set; }

    public virtual void Initialize() {

    }

    public void Start() {
        context = new Context();
    }

    public void Update() {

    }

    public void Cancel() {

    }

    public void Interrupt() {

    }

    public void End() {

    }

    public void SetContext(Context context) {
        this.context = context;
    }

    public void Ready() {
        IsContextReady = true;
    }
}
