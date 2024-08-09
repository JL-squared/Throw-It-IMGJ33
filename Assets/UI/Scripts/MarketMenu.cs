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
}
