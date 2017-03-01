using UnityEngine;
using UnityEditor;
using EntitySystem;

public class CharacterPage_DetailView : DetailView<Character> {

    public CharacterPage_DetailView() : base()
    {
        sections.Add(new CharacterPage_NameSection(20f));
        sections.Add(new CharacterPage_GeneralSection(20f));
        sections.Add(new CharacterPage_ParamSection(20f));
        sections.Add(new CharacterPage_EquipmentSection(20f));
 //       sections.Add(new CharacterPage_ComponentSection(20f));
    }
}
