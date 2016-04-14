namespace AbilitySystem {
    public interface IStatusPrototype {
        string Name { get; set; }
        bool IsDispellable { get; set; }
        bool IsRefreshable { get; set; }
        bool IsStackable { get; }
        TagCollection Tags { get; set; }
        int MaxStacks { get; set; }

        // ModifiableAttributePrototype BaseDuration { get; set; }
        // ModifiableAttributePrototype MaxStacks { get; set; }

        void OnUpdate(Status status);
        void OnApply(Status status);
        void OnRefresh(Status status);
        void OnDispel(Status status);
        void OnExpire(Status status);
        void OnRemove(Status status);
    }
}