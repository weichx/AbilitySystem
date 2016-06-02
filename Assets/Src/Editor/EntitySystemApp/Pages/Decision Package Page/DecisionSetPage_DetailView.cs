using Intelligence;

public class DescisionPackagePage_DetailView : DetailView<DecisionPackage> {

    public DescisionPackagePage_DetailView() {
        sections.Add(new DecisionPackage_NameSection(40f));
        sections.Add(new DecisionSetPage_DecisionList(10f));
    }

}

