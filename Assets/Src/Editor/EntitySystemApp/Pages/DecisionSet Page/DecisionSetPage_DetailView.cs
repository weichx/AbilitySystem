//using UnityEngine;
//using UnityEditor;

//public class DecisionSetPage_DetailView {

//    private Vector2 scrollPosition;
//    private AssetItem<DecisionSet> target;
//    private DecisionSetPage_NameSection nameSection;
//	private DecisionSetPage_DecisionList skillList;

//    public DecisionSetPage_DetailView() {
//        nameSection = new DecisionSetPage_NameSection();
//		skillList = new DecisionSetPage_DecisionList();
//    }

//    public void Render() {
//        //if (target == null || target.SerializedObject == null) return;
//        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
//        GUILayout.BeginVertical();
//        nameSection.Render();
//        GUILayout.EndVertical();
//        GUILayout.Space(20f);
//		GUILayout.BeginVertical();
//		skillList.Render();
//		GUILayout.EndVertical();
//		GUILayout.Space(20f);
//        GUILayout.EndScrollView();
//    }

//    public void SetTargetObject(AssetItem<DecisionSet> targetItem) {
//        target = targetItem;
//        nameSection.SetTargetObject(target);
//		skillList.SetTargetObject(target);
//    }
//}