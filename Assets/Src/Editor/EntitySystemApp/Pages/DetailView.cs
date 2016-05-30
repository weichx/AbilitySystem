using  UnityEngine;
using System.Collections.Generic;

public class DetailView<T> where T : EntitySystemBase {

    protected List<SectionBase<T>> sections;
    private Vector2 scrollPosition;

    public DetailView() {
        sections = new List<SectionBase<T>>();
    }

    public void SetTargetObject(AssetItem<T> assetItem) {
        for (int i = 0; i < sections.Count; i++) {
            sections[i].SetTargetObject(assetItem);
        }
    }

    public virtual void Render() {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < sections.Count; i++) {
            GUILayout.BeginVertical();
            sections[i].Render();
            GUILayout.Space(sections[i].Space);
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();
    }

}