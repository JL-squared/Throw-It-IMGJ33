using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemStock {
    public ItemData item;
    public float sellPercent;
    public float buyPercent;
    public int count;

    public int GetBuyCost {
        get { return Mathf.FloorToInt(buyPercent * item.marketBuyCost); }
    }

    public int GetSellCost {
        get { return Mathf.FloorToInt(sellPercent * item.marketSellCost); }
    }
}

public class MarketManager : MonoBehaviour {
    public int Current {
        private set {
            _current = value;
            UIMaster.Instance.inGameHUD.SetMarketCurrency(value);
        }
        get { return _current; }
    }

    private int _current = 100;
    public List<ItemStock> stocks;

    public void Start() {
        _current = 100;
        stocks = new List<ItemStock>();
        foreach (var item in ItemUtils.itemDatas) {
            if (item.Value.marketLimit == 0)
                continue;

            stocks.Add(new ItemStock {
                item = item.Value,
                sellPercent = 1,
                buyPercent = 1,
                count = 10,
            });
        }

        stocks.Sort((a, b) => string.Compare(a.item.title, b.item.title));
    }

    public bool Buy(ItemStock stock) {
        stock.count--;
        if (stock.count <= 0)
            return false;
        Current -= stock.GetBuyCost;
        Player.Instance.AddItem(new Item(stock.item, 1));
        return true;
    }


    public void MarketUpdate() {
        foreach (var stock in stocks) {
            stock.count = stock.item.marketLimit;
        } 
    }

    internal void Sell(Item item) {
        Current += item.Data.marketSellCost * item.Count;
    }
}
