using UnityEngine;
using UnityEditor;
using EntitySystem;

public class CharacterPage_ComponentSection : ListSection<Character> {

    public CharacterPage_ComponentSection(float spacing) : base(spacing) {}

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(CharacterComponent), AddListItem, "Add Component", "Components");
    }
}