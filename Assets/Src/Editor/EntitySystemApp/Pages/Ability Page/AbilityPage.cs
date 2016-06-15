using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class AbilityPage : MasterDetailPage<Ability> {

    public AbilityPage() : base() {
        detailView = new AbilityPage_DetailView();
    }
   
}
