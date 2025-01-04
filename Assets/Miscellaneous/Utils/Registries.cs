using UnityEngine;
using System;
using System.Collections.Generic;

public static class Registries {
    public static AddressablesRegistry<ItemData> items;
    public static AddressablesRegistry<BotData> bots;
    public static AddressablesRegistry<GameObject> projectiles;
    public static AddressablesRegistry<GameObject> vehicles;

    #region Sounds
    public static AddressablesRegistry<AudioClip> music;
    public static AddressablesRegistry<AudioClip> snowBrickPlace;
    public static AddressablesRegistry<AudioClip> rockWalk;
    public static AddressablesRegistry<AudioClip> rockRun;
    public static AddressablesRegistry<AudioClip> rockJump;
    #endregion


    private static Dictionary<Type, Item> itemsLogic = new Dictionary<Type, Item>();
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        items = new AddressablesRegistry<ItemData>("Items");
        bots = new AddressablesRegistry<BotData>("Bots");
        projectiles = new AddressablesRegistry<GameObject>("Projectiles");

        music = new AddressablesRegistry<AudioClip>("Music");

        snowBrickPlace = new AddressablesRegistry<AudioClip>("Snow Brick Place");

        rockWalk = new AddressablesRegistry<AudioClip>("Rock Walk");
        rockRun = new AddressablesRegistry<AudioClip>("Rock Run");
        rockJump = new AddressablesRegistry<AudioClip>("Rock Jump");

        Player.Instance.PlayMusic();
    }

    public static GameObject Summon(string id, EntityData data) {
        string[] args = id.Split(':');
        string basic = args[0];
        string name = args[1];

        GameObject sauce = null;
        switch (basic) {
            case "bots":
                sauce = BotBase.Summon(data.bot, bots[name], data.position.Value, data.rotation.Value);
                break;
            case "projectiles":
                sauce = Projectile.Spawn((ProjectileItemData)items[name], data.position.Value, data.velocity.Value);
                break;
            case "entity":
                if (name == "world_item") {
                    sauce = WorldItem.Spawn(data.itemStack, data.position.Value, data.rotation.Value);
                }
                break;
            case "vehicles":
                //sauce = vehicles;
                break;
            default:
                sauce = null;
                break;
        };

        return sauce;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RegisterItems() {
        RegisterItem<ItemData>(new Item());
        RegisterItem<ShovelItemData>(new ShovelItem());
        RegisterItem<SnowballItemData>(new SnowballItem());
    }

    public static Item GetItem(ItemData dataType) {
        if (dataType == null) return null;
        itemsLogic.TryGetValue(dataType.GetType(), out var item);
        return (Item)item.Clone();
    }

    static void RegisterItem<T>(Item item) where T : ItemData {
        itemsLogic.Add(typeof(T), item);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() {
        itemsLogic = new Dictionary<Type, Item>();
    }
}
