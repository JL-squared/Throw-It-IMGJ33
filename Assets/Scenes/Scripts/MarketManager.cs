using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

// X = sellPercent, Y = buyPercent
public class ItemStock {
    public ItemData item;
    public Vector2[] percents;
    public int count;
    public Vector2 recentInteractionCounts;

    public Vector2Int[] Costs {
        get {
            return percents.AsEnumerable().Select((x) => new int2(math.ceil(x * new Vector2(item.marketSellCost, item.marketBuyCost)))).Select(x => new Vector2Int(x.x, x.y)).ToArray();
        }
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

    public int startingCurrency = 100;
    public float chanceToFluctuate = 0.05f;
    public float rngFluctuationsFactor = 0.2f;
    public float marketResponseFactor = 0.4f;
    private int _current;
    public List<ItemStock> stocks;

    public void Start() {
        Current = startingCurrency;
        stocks = new List<ItemStock>();
        foreach (var item in Registries.items.data) {
            if (item.Value.marketLimit == 0)
                continue;

            stocks.Add(new ItemStock {
                item = item.Value,
                percents = new Vector2[2] {
                    Vector2.one, Vector2.one,
                },
                count = item.Value.marketLimit,
            });
        }

        stocks.Sort((a, b) => string.Compare(a.item.name, b.item.name));
    }

    public bool Buy(ItemStock stock) {
        stock.count--;
        if (stock.count <= 0)
            return false;
        Current -= stock.Costs[1].y;
        Player.Instance.AddItem(new ItemStack(stock.item, 1));
        stock.recentInteractionCounts.y += 1;
        return true;
    }


    public void MarketUpdate() {
        foreach (var stock in stocks) {
            stock.count = stock.item.marketLimit;
            float2 values = (new float2(UnityEngine.Random.value - 0.5f, UnityEngine.Random.value) - 0.5f) * 2f;
            values *= UnityEngine.Random.value < chanceToFluctuate ? rngFluctuationsFactor : 0f; // market rng fluctuations
            values += 1f;

            float demand = stock.recentInteractionCounts.y - stock.recentInteractionCounts.x;
            values -= new float2(-demand, demand) * marketResponseFactor; // market response

            values = math.clamp(values, new float2(0.01), new float2(100.0));
            stock.percents[1] = new Vector2(values.x, values.y);
        }

        UIMaster.Instance.inGameHUD.RefreshMarket();

        foreach (var stock in stocks) {
            stock.percents[0] = stock.percents[1];
            stock.recentInteractionCounts = Vector2.zero;
        }
    }

    internal void Sell(ItemStack item) {
        ItemStock stock = stocks.Find((x) => x.item == item.Data);
        Current += stock.Costs[1].x * item.Count;
        stock.recentInteractionCounts.x += item.Count;
    }
}
