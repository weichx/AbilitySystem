using System;
using System.Collections.Generic;
using AbilitySystem;
using Intelligence;
using EntitySystem;

public class InventoryItemManager {

    protected List<InventoryItem> items;
    protected Entity entity;

    public InventoryItemManager(Entity entiy) {
        this.entity = entity;
        items = new List<InventoryItem>();         
    }

    public void Update() {

    }
}