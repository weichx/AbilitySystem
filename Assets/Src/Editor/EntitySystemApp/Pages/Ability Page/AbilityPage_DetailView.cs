using UnityEditor;
using UnityEngine;

public class AbilityPage_DetailView : DetailView<Ability> {

    public AbilityPage_DetailView() : base() {
        sections.Add(new AbilityPage_NameSection(20f));
        sections.Add(new AbilityPage_GeneralSection(20f));
        sections.Add(new AbilityPage_ContextSection(20f));
        sections.Add(new AbilityPage_ComponentSection(10f));
        sections.Add(new AbilityPage_RequirementSection(10f));
    }

}