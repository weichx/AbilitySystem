public abstract class AbilityRequirement {

    protected bool supressed;
    public string requirementId;
    protected RequirementType appliesTo;

    public RequirementType RequirementType {
        get { return appliesTo; }
    }

    public static string[] Options =
    {
        "Start", "Update", "Complete", "Start + Update", "Start + End", "Update + End", "All"
    };

    public bool Test(Context context, RequirementType type) {
        if (supressed || (type & RequirementType) == 0) {
            return true;
        }

        bool requirementMet = OnTest(context, type);

        if (requirementMet) {
            OnPassed(context, type);
        }
        else {
            OnFailed(context, type);
        }
        return requirementMet;
    }

    public virtual bool OnTest(Context context, RequirementType type) { return true; }
    public virtual void OnPassed(Context context, RequirementType type) { }
    public virtual void OnFailed(Context context, RequirementType type) { }

    public void ApplyTo(RequirementType type) {
        appliesTo |= type;
    }

    public void DoNotApplyTo(RequirementType type) {
        appliesTo &= ~type;
    }

    public bool AppliesTo(RequirementType type) {
        return (type & appliesTo) != 0;
    }

    public bool IsSupressed {
        get { return supressed; }
        set { supressed = value; }
    }

}
