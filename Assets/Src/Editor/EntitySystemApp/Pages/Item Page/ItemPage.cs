using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

public class ItemPage : MasterDetailPage<Item> {
    public ItemPage() : base()
    {
        detailView = new ItemPage_DetailView();
    }
}
