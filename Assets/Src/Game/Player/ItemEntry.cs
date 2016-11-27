using UnityEngine;

[System.Serializable]
public class ItemEntry {
    private bool initialized;

    private InventoryItem item;

    [SerializeField] private InventoryItemCreator itemCreator;

    public void Initialize(Entity entity) {
        if (initialized) return;
        item = itemCreator.Create();
        item.Owner = entity;
    }
}