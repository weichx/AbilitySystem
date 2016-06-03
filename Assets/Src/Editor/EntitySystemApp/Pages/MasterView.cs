using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MasterView<T> where T : EntitySystemBase, new() {

    protected string searchString;
    protected string assetQuery;
    protected Vector2 scrollPosition;
    protected List<AssetItem<T>> itemList;
    protected List<AssetItem<T>> filteredList;
    protected Action<AssetItem<T>> SetActiveItem;
    protected string NewButtonText;

    public MasterView(Action<AssetItem<T>> onItemSelected) {
        SetActiveItem = onItemSelected;
        assetQuery = "t:" + typeof(T).Name + "Creator";
        searchString = "";
        itemList = new List<AssetItem<T>>();
        LoadFiles();
        NewButtonText = "New " + Util.SplitAndTitlize(typeof(T).Name);
    }

    public void AddItem(AssetItem<T> item) {
        if (itemList.Contains(item)) return;
        itemList.Add(item);
        if (!string.IsNullOrEmpty(searchString)) {
            filteredList = itemList.FindAll((current) => {
                return current.Name.Contains(searchString);
            });
        }
    }

    public void SelectItemById(string itemId) {
        for (int i = 0; i < itemList.Count; i++) {
            if (itemList[i].Name == itemId && SetActiveItem != null) {
                SetActiveItem(itemList[i]);
                return;
            }
        }
    }

    public void RemoveItem(AssetItem<T> item) {
        itemList.Remove(item);
        if (filteredList != null) {
            filteredList.Remove(item);
        }
    }

    public virtual void Render() {
        GUILayout.BeginVertical(GUILayout.MaxWidth(200f));
        GUILayout.Space(10f);
        RenderSearchArea();
        GUILayout.Space(5f);
        RenderItemList();
        GUILayout.EndVertical();
    }

    protected virtual void RenderItemList() {
        GUIStyle listStyle = new GUIStyle(GUI.skin.box) {
            margin = new RectOffset() { top = 3 }
        };
        //todo use icon or something
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        Color original = GUI.backgroundColor;
        for (int i = 0; i < filteredList.Count; i++) {
            AssetItem<T> item = filteredList[i];
            GUI.backgroundColor = item.IsSelected ? Color.green : original;
            if (GUILayout.Button(filteredList[i].Name, listStyle, GUILayout.ExpandWidth(true))) {
                if (SetActiveItem != null) {
                    SetActiveItem(filteredList[i]);
                }
            }
        }
        GUI.backgroundColor = original;
        GUILayout.EndScrollView();
    }

    protected virtual void RenderSearchArea() {
        string current = searchString;
        GUILayout.BeginHorizontal();
        GUIStyle toolbarStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachTextField"));
        GUIStyle toolbarCancelStyle = new GUIStyle(GUI.skin.FindStyle("ToolbarSeachCancelButton"));

        searchString = GUILayout.TextField(searchString, toolbarStyle, GUILayout.ExpandWidth(true));

        if (searchString != current) {
            filteredList = itemList.FindAll((currentItem) => {
                return currentItem.Name.Contains(searchString);
            });
        }
        else if (searchString == "") {
            filteredList = itemList;
        }

        if (GUILayout.Button("", toolbarCancelStyle)) {
            searchString = "";
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(5f);
        if (GUILayout.Button(NewButtonText)) {
            CreateNewItem();
        }
    }

    protected virtual void CreateNewItem() {
        Type creatorType = typeof(EntitySystemBase).Assembly.GetType(typeof(T).Name + "Creator");
        if (creatorType == null) {
            Debug.LogError("Cant find creator type: " + typeof(T).Name + "Creator");
            return;
        }

        string titlizedName = Util.SplitAndTitlize(typeof(T).Name);
        string assetpath = "Assets/Entity System/" + titlizedName + "/" + titlizedName + ".asset";
        if (!Directory.Exists(Application.dataPath + "/Entity System/")) {
            Directory.CreateDirectory(Application.dataPath + "/Assets/Entity System/");
        }
        if (!Directory.Exists(Application.dataPath + "/Entity System/" + titlizedName + "/")) {
            Directory.CreateDirectory(Application.dataPath + "/Entity System/" + titlizedName + "/");
        }

        AssetCreator<T> assetCreator = ScriptableObject.CreateInstance(creatorType) as AssetCreator<T>;
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        T instance = Activator.CreateInstance<T>();
        instance.Id = Path.GetFileNameWithoutExtension(assetPathAndName);
        assetCreator.SetSourceAsset(instance);
        AssetDatabase.CreateAsset(assetCreator, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string guid = AssetDatabase.AssetPathToGUID(assetPathAndName);
        itemList.Add(new AssetItem<T>(guid));
        if (SetActiveItem != null) {
            SetActiveItem(itemList.Last());
        }
    }

    protected virtual void LoadFiles() {
        string[] guids = AssetDatabase.FindAssets(assetQuery);

        for (int i = 0; i < guids.Length; i++) {
            string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string guid = AssetDatabase.AssetPathToGUID(assetpath);
            itemList.Add(new AssetItem<T>(guid));
        }
    }

}