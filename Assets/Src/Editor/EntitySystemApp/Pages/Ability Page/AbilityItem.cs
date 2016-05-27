
public class AbilityItem : AssetItem<Ability> {

    public AbilityItem(AssetCreator creator) : base(creator) { }

    protected override void InitializeScriptable() {
        scriptableType.GetField("ability").SetValue(scriptable, instanceRef);
        for (int i = 0; i < instanceRef.components.Count; i++) {
            scriptableType.GetField("__component" + i).SetValue(scriptable, instanceRef.components[i]);
        }
        for (int i = 0; i < instanceRef.requirements.Count; i++) {
            scriptableType.GetField("__requirement" + i).SetValue(scriptable, instanceRef.requirements[i]);
        }
    }

    protected override string GetCodeString() {
        string code = "using UnityEngine;\n";
        code += "public class GeneratedScriptable : ScriptableObject {\n";
        code += "\tpublic Ability ability;\n";
        for (int i = 0; i < instanceRef.components.Count; i++) {
            code += "\tpublic " + instanceRef.components[i].GetType().Name + " __component" + i + ";\n";
        }
        for (int i = 0; i < instanceRef.requirements.Count; i++) {
            code += "\tpublic " + instanceRef.requirements[i].GetType().Name + " __requirement" + i + ";\n";
        }
        code += "}";
        return code;
    }
}