using UnityEngine;
using UnityEditor;

public class SkillSetPage_DetailView {

    private Vector2 scrollPosition;
    private AssetItem<SkillSet> target;
    private SkillSetPage_NameSection nameSection;

    public SkillSetPage_DetailView() {
        nameSection = new SkillSetPage_NameSection();
    }

    public void Render() {
        //if (target == null || target.SerializedObject == null) return;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginVertical();
        nameSection.Render();
        GUILayout.EndVertical();
        GUILayout.Space(20f);
        GUILayout.EndScrollView();
    }

    public void SetTargetObject(AssetItem<SkillSet> targetItem) {
        target = targetItem;
        nameSection.SetTargetObject(target);
    }
}