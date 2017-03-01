using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using EntitySystem;

public class CharacterPage : MasterDetailPage<Character> {
    public CharacterPage() : base()
    {
        detailView = new CharacterPage_DetailView();
    }
}
