using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class AIDecisionLog {
    public string entityName;
    public List<AIDecisionLogEntry> entries;

    public AIDecisionLog(string entityName) {
        this.entityName = entityName;
        entries = new List<AIDecisionLogEntry>(100);
    }

    public AIDecisionLogEntry AddEntry() {
        var entry = new AIDecisionLogEntry();
        entries.Add(entry);
        return entry;
    }

    public void Upload() {

    }

    public void WriteToDisk(string path, int entryCount = -1) {
        path = Application.dataPath + "/" + path;
        using (StreamWriter file = new StreamWriter(path, false)) {
            if (entryCount == -1) {
                file.WriteLine(JsonUtility.ToJson(this));
            }
            else {
                StringBuilder builder = new StringBuilder();
                if(entries.Count < entryCount) {
                    entryCount = entries.Count;
                }
                builder.Append("{\n\"entityName\":" + entityName + ",\n\"entries\":[");
                for(int i = entryCount - 1; i >= 0; i--) {
                    builder.Append(JsonUtility.ToJson(entries[i]));
                    if (i != 0) builder.Append(",\n");
                }
                builder.Append("\n]");
                file.WriteLine(JsonUtility.ToJson(builder.ToString()));
            }
        }
    }
}