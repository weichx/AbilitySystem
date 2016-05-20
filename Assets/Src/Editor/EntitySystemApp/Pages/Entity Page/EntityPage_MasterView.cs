using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EntityPage_MasterView {

    public List<EntityListEntry> entityList;

    private EntityPage page;
    private string searchString;
    private List<EntityListEntry> filteredList;

    public EntityPage_MasterView(EntityPage page) {
        this.page = page;
    }

    public void Render() {

    }

    private void RenderSearchArea() {
        string current = searchString;
        GUILayout.BeginHorizontal();
        GUIStyle toolbarStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
        GUIStyle toolbarCancelStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));

        searchString = GUILayout.TextField(searchString, toolbarStyle, GUILayout.ExpandWidth(true));

        if (searchString != current) {
            filteredList = entityList.FindAll((entry) => {
                return false;// entry.Name.Contains(searchString);
            });
        }
        else if (searchString == "") {
            filteredList = entityList;
        }

        if (GUILayout.Button("", toolbarCancelStyle)) {
            searchString = "";
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        if (GUILayout.Button("New Status Effect")) {
            //CreateNewEntity();
        }
    }
}