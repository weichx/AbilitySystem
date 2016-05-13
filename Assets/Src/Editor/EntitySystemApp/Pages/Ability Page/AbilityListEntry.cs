using System;

public class AbilityListEntry {
    public bool isSelected;
    public string name;
    public Type scriptableType;
    public Ability ability;
    private string assetpath;
    private AbilityCreator wrapper;

    public AbilityListEntry(string name, AbilityCreator wrapper, string assetpath) {
        this.name = name;
        this.wrapper = wrapper;
        this.assetpath = assetpath;
    }

    public AbilityCreator Wrapper {
        get { return wrapper; }
    }

    public string Name {
        get {
            return (ability != null) ? ability.abilityId : name;
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
            return wrapper.source;
        }
    }
}