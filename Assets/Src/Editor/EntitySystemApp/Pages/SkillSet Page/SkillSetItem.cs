
public class SkillSetItem : AssetItem<SkillSet> {

    public SkillSetItem(AssetCreator creator) : base(creator) { }

    protected override void InitializeScriptable() {
        scriptableType.GetField("skillSet").SetValue(scriptable, instanceRef);
        //for (int i = 0; i < instanceRef.components.Count; i++) {
        //    scriptableType.GetField("__component" + i).SetValue(scriptable, instanceRef.components[i]);
        //}
        //for (int i = 0; i < instanceRef.requirements.Count; i++) {
        //    scriptableType.GetField("__requirement" + i).SetValue(scriptable, instanceRef.requirements[i]);
        //}
    }

    protected override string GetCodeString() {
        string code = "using UnityEngine;\n";
        code += "public class GeneratedScriptable : ScriptableObject {\n";
        code += "\tpublic SkillSet skillSet;\n";
        code += "}";
        return code;
    }
}