using EntitySystem;

public class StatusPage : MasterDetailPage<StatusEffect> {

    public StatusPage() : base() {
        detailView = new StatusPage_DetailView();
    }
    
}