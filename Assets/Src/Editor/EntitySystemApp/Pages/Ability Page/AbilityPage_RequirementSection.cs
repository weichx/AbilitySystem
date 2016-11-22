using System;
using EntitySystem;
using Intelligence;

public class AbilityPage_RequirementSection : ListSection<Ability> {

    public AbilityPage_RequirementSection(float spacing) : base(spacing) { }
    private Type lastContextType;

    protected override string FoldOutLabel {
        get { return "Requirements"; }
    }

    protected override string ListRootName {
        get { return "requirements"; }
    }


    public override void SetTargetProperty(SerializedPropertyX rootProperty) {
        base.SetTargetProperty(rootProperty);
        if (rootProperty == null) return;
        lastContextType = rootProperty["contextType"].GetValue<Type>();
        searchBox = CreateSearchBox();
    }

    protected override SearchBox CreateSearchBox() {
        if (rootProperty == null) return null;
        Type targetType = rootProperty["contextType"].GetValue<Type>();
        Type baseType = typeof(AbilityRequirement<>);
        Type genType = baseType.MakeGenericType(new Type[] { targetType });
        var searchSet = Reflector.FindSubClasses(typeof(AbilityRequirement));
        searchSet = searchSet.FindAll((componentType) => {
            var dummy = Activator.CreateInstance(componentType) as AbilityRequirement;
            return dummy.GetContextType().IsAssignableFrom(targetType);
        });
        return new SearchBox(null, searchSet, AddListItem, "Add Requirement", "Requirements");
    }


    public override void Render() {
        if (rootProperty == null) return;
        var contextType = rootProperty["contextType"].GetValue<Type>();
        if (contextType != lastContextType) {
            lastContextType = rootProperty["contextType"].GetValue<Type>();
            searchBox = CreateSearchBox();
        }
        base.Render();
    }
}