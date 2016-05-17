using System;

public class EntityListEntry {
    public bool isSelected;
    public string name;
    public Type scriptableType;
    public EntityTemplate template;
    private string assetpath;
    //private EntityCreator wrapper;

    //public EntityListEntry(string name, EntityCreator wrapper, string assetpath) {
    //    this.name = name;
    //    this.wrapper = wrapper;
    //    this.assetpath = assetpath;
    //}

    //public EntityCreator Wrapper {
    //    get { return wrapper; }
    //}

    //public string Name {
    //    get { 
    //        return (ability != null) ? ability.abilityId : name;
    //    }
    //}

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
            return "";// wrapper.source;
        }
    }
}