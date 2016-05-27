public class ArmorAdjustor : ResourceAdjuster {

    public override float Adjust(float delta, Resource resource, OldContext context) {
        float armor = context.Get<float>("armor");
        return delta - armor;
    }
}