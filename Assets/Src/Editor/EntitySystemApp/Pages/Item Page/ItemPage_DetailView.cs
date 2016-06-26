using UnityEngine;
using UnityEditor;

public class ItemPage_DetailView : DetailView<Item> {

    public ItemPage_DetailView() : base()
    {
        sections.Add(new ItemPage_NameSection(20f));
    }
}
