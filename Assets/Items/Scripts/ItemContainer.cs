using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemContainer : IEnumerable<ItemStack> {
    public int size = 0;
    public List<ItemStack> items = new List<ItemStack>();

    public UnityEvent<List<ItemStack>> onUpdate = new UnityEvent<List<ItemStack>>();

    public ItemStack this[int i] {
        get => items[i];
        set => items[i] = value;
    }

    public ItemContainer(int size, bool fill = true) {
        this.size = size;
        if(fill) {
            Initialize();
        }
    }

    public ItemContainer(List<ItemStack> container) {
        this.items = container;
    }

    public void Initialize() {
        for (int i = 0; i < this.size; i++) {
            ItemStack emptySlot = new ItemStack();
            items.Add(emptySlot);
            emptySlot.onUpdate?.AddListener(() => { onUpdate.Invoke(items); });
        }

        onUpdate.Invoke(items);
    }

    // Add item to inventory if possible,
    // Works with invalid stack sizes
    // THIS WILL TAKE FROM THE ITEM YOU INSERT. YOU HAVE BEEN WARNED
    public void AddItemUnclamped(ItemStack itemIn) {
        int count = itemIn.Count;
        while (count > 0) {
            int tempCount = Mathf.Min(itemIn.Data.stackSize, count);
            TransferItem(new ItemStack(itemIn.Data, tempCount));
            count -= itemIn.Data.stackSize;
        }
    }

    public void PutItem(ItemStack itemIn) {
        ItemStack item = itemIn.Clone();
        TransferItem(item);
    }

    public void TransferItem(ItemStack itemIn) {
        foreach (ItemStack item in items) { // Iterate over partial stacks and subtract
            if (itemIn.IsEmpty()) {
                onUpdate.Invoke(items);
                return;
            }

            if (item.Data == itemIn.Data && !item.IsFull()) {
                int amountWeCanPutIn = item.Data.stackSize - item.Count;
                int amountWePutIn = itemIn.Count < amountWeCanPutIn ? itemIn.Count : amountWeCanPutIn;
                item.Count += amountWePutIn;
                itemIn.Count -= amountWePutIn;
            }
        }

        // THE PROBLEM CODE
        foreach (ItemStack item in items) { // Iterate over empty stacks and subtract
            if (itemIn.IsEmpty()) {
                onUpdate.Invoke(items);
                return;
            }

            // THE EVEN MORE PROBLEM CODE
            if (item.IsEmpty()) {
                item.Data = itemIn.Data;
                if (itemIn.Count < itemIn.Data.stackSize) {
                    item.Count = itemIn.Count;
                    itemIn.Count = 0;
                } else {
                    item.Count = itemIn.Data.stackSize;
                    itemIn.Count -= itemIn.Data.stackSize;
                }
            }
        }

        /* This is for throwing things but context wise idk if this is needed lol
        if (!itemIn.IsEmpty()) { // If anything is left then poopity scoop
            Debug.LogWarning("Could not find spot to add item, spawning as World Item!");
            for (int k = 0; k < itemIn.Count; k++) {
                WorldItem.Spawn(itemIn, transform.position, transform.rotation);
            }
        }
        */

        onUpdate.Invoke(items);
    }

    // Checks if we can fit a specific item
    public bool CanFitItem(ItemStack itemIn) {
        int emptyCounts = 0;

        foreach (ItemStack item in items) {
            if (item.IsEmpty()) {
                emptyCounts += itemIn.Data.stackSize;
            }
            else if (itemIn.Data == item.Data && !item.IsFull()) {
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
                items[slot].onEmpty.Invoke();
            }

            items[slot].Count = currentCount;

            return ogCount - currentCount;
        }

        return 0;
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

    public int GetItemAmount(string id) {
        int count = 0;
        foreach (ItemStack item in items) {
            if (item.Data == Registries.items.data[id]) {
                count += item.Count;
            }
        }
        return count;
    }

    public int GetItemAmount(ItemStack itemStack) {
        return GetItemAmount(itemStack.data.name);
    }

    public bool CheckForItems(List<ItemStack> items) {
        bool i = true;
        foreach (ItemStack item in items) {
            i = i && CheckForItems(item);
        }
        return i;
    }

    // Unsafe, 
    public void TakeItems(List<ItemStack> items) {
        foreach (ItemStack item in items) {
            TakeItem(item);
        }
    }

    // Both of these functions are super unsafe, fix later
    public void TakeItem(ItemStack stack) {
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

    public IEnumerator<ItemStack> GetEnumerator() {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return items.GetEnumerator();
    }

    public static implicit operator List<ItemStack>(ItemContainer container) { return container.items; }
}
