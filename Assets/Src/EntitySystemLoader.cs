using UnityEngine;
using MiniJSON;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class EntitySystemLoader : MonoBehaviour {

    public string entityPath = "Entity Templates";
    public string statusPath;
    public string abilityPath;
    public string resourcePath;
    public string currentLevelPath;

    protected Dictionary<string, Dictionary<string, string>> database;

    public virtual void StartLoad() {
        database = new Dictionary<string, Dictionary<string, string>>();
        LoadPath<Ability>("Abilities");
        LoadPath<StatusEffect>("Status Effects");
        LoadPath<EntityTemplate>("Entity Templates");
    }

    protected List<TextAsset> GetJsonAtPath(string path) {
        List<TextAsset> retn = new List<TextAsset>();
        Object[] assets = Resources.LoadAll(path) as Object[];
        for (int i = 0; i < assets.Length; i++) {
            TextAsset json = assets[i] as TextAsset;
            if (json == null) continue;
            retn.Add(json);
        }
        return retn;
    }

    protected void LoadPath<T>(string path) {
        string bucketId = typeof(T).Name;
        List<TextAsset> jsonFiles = GetJsonAtPath(path);
        Dictionary<string, string> templateDatabase = new Dictionary<string, string>();
        for (int i = 0; i < jsonFiles.Count; i++) {
            templateDatabase[jsonFiles[i].name] = jsonFiles[i].text;
        }
        database[bucketId] = templateDatabase;
    }

    public T Create<T>(string templateId) where T : class, new() {
        if (string.IsNullOrEmpty(templateId)) return null;
        Type type = typeof(T);
        Dictionary<string, string> accessor;
        if (database.TryGetValue(type.Name, out accessor)) {
            string json;
            if (accessor.TryGetValue(templateId, out json)) {
                return Json.Deserialize<T>(json);
            }
            Debug.LogError("Could find template '" + templateId + "' when creating " + type.Name);
            return null;
        }
        Debug.LogError("Could find any templates for " + type.Name);
        return null;
    }

    public T[] CreateAll<T>() where T : class, new() {
        if (database == null) {
            StartLoad();
        }
        Type type = typeof(T);
        Dictionary<string, string> accessor;
        if (database.TryGetValue(type.Name, out accessor)) {
            int i = 0;
            T[] retn = new T[accessor.Count];
            foreach (var item in accessor) {
                retn[i++] = Json.Deserialize<T>(item.Value);
            }
            return retn;
        }
        return null;

    }

    private static EntitySystemLoader instance;
    public static EntitySystemLoader Instance {
        get {
            if (instance == null) {
                instance = Util.FindOrCreateByName("EntitySystemLoader", new Type[] {
                    typeof(EntitySystemLoader)
                }).GetComponent<EntitySystemLoader>();
                return instance;
            }
            else {
                return instance;
            }
        }
    }


}
