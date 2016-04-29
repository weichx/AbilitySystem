using System;

public enum ActionStatus {
    Running, Failure, Success
}

[Serializable]
public abstract class AIAction {

    public string name;
    public Entity entity;
    //public AIConsideration[] considerations;
    //public AIRequirement[] requirements;

    protected Context context;

    private bool isInitialized = false;

    public void Execute(Context context) {
        this.context = context;
        entity = context.entity;
        if (!isInitialized) {
            isInitialized = true;
            OnInitialize();
        }
        OnStart();
    }

    public virtual ActionStatus OnUpdate() {
        return ActionStatus.Success;
    }

    public virtual void OnInitialize() { }
    public virtual void OnStart() { }
    public virtual void OnSuccess() { }
    public virtual void OnInterrupt() { }
    public virtual void OnFailure() { }
    public virtual void OnEnd() { }

    //public void AddConsideration(AIConsideration consideration) {
    //    Array.Resize(ref considerations, considerations.Length + 1);
    //    considerations[considerations.Length - 1] = consideration;
    //}

    //public void AddRequirement(AIRequirement requirement) {
    //    Array.Resize(ref requirements, requirements.Length + 1);
    //    requirements[requirements.Length - 1] = requirement;
    //}

    
}



