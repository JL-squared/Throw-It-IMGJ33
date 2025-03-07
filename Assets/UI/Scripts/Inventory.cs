using System;
using UnityEngine;

public class Inventory : MonoBehaviour {
    public GameObject inventoryContent;
    public GameObject slotPrefab;

    public void Initialize(ItemContainer itemContainer) {
        inventoryContent.transform.KillChildren();
        foreach(ItemStack item in itemContainer) {
            var slot = Instantiate(slotPrefab, inventoryContent.transform);
            var slotComponent = slot.GetComponent<VisualSlot>();
            item.onUpdate.AddListener(() => {
                slotComponent.Refresh(item);
            });
            slotComponent.Refresh(item);
        }
    }
}