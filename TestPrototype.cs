using UnityEngine;
using System.Collections;
using AbilitySystem;

public class TestPrototype : MonoBehaviour {
    bool ran = false;
    // Use this for initialization
    void Start() {
        if (ran) return;
        ran = true;
        var player = GameObject.Find("Player");
        var abi = player.GetComponent<AbilityManager>();
        var abSelf = GetComponent<AbilityManager>();
        var myMeteor = abSelf.GetAbility("Meteor");
        var playerMeteor = abi.GetAbility("Meteor");

    }
}
