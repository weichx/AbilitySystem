
public partial class Ability {

    protected void OnUse() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnUse();
        }
    }

    protected void OnChargeConsumed() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChargeConsumed();
        }
    }

    protected void OnCastStarted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastStarted();
        }
    }

    protected void OnCastUpdated() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastUpdated();
        }
    }

    protected void OnCastInterrupted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastInterrupted();
        }
    }

    protected void OnCastCompleted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastCompleted();
        }
    }

    protected void OnCastCancelled() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastCancelled();
        }
    }

    protected void OnCastFailed() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastFailed();
        }
    }

    protected void OnCastEnded() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnCastEnded();
        }
    }

    protected void OnChannelStart() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelStart();
        }
    }

    protected void OnChannelUpdated() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelUpdated();
        }
    }

    protected void OnChannelTick() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelTick();
        }
    }

    protected void OnChannelInterrupted() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelInterrupted();
        }
    }

    protected void OnChannelCancelled() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelCancelled();
        }
    }

    protected void OnChannelEnd() {
        for (int i = 0; i < components.Count; i++) {
            components[i].OnChannelEnd();
        }
    }

}

