using System;
using Intelligence;
using UnityEngine;

public class AbilityPage_ComponentSection : ListSection<Ability> {

    public AbilityPage_ComponentSection(float spacing) : base(spacing) { }
    private Type lastContextType;

    protected override string FoldOutLabel {
        get { return "Componets"; }
    }

    protected override string ListRootName {
        get { return "components"; }
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
        Type baseType = typeof(AbilityComponent<>);
        Type genType = baseType.MakeGenericType(new Type[] { targetType });
        var searchSet = Reflector.FindSubClasses(typeof(AbilityComponent));
        searchSet = searchSet.FindAll((componentType) => {
            //is context type assignable from target type?
            var dummy = Activator.CreateInstance(componentType) as AbilityComponent;
            return dummy.GetContextType().IsAssignableFrom(targetType);
        });
        return new SearchBox(null, searchSet, AddListItem, "Add Component", "Components");
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
