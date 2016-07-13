using UnityEngine;
using UnityEditor;

public class InventoryItemPage_ComponentSection : ListSection<InventoryItem> {

    public InventoryItemPage_ComponentSection(float spacing) : base(spacing) {}

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(InventoryItemComponent), AddListItem, "Add Component", "Components");
    }
}