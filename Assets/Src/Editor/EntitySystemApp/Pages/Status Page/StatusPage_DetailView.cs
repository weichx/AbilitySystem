using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_DetailView {

    private AssetItem<StatusEffect> target;
    private StatusPage_NameSection nameSection;
    private StatusPage_GeneralSection generalSection;
    private StatusPage_ComponentSection componentSection;

    public StatusPage_DetailView() {
        nameSection = new StatusPage_NameSection();
        generalSection = new StatusPage_GeneralSection();
        componentSection = new StatusPage_ComponentSection();
    }

    public void SetTargetObject(AssetItem<StatusEffect> target) {
        this.target = target;
        nameSection.SetTargetObject(target);
        generalSection.SetTargetObject(target);
        componentSection.SetTargetObject(target);
    }

    public void Render() {
        if (target == null) return;
        GUILayout.BeginVertical();
        nameSection.Render();
        GUILayout.EndVertical();
        GUILayout.Space(20f);
        GUILayout.BeginVertical();
        generalSection.Render();
        GUILayout.EndVertical();
        GUILayout.Space(20f);
        GUILayout.BeginVertical();
        componentSection.Render();
        GUILayout.EndVertical();
    }

}
