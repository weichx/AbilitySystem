using Intelligence;

public class DecisionPackagePage : MasterDetailPage<DecisionPackage> {

    public DecisionPackagePage() {
        detailView = new DescisionPackagePage_DetailView();
    }

}