
public class StatusPage_ComponentSection : ListSection<StatusEffect> {

    public StatusPage_ComponentSection(float spacing) : base(spacing) { }

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(StatusEffectComponent), AddListItem, "Add Component", "Components");
    }
}