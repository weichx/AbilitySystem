public class StatusItem : AssetItem<StatusEffect> {

    public StatusItem(AssetCreator creator) : base(creator) { }

    protected override void InitializeScriptable() {
        scriptableType.GetField("statusEffect").SetValue(scriptable, instanceRef);
        for (int i = 0; i < instanceRef.components.Count; i++) {
            scriptableType.GetField("component" + i).SetValue(scriptable, instanceRef.components[i]);
        }
    }

    protected override string GetCodeString() {
        string code = "using UnityEngine;\n";
        code += "public class GeneratedScriptable : ScriptableObject {\n";
        code += "\tpublic StatusEffect statusEffect;\n";
        for (int i = 0; i < instanceRef.components.Count; i++) {
            code += "\tpublic " + instanceRef.components[i].GetType().Name + " component" + i + ";\n";
        }
        code += "}";
        return code;
    }
}