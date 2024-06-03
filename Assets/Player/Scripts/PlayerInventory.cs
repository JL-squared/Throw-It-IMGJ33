using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour {
    Item[] items = new Item[10];

    UnityEvent<int, bool> slotUpdateEvent;

    /// <summary>
    /// Add item to inventory if possible
    /// </summary>
    /// <param name="itemIn"></param>
    public void addItem(ref Item itemIn) {
        foreach (Item item in items) {
            if (itemIn.data.id == item.data.id) {
                item.Count += itemIn.Count; // TODO ; ACTUAL STACK SIZE LOL
                itemIn.Count = 0;
            }
        }
    }    

    /// <summary>
    /// Returns slot number (0-9) if we have item of specified count (default 1), otherwise returns -1
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int checkForItem(string id, int count = 1) {
        int i = 0;

        foreach(Item item in items) {
            if(item.Count == count && item.data.id == id) {
                return i;
            }
            i++;
        }

        return -1;
    }
}
