
public abstract class AbilityContextCreator {

    public OldContext context;

    public bool IsContextReady { get; private set; }

    public virtual void Initialize() {

    }

    public void Start() {
        context = new OldContext();
    }

    public void Update() {

    }

    public void Cancel() {

    }

    public void Interrupt() {

    }

    public void End() {

    }

    public void SetContext(OldContext context) {
        this.context = context;
    }

    public void Ready() {
        IsContextReady = true;
    }
}
