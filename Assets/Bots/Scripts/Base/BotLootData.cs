using System;

[Serializable]
public class BotLootData : RngItem {
    public BotLootData(ItemData item) {
        this.item = item;
    }

    public ItemData item;
}