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

    public override void Render() {
        base.Render();
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(InventoryItemComponent), AddListItem, "Add Component", "Components");
    }
}