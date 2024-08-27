using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class MarketSlot : MonoBehaviour {
    public ItemStock stock;
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemBuyCost;
    public TextMeshProUGUI itemSellCost;
    public TextMeshProUGUI count;
    public Button buyButton;
    private bool remaining = true;
    private bool canFitItem = true;

    public void Start() {
        Player.Instance.inventoryUpdateEvent.AddListener((data) => {
            canFitItem = Player.Instance.CanFitItem(new Item(stock.item, 1));
            Refresh();
        });
    }

    private string EncodeDiff(int last, int current, bool flip) {
        if (last == current) {
            return current.ToString();
        }

        string good = "<color=\"red\">";
        string bad = "<color=\"green\">";
        string white = "<color=\"white\">";

        if (flip) {
            string temp = good;
            good = bad;
            bad = temp;
        }

        if (last > current) {
            return $"{white}<s>{good}{last}{white}</s>  {bad}{current}";
        } else {
            return $"{white}<s>{bad}{last}{white}</s>  {good}{current}";
        }
    }

    public void Refresh() {
        remaining = stock.count > 0;
        itemName.text = stock.item.name;
        itemBuyCost.text = "B: " + EncodeDiff(stock.Costs[0].y, stock.Costs[1].y, false);
        itemSellCost.text = "S: " + EncodeDiff(stock.Costs[0].x, stock.Costs[1].x, true);
        itemIcon.sprite = stock.item.icon;
        count.text = stock.count.ToString();
        buyButton.interactable = remaining && canFitItem && GameManager.Instance.marketManager.Current > stock.Costs[1].y;
    }

    public void Buy() {
        GameManager.Instance.marketManager.Buy(stock);
        Refresh();
    }
}
