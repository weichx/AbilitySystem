

public class Faction {

    public readonly string name;

    public bool IsFriendly(Faction other) {
        return other == this;
    }

    public bool IsNeutral(Faction other) {
        return false;
    }

    public bool IsHostile(Faction other) {
        return true;
    }
}