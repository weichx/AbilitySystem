using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StatusPage_DetailView : DetailView<StatusEffect> {

   
    public StatusPage_DetailView() : base() {
        sections.Add(new StatusPage_NameSection(20f));
        sections.Add(new StatusPage_GeneralSection(10f));
        sections.Add(new StatusPage_ComponentSection(10f));
    }

}
