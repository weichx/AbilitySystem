using UnityEngine;

public class FactionManager {
    //todo this is overly simple for now on purpose

    public static bool IsFriendly(Entity one, Entity two) {
        return one.factionId == two.factionId;
    }

    public static bool IsHostile(Entity one, Entity two) {
        return one.factionId != two.factionId;
    }

}