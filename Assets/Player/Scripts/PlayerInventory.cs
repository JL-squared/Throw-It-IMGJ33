using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using static UnityEditor.Profiling.HierarchyFrameDataView;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UIElements;

public class PlayerInventory : PlayerBehaviour {
    public bool keepUpdatingHolsterTransform;
    public GameObject viewModel;
    public ItemContainer container;

    [SerializeField]
    private int equipped;
    public int Equipped {
        get {
            return equipped;
        }
        set {
            equipped = value;
            selectedEvent?.Invoke(equipped);
        }
    }
    [HideInInspector]
    public ItemStack EquippedItem { get { return container[equipped]; } }

    [HideInInspector]
    public UnityEvent<int> selectedEvent;
    [HideInInspector]
    public UnityEvent<int, bool> slotUpdateEvent;

    public void Start() {
        // Add temp items at start
        container.PutItem(new ItemStack("snowball", 1));
        container.PutItem(new ItemStack("battery", 1));
        container.PutItem(new ItemStack("shovel", 1));
        container.PutItem(new ItemStack("wires", 1));

        foreach(ItemStack item in container) {
            item.onEmpty.AddListener(() => { SelectionChanged(() => { item.Data = null; }); });
        }
    }

    private void Update() {
        EquippedItem.logic?.EquippedUpdate(player);
        foreach (ItemStack item in container) {
            item.logic?.Update(player);
        }

        if (viewModel != null && !EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty() && keepUpdatingHolsterTransform) {
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void ToggleInventory(InputAction.CallbackContext context) {
        if (context.performed && !context.canceled && player.state != Player.State.Dead && GameManager.Instance.initialized) {
            UIScriptMaster.Instance.inGameHUD.ToggleInventory();
        }
    }

    public void SelectSlot(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            SelectionChanged(() => { Equipped = (int)context.ReadValue<float>(); });
        }
    }

    public void Scroll(float scroll) {
        SelectionChanged(() => {
            int newSelected = (Equipped + (int)scroll) % 10;
            Equipped = (newSelected < 0) ? 9 : newSelected;
        });
    }

    public void PrimaryAction(InputAction.CallbackContext context) {
        if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.PrimaryAction(context, player);
        }
    }

    public void SecondaryAction(InputAction.CallbackContext context) {
        if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.SecondaryAction(context, player);
        }
    }

    // Only called when the following happens:
    // - User changes selected slot to new slot
    // - Item count gets changed from zero to positive value and vice versa
    private void SelectionChanged(Action function = null) {
        EquippedItem.logic.Unequipped(player);
        function?.Invoke();
        EquippedItem.logic.Equipped(player);

        if (viewModel != null) {
            Destroy(viewModel);
        }

        if (!EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty()) {
            viewModel = Instantiate(EquippedItem.Data.prefab, player.bobbing.viewModelHolster.transform);
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void DropItem(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            ItemStack item = container[equipped];
            if (item.Count > 0) {
                if (WorldItem.Spawn(container[equipped].NewCount(1), player.camera.transform.position + player.camera.transform.forward, Quaternion.identity, player.movement.inner.Velocity))
                    container.RemoveItem(equipped, 1);
            }
        }
    }
}