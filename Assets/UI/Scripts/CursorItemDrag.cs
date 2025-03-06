using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class CursorItemDrag : MonoBehaviour {
    public ItemDisplay itemDisplay;
    public bool containsSomething;

    private void Update() {
        transform.position = Mouse.current.position.ReadValue();
    }

    public void Refresh(ItemStack itemStack) {
        containsSomething = !itemStack.IsEmpty();
        itemDisplay.UpdateValues(itemStack);
    }
}