using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_DetailView {

    private StatusPage page;
    private SerializedObject target;
    private SerializedProperty statusProperty;
    private StatusPage_NameSection nameSection;
    private StatusPage_GeneralSection generalSection;
    private StatusPage_ComponentSection componentSection;

    public StatusPage_DetailView(StatusPage page) {
        this.page = page;
        nameSection = new StatusPage_NameSection(page);
        generalSection = new StatusPage_GeneralSection(page);
        componentSection = new StatusPage_ComponentSection(page);
    }

    public void SetTargetObject(SerializedObject target) {
        this.target = target;
        if(target != null) {
            statusProperty = target.FindProperty("statusEffect");
        }
        else {
            statusProperty = null;
        }
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
