using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDatabase {

    private static AbilityDatabase instance;

    private Dictionary<string, string> abilityMap;

    private AbilityDatabase() {
        abilityMap = new Dictionary<string, string>();
    }

    public Ability Create(string abilityId) {
        var abilityJSON = abilityMap.Get(abilityId);
        if (abilityJSON == null) throw new AbilityMissingException(abilityId);
        return MiniJSON.Json.Deserialize<Ability>(abilityJSON);
    }

    public void Add(string abilityId, string abilityJSON) {
        if(abilityMap.ContainsKey(abilityId)) {
            throw new DuplicateAbilityException(abilityId);
        }
        abilityMap[abilityId] = abilityJSON;
    }

    static AbilityDatabase() {
        instance = new AbilityDatabase();
        instance.Load();
    }

    private void Load() {
        UnityEngine.Object[] assets = Resources.LoadAll("Ability Definitions");
        for (int i = 0; i < assets.Length; i++) {
            TextAsset asset = assets[i] as TextAsset;
            Add(asset.name, asset.text);
        }
    }

    public static AbilityDatabase Instance {
        get { return instance; }
    }

    private class DuplicateAbilityException : Exception {

        public DuplicateAbilityException(string message) : base(message) { }

    }
}