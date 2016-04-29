using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AddStatusEffect : AbilityComponent {

    public GameObject statusObject;
    [SerializeField]
    public Stack<string> tmp;

    public AddStatusEffect() {
        tmp = new Stack<string>();
        tmp.Push("one");
        tmp.Push("two");
        tmp.Push("three");
    }

}