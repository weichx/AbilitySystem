using UnityEditor;
using UnityEngine;

public class AbilityPage_DetailView {

    private Vector2 scrollPosition;
    private AbilityPage_NameSection nameSection;
    private AbilityPage_GeneralSection generalSection;
    private AbilityPage_ComponentSection componentSection;
    private AbilityPage_RequirementSection requirementSection;
    private AssetItem<Ability> target;

    public AbilityPage_DetailView() {
        nameSection = new AbilityPage_NameSection();
        generalSection = new AbilityPage_GeneralSection();
        componentSection = new AbilityPage_ComponentSection();
        requirementSection = new AbilityPage_RequirementSection();
    }

    public void SetTargetObject(AssetItem<Ability> target) {
        this.target = target;
        nameSection.SetTargetObject(target);
        generalSection.SetTargetObject(target);
        componentSection.SetTargetObject(target);
        requirementSection.SetTargetObject(target);
    }

    public void Render() {
        if (target == null || target.SerializedObject == null) return;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
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
        GUILayout.Space(20f);
        GUILayout.BeginVertical();
        requirementSection.Render();
        GUILayout.Space(50f);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }
}