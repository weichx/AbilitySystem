using UnityEngine;
using Intelligence;

public interface IActionbarItem {
    Sprite Icon { get; }
    PlayerCharacterAction Action { get; }
    void PrepareToolTip();
}