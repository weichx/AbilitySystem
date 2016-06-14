using Intelligence;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class ActionbarSlot : UISlotBase {

    private Image cooldownImage;
    private Text keybindText;
    private Text cooldownText;
    private IActionbarItem item;
    private string actionId;

    public void SetItem(IActionbarItem item) {
        this.item = item;
        if (item != null) {
            Assign(item);
        }
    }

    public bool Assign(IActionbarItem item) {
        this.item = item;
        ClearIcon();
        SetIcon(item.Icon);
        return true;
    }

    public override bool Assign(Object source) {
        ActionbarSlot other = source as ActionbarSlot;
        if (other == null) return false;
        return Assign(other.item);
    }

    public override bool CanSwapWith(Object target) {
        return target is ActionbarSlot;
    }

    public override bool PerformSlotSwap(Object source) {
        ActionbarSlot other = source as ActionbarSlot;
        if (other == null) return false;
        IActionbarItem otherItem = other.item;
        bool assign1 = other.Assign(item);
        bool assign2 = Assign(otherItem);
        return assign1 && assign2;
    }

    public override void OnTooltip(bool show) {
        if (item == null) return;

        if (show) {
            item.PrepareToolTip();
            UITooltip.AnchorToRect(transform as RectTransform);
            UITooltip.Show();
        }
        else {
            UITooltip.Hide();
        }
    }

    public override bool IsAssigned() {
        return item != null;
    }

    public override void OnPointerClick(PointerEventData eventData) {
        if (!IsAssigned() || item == null) return;

        PlayerIntelligenceController.Enqueue(item.Action);

        eventData.Use();

    }
}