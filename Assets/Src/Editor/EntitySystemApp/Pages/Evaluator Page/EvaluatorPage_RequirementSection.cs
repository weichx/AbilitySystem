using  System;
using Intelligence;

public class EvaluatorPage_RequirementSection : ListSection<DecisionEvaluator> {

    public EvaluatorPage_RequirementSection(float spacing) : base(spacing) { }

    protected override string FoldOutLabel {
        get { return "Requirements (" + listRoot.ChildCount + ")"; }
    }

    protected override string ListRootName {
        get { return "requirements"; }
    }

    public override void SetTargetProperty(SerializedPropertyX rootProperty) {
        base.SetTargetProperty(rootProperty);
        searchBox = CreateSearchBox();
    }

    //todo should this be the same as considerations?
    protected override SearchBox CreateSearchBox() {
        if (rootProperty == null) return null;
        Type targetType = rootProperty["contextType"].GetValue<Type>();
        Type baseType = typeof(Requirement);
        Type compareType = null;
        if (targetType.IsGenericType) {
            compareType = baseType.MakeGenericType(new Type[] { targetType });
        } else {
            compareType = baseType;
        }
        
        var searchSet = Reflector.FindSubClasses(typeof(Requirement));
        searchSet = searchSet.FindAll((requirementType) => {
            return compareType.IsAssignableFrom(requirementType);
        });
        return new SearchBox(null, searchSet, AddListItem, "Add Requirement", "Requirements");
    }

}