using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemContainer : MonoBehaviour {
    public List<ItemStack> items;

    public UnityEvent<List<ItemStack>> onUpdate;

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
            }
            else if (itemIn.Data == item.Data && !item.IsFull()) { // this will keep running for as many partial stacks as we can find
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
        }
        else {
            Debug.LogWarning("Could not find spot to add item, spawning as World Item!");
            for (int k = 0; k < itemIn.Count; k++) {
                WorldItem.Spawn(itemIn, transform.position, transform.rotation);
            }
        }

        onUpdate.Invoke(items);
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
                //SelectionChanged(() => { items[slot].Data = null; });
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
}
