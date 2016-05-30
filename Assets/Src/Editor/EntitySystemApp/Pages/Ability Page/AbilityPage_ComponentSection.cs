public class AbilityPage_ComponentSection : ListSection<Ability> {

    public AbilityPage_ComponentSection(float spacing) : base(spacing) { }

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(AbilityComponent), AddListItem, "Add Component", "Components");
    }

}
