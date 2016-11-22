using UnityEngine;
using EntitySystem;
using System.Collections.Generic;

public class DetailView<T> where T : EntitySystemBase, new() {

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
        GUILayout.BeginVertical();
        for (int i = 0; i < sections.Count; i++) {
            GUILayout.BeginVertical();
            sections[i].Render();
            GUILayout.Space(sections[i].Space);
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

}