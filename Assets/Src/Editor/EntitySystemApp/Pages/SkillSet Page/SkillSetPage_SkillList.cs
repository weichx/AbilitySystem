using UnityEngine;
using UnityEditor;

public class SkillSetPage_SkillList : SkillSetPage_SectionBase {

    public void Render() {
        if (serialRoot == null) return;
       
        GUILayout.BeginVertical();
        {
            for(int i = 0; i < skillSet.skillList.Count; i++) {
                RenderSkill(skillSet.skillList[i]);
            }

        }
        GUILayout.EndVertical();
    }

    private void RenderSkill(DecisionEvaluator skillEvaluator) {

    }

}