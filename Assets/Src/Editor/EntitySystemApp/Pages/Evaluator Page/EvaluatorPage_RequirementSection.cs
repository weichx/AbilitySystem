using UnityEngine;
using UnityEditor;
using Intelligence;
using System;
using System.Collections.Generic;


public class EvaluatorPage_RequirementSection : ListSection<DecisionScoreEvaluator> {

    public EvaluatorPage_RequirementSection(float spacing) : base(spacing) { }

    protected override string FoldOutLabel {
        get { return "Requirements"; }
    }

    protected override string ListRootName {
        get { return "requirements"; }
    }

    protected override SearchBox CreateSearchBox() {
        return new SearchBox(null, typeof(Requirement), AddListItem, "Add Requirement", "Requirements");
    }

}