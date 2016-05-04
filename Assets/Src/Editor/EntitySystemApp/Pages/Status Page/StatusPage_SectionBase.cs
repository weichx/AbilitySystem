using UnityEditor;

public abstract class StatusPage_SectionBase {

    protected StatusPage page;
    protected SerializedObject target;
    protected SerializedProperty statusProperty;

    public StatusPage_SectionBase(StatusPage page) {
        this.page = page;
    }

    public virtual void SetTargetObject(SerializedObject target) {
        this.target = target;
        if (target != null) {
            statusProperty = target.FindProperty("statusEffect");
        }
        else {
            statusProperty = null;
        }
    }

    public abstract void Render();

}
