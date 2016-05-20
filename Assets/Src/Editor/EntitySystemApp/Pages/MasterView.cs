using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MasterView<T, U> where T : AssetItem<U> where U : EntitySystemBase {

    protected string searchString;
    protected string assetQuery;
    protected Vector2 scrollPosition;
    protected List<T> itemList;
    protected List<T> filteredList;
    protected Action<T> SetActiveItem;

    public MasterView(Action<T> onItemSelected, string assetQuery = null) {
        SetActiveItem = onItemSelected;
        this.assetQuery = assetQuery ?? "t:" + typeof(U).Name + "Creator";
        searchString = "";
        itemList = new List<T>();
        LoadFiles();
    }

    protected virtual string NewButtonText {
        get { return "New " + typeof(U).Name;  }
    }

    public void AddItem(T item) {
        if (itemList.Contains(item)) return;
        itemList.Add(item);
        if(!string.IsNullOrEmpty(searchString)) {
            filteredList = itemList.FindAll((current) => {
                return current.Name.Contains(searchString);
            });
        }
    }

    public void RemoveItem(T item) {
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
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        Color original = GUI.backgroundColor;
        for (int i = 0; i < filteredList.Count; i++) {
            T item = filteredList[i];
            GUI.backgroundColor = item.IsSelected ? Color.green : original;
            if (GUILayout.Button(filteredList[i].Name, listStyle, GUILayout.ExpandWidth(true))) {
                if(SetActiveItem != null) {
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
        Type creatorType = typeof(EntitySystemBase).Assembly.GetType(typeof(U).Name + "Creator");
        if (creatorType == null) {
            Debug.LogError("Cant find creator type: " + typeof(U).Name + "Creator");
            return;
        }

        string titlizedName = Util.SplitAndTitlize(typeof(U).Name);
        string assetpath = "Assets/Entity System/" + titlizedName + "/" + titlizedName + ".asset";
        if (!Directory.Exists(Application.dataPath + "/Entity System/")) {
            Directory.CreateDirectory(Application.dataPath + "/Assets/Entity System/");
        }
        if (!Directory.Exists(Application.dataPath + "/Entity System/" + titlizedName + "/")) {
            Directory.CreateDirectory(Application.dataPath + "/Entity System/" + titlizedName + "/");
        }

        AssetCreator asset = ScriptableObject.CreateInstance(creatorType) as AssetCreator;
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetpath);
        U instance = Activator.CreateInstance<U>();
        instance.Id = Path.GetFileNameWithoutExtension(assetPathAndName);

        AssetSerializer serializer = new AssetSerializer();
        serializer.AddItem(instance);
        asset.source = serializer.WriteToString();
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssetCreator creator = AssetDatabase.LoadAssetAtPath<AssetCreator>(assetPathAndName);
        itemList.Add(Activator.CreateInstance(typeof(T), new object[] { creator }) as T);
        if (SetActiveItem != null) {
            SetActiveItem(itemList.Last());
        }
    }

    protected virtual void LoadFiles() {
        Type creatorType = typeof(EntitySystemBase).Assembly.GetType(typeof(U).Name + "Creator");
        string[] guids = AssetDatabase.FindAssets(assetQuery);

        for (int i = 0; i < guids.Length; i++) {
            string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
            string name = Path.GetFileNameWithoutExtension(assetpath);
            AssetCreator creator = AssetDatabase.LoadAssetAtPath(assetpath, creatorType) as AssetCreator;
            itemList.Add(Activator.CreateInstance(typeof(T), new object[] { creator }) as T);
        }
    }

}