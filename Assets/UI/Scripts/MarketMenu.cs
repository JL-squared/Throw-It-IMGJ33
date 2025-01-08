using System.Collections.Generic;
using UnityEngine;

public class MarketMenu : MonoBehaviour {
    private List<MarketSlot> slots;
    public GameObject marketSlotPrefab;
    public Transform verticalGroup;
    void Start() {
        var manager = GameManager.Instance.marketManager;
        var stocks = manager.stocks;
        verticalGroup.KillChildren();
        slots = new List<MarketSlot>();
        foreach (var stock in stocks) {
            var obj = Instantiate(marketSlotPrefab, verticalGroup);
            MarketSlot slot = obj.GetComponent<MarketSlot>();
            slot.stock = stock;
            slots.Add(slot);
            slot.Refresh();
        }
    }

    public void SellAnythingTest() {
        int slot = Player.Instance.CheckForUnfullMatchingStack("scrap");
        int removed = -1;
        if (slot >= 0) {
            removed = Player.Instance.RemoveItem(slot, 1000);
        }

        if (removed > 0) {
            GameManager.Instance.marketManager.Sell(new ItemStack("scrap", removed));
        }
    }

    public void MarketUpdate() {
        foreach (var slot in slots) {
            slot.Refresh();
        }
    }
}
