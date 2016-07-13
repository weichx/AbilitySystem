using UnityEngine;
using UnityEditor;

public class InventoryItemPage_DetailView : DetailView<InventoryItem> {

    public InventoryItemPage_DetailView() : base()
    {
        sections.Add(new InventoryItemPage_NameSection(20f));
        sections.Add(new InventoryItemPage_GeneralSection(20f));
        sections.Add(new InventoryItemPage_ComponentSection(20f));
    }
}
