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
    [HideInInspector]
    public List<ItemStack> items = new List<ItemStack>();

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
    public ItemStack EquippedItem { get { return items[equipped]; } }

    [HideInInspector]
    public UnityEvent<int> selectedEvent;
    [HideInInspector]
    public UnityEvent<List<ItemStack>> inventoryUpdateEvent;
    [HideInInspector]
    public UnityEvent<int, bool> slotUpdateEvent;

    public void Start() {
        // Creates inventory items and hooks onto their update event
        for (int i = 0; i < 10; i++) {
            ItemStack temp = new ItemStack(null, 0);
            items.Add(temp);
            temp.onUpdate?.AddListener((ItemStack item) => { inventoryUpdateEvent.Invoke(items); });
        }

        // Add temp items at start
        AddItem(new ItemStack("snowball", 1));
        AddItem(new ItemStack("battery", 1));
        AddItem(new ItemStack("shovel", 1));
        AddItem(new ItemStack("wires", 1));
    }

    private void Update() {
        EquippedItem.logic.EquippedUpdate(player);
        foreach (ItemStack item in items) {
            item.logic.Update(player);
        }

        if (viewModel != null && !EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty() && keepUpdatingHolsterTransform) {
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void ToggleInventory(InputAction.CallbackContext context) {
        Debug.Log("nuhuh");
        /*
        if (context.performed && !context.canceled && player.state != Player.State.Dead && GameManager.Instance.initialized) {
            UIScriptMaster.Instance.inGameHUD.ToggleInventory();
        }
        */
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

    // Add item to inventory if possible,
    // Works with invalid stack sizes
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItemUnclamped(ItemStack itemIn) {
        int count = itemIn.Count;
        while (count > 0) {
            int tempCount = Mathf.Min(itemIn.Data.stackSize, count);
            AddItem(new ItemStack(itemIn.Data, tempCount));
            count -= itemIn.Data.stackSize;
        }
    }


    // Add item to inventory if possible,
    // DO NOT INSERT INVALID STACK COUNT ITEM, (clamped anyways)
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItem(ItemStack itemIn) {
        /*
        if (itemIn.Count > itemIn.Data.stackSize) {
            Debug.LogWarning("Given item count was greater than stack size. Clamping. Use AddItemUnclamped for unclamped stack sizes");
            itemIn.Count = itemIn.Data.stackSize;
        }
        */

        int firstEmpty = -1;
        int i = 0;
        foreach (ItemStack item in items) {
            if (item.IsEmpty() && firstEmpty == -1) {
                firstEmpty = i;
            } else if (itemIn.Data == item.Data && !item.IsFull()) { // this will keep running for as many partial stacks as we can find
                int transferSize = item.Data.stackSize - item.Count; // amount we can fit in here
                transferSize = Mathf.Min(transferSize, itemIn.Count);

                item.Count += transferSize; // transfer
                itemIn.Count -= transferSize;
                if (itemIn.IsEmpty()) {
                    return; // break when done adding into partial stacks
                }
            }
            i++;
        }

        // by this point we should have exited if everything is handled, otherwise;
        if (firstEmpty != -1) {
            //Debug.Log("oh yeah, slot was empty. It's sex time...");
            items[firstEmpty].CopyItem(itemIn.Clone()); // Don't know if we actually have to Clone this lol but wtv
            itemIn.MakeEmpty();
        } else {
            Debug.LogWarning("Could not find spot to add item, spawning as World Item!");
            for (int k = 0; k < itemIn.Count; k++) {
                WorldItem.Spawn(itemIn, transform.position, transform.rotation);
            }
        }

        if (firstEmpty == equipped) {
            SelectionChanged();
        }

        inventoryUpdateEvent.Invoke(items);
    }

    public void PutItem(ItemStack itemIn) {
        var itemInEvenMore = itemIn.Clone();

        AddItem(itemInEvenMore);
    }

    // Checks if we can fit a specific item
    // Count must be within stack count (valid item moment)
    public bool CanFitItem(ItemStack itemIn) {
        int emptyCounts = 0;

        foreach (ItemStack item in items) {
            if (item.IsEmpty()) {
                return true;
            } else if (itemIn.Data == item.Data && !item.IsFull()) {
                emptyCounts += itemIn.Data.stackSize - item.Count;
            }
        }

        return emptyCounts >= itemIn.Count;
    }

    // Remove a specific count from a specific slot. Returns the number of items that successfully got removed
    public int RemoveItem(int slot, int count) {
        if (items[slot].Data != null && items[slot].Count > 0) {
            int ogCount = items[slot].Count;
            int currentCount = items[slot].Count;
            currentCount = Mathf.Max(currentCount - count, 0);
            //int missingCount = Mathf.Max(count - currentCount, 0);

            if (currentCount == 0) {
                SelectionChanged(() => { items[slot].Data = null; });
            }

            items[slot].Count = currentCount;

            return ogCount - currentCount;
        }

        return -1;
    }

    // Returns slot number (0-9) if we have item of specified count (default 1), otherwise returns -1
    public int CheckForUnfullMatchingStack(string id, int count = 1) {
        int i = 0;
        foreach (ItemStack item in items) {
            if (item.Count >= count && item.Data == Registries.items.data[id]) {
                return i;
            }
            i++;
        }

        return -1;
    }

    public bool CheckForItems(ItemStack stack) {
        if (stack.IsNullOrDestroyed() || stack.IsEmpty()) return true;
        return CheckForItemAmount(stack.data.name, stack.count);
    }

    public bool CheckForItemAmount(string id, int count) {
        foreach (ItemStack item in items) {
            if (count <= 0)
                return true;

            if (item.Data == Registries.items.data[id]) {
                count -= item.Count;
            }
        }
        return false;
    }

    public bool CheckForRequirements(List<ItemStack> items) {
        bool i = true;
        foreach (ItemStack item in items) {
            i = i && CheckForItems(item);
        }
        return i;
    }

    // Both of these functions are super unsafe, fix later
    public void TakeItems(ItemStack stack) {
        if (!stack.IsNullOrDestroyed() && !stack.IsEmpty()) {
            TakeItemAmount(stack.data.name, stack.count);
        }
    }

    public void TakeItemAmount(string id, int count = 0) {
        foreach (ItemStack item in items) {
            if (count <= 0)
                return;

            if (item.Data == Registries.items.data[id]) {
                count -= item.Count;
                item.Count = count <= 0 ? -count : 0; // if count goes below 0, give it back to the item. othwerwise stack should become 0
            }
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

        if (EquippedItem != null && !EquippedItem.IsEmpty()) {
            viewModel = Instantiate(EquippedItem.Data.prefab, player.bobbing.viewModelHolster.transform);
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void DropItem(InputAction.CallbackContext context) {
        if (Pressed(context)) {
            ItemStack item = items[equipped];
            if (item.Count > 0) {
                if (WorldItem.Spawn(items[equipped].NewCount(1), player.camera.transform.position + player.camera.transform.forward, Quaternion.identity, player.movement.inner.Velocity))
                    RemoveItem(equipped, 1);
            }
        }
    }
}