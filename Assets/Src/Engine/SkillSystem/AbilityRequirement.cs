using System;
using UnityEngine;
using Intelligence;

public abstract class AbilityRequirement {

    protected bool supressed;
    [EnumFlag] [SerializeField]
    public RequirementType appliesTo = RequirementType.All;

    protected Context context;

    public virtual void SetContext(Context context) {
        this.context = context;
    }

    public RequirementType RequirementType {
        get { return appliesTo; }
    }

    public static string[] Options =
    {
        "Start", "Update", "Complete", "Start + Update", "Start + End", "Update + End", "All"
    };

    public bool Test(RequirementType type) {
        if (supressed || (type & RequirementType) == 0) {
            return true;
        }

        bool requirementMet = OnTest(type);

        if (requirementMet) {
            OnPassed(type);
        }
        else {
            OnFailed( type);
        }
        return requirementMet;
    }

    public virtual bool OnTest(RequirementType type) { return true; }
    public virtual void OnPassed(RequirementType type) { }
    public virtual void OnFailed(RequirementType type) { }

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

    public virtual Type GetContextType() {
        return context.GetType();
    }
}

public abstract class AbilityRequirement<T> : AbilityRequirement where T : Context {

    protected new T context;

    public override void SetContext(Context context) {
        this.context = context as T;
    }

    public override Type GetContextType() {
        return typeof(T);
    }
}
