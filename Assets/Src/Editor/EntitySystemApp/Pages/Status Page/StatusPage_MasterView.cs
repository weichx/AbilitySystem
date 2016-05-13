using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class StatusPage_MasterView : StatusPage_SectionBase {

    private string searchString;
    private List<StatusListEntry> filteredList;
    private List<StatusListEntry> statusList;
    private Vector2 scrollPosition;

    public StatusPage_MasterView(StatusPage page) : base(page) {
        searchString = "";
        statusList = new List<StatusListEntry>();
        LoadFiles();
    }

    public override void Render() {
        GUILayout.BeginVertical(GUILayout.MaxWidth(200f));
        GUILayout.Space(10f);
        RenderSearchArea();
        GUILayout.Space(5f);
        RenderStatusList();
        GUILayout.EndVertical();
    }

    public void RemoveEntry(StatusListEntry entry) {
        statusList.Remove(entry);
        if(filteredList != null) {
            filteredList.Remove(entry);
        }    
    }

    private void RenderSearchArea() {
        string current = searchString;
        GUILayout.BeginHorizontal();
        GUIStyle toolbarStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
        GUIStyle toolbarCancelStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));

        searchString = GUILayout.TextField(searchString, toolbarStyle, GUILayout.ExpandWidth(true));

        if (searchString != current) {
            filteredList = statusList.FindAll((entry) => {
                return entry.Name.Contains(searchString);
            });
        }
        else if (searchString == "") {
            filteredList = statusList;
        }

        if (GUILayout.Button("", toolbarCancelStyle)) {
            searchString = "";
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        if (GUILayout.Button("New Status Effect")) {
            CreateNewStatusEffect();
        }
    }

    private void CreateNewStatusEffect() {
        StatusEffectCreator wrapper = StatusEffectMenuItem.CreateScriptableObject();
        string assetpath = AssetDatabase.GetAssetPath(wrapper);
        statusList.Add(new StatusListEntry(wrapper.name, wrapper, assetpath));
        page.SetActiveStatusEffect(statusList.Last());
    }

    private void RenderStatusList() {
        GUIStyle listStyle = new GUIStyle(GUI.skin.box) {
            margin = new RectOffset() { top = 3 }
        };
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        Color original = GUI.backgroundColor;
        for (int i = 0; i < filteredList.Count; i++) {
            StatusListEntry entry = filteredList[i];
            GUI.backgroundColor = entry.isSelected ? Color.green : original;
            if (GUILayout.Button(filteredList[i].Name, listStyle, GUILayout.ExpandWidth(true))) {
                page.SetActiveStatusEffect(filteredList[i]);
            }
        }
        GUI.backgroundColor = original;
        GUILayout.EndScrollView();
    }

    private void LoadFiles() {
        string[] guids = AssetDatabase.FindAssets("t:StatusEffectWrapper");
        for(int i = 0; i < guids.Length; i++) {
            string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(assetpath);
            StatusEffectCreator item = AssetDatabase.LoadAssetAtPath(assetpath, typeof(StatusEffectCreator)) as StatusEffectCreator;
            statusList.Add(new StatusListEntry(name, item, assetpath));
        }
    }
}