using UnityEngine;
using System.Collections;
using UnityEditor;
using EntitySystem;
using System.Reflection;

public class InventoryItemPage : MasterDetailPage<InventoryItem> {
    public InventoryItemPage() : base()
    {
        detailView = new InventoryItemPage_DetailView();
    }
}
