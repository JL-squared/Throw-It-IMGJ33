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

    public void Refresh() {
        itemName.text = stock.item.title;
        itemBuyCost.text = $"B: {stock.GetBuyCost}";
        itemSellCost.text = $"S: {stock.GetSellCost}";
        itemIcon.sprite = stock.item.icon;
        count.text = stock.count.ToString();
        buyButton.interactable = remaining && canFitItem && GameManager.Instance.marketManager.Current > stock.GetBuyCost;
    }

    public void Buy() {
        remaining = GameManager.Instance.marketManager.Buy(stock);
        Refresh();
    }
}
