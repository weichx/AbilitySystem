public class AbilityPage_RequirementSection : ListSection<Ability> {

    public AbilityPage_RequirementSection(float spacing) : base(spacing) { }

    protected override string FoldOutLabel {
        get { return "Requirements"; }
    }

    protected override string ListRootName {
        get { return "requirements"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(AbilityRequirement), AddListItem, "Add Requirement", "Requriements");
    }

}