using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class MarketMenu : MonoBehaviour {
    public List<MarketSlot> slots;
    public GameObject marketSlotPrefab;
    public Transform verticalGroup;
    void Start() {
        var stocks = GameManager.Instance.marketManager.stocks;
        Utils.KillChildren(verticalGroup);
        foreach (var stock in stocks) {
            var obj = Instantiate(marketSlotPrefab, verticalGroup);
            MarketSlot slot = obj.GetComponent<MarketSlot>();
            slot.stock = stock;
            slot.Refresh();
        }
    }

    public void SellAnythingTest() {
        int slot = Player.Instance.CheckForItem("scrap");
        Debug.Log(slot);
        int removed = -1;
        if (slot >= 0) {
            removed = Player.Instance.RemoveItem(slot, 1000);
        }

        if (removed > 0) {
            GameManager.Instance.marketManager.Sell(new Item("scrap", removed));
        }
    }
}
