using UnityEngine;
using UnityEditor;
using EntitySystem;
using Intelligence;
using System;

public class InventoryItemPage_ComponentSection : ListSection<InventoryItem> {

    public InventoryItemPage_ComponentSection(float spacing) : base(spacing) {}

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
    }

    // Debug button for easier testing of formula outputs
    public void CreateTestButton() {
        if (GUILayout.Button("Test formula")) {
            for (int i = 0; i < listRoot.ChildCount; i++) {
                var child = listRoot.GetChildAt(i);
                if (child.Type == typeof(DamageFormula)) {
                    var value = (int)child.FindProperty("baseDamage").Value;
                    var formula = child.FindProperty("damageFormula");
                    var target = (CharacterCreator)child.FindProperty("TargetCharacter").Value;
                    Delegate d = Reflector.FindDelegateWithSignature(formula.Value.ToString());
                    Func<SingleTargetContext, float, float> fn = d as Func<SingleTargetContext, float, float>;
                    var ctx = (SingleTargetContext)target.Create().GetContext();
                    Debug.Log(fn(ctx, value));
                }
            }
        }
    }


    public override void Render() {
        base.Render();
        CreateTestButton();
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(InventoryItemComponent), AddListItem, "Add Component", "Components");
    }
}