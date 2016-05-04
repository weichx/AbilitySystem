using System;

public class StatusListEntry {
    public bool isSelected;
    public string name;
    public Type scriptableType;
    public StatusEffect statusEffect;
    private string assetpath;
    private StatusEffectWrapper wrapper;

    public StatusListEntry(string name, StatusEffectWrapper wrapper, string assetpath) {
        this.name = name;
        this.wrapper = wrapper;
        this.assetpath = assetpath;
    }

    public StatusEffectWrapper Wrapper {
        get { return wrapper; }
    }

    public string Name {
        get {
            return (statusEffect != null) ? statusEffect.statusEffectId : name;
        }
    }
    public string FilePath {
        get {
            return assetpath;
        }
        set {
            assetpath = value;
        }
    }

    public string Source {
        get {
            return wrapper.statusSource;
        }
    }
}