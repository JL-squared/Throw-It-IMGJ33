using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using System;

// Global static util class used to fetch items, buildings, names
public static class Util {
    private static Dictionary<string, ItemType> itemTypes;
    private static Dictionary<string, CraftingRecipe> craftingRecipes;
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        // WARNING: This loads everything asset related during init. Makes it easy to code, but could bloat up the ram
        // for now this works but we're gonna need to find a way to dynamically unload unused assets later on (very easy to do with the adressables system)
        GetAllTypes(ref itemTypes, "Items");
        GetAllTypes(ref craftingRecipes, "Item Recipes");

        // Register all types of custom item serializers (so we know how to serialize their item datas)
        ItemSerializerRegistry.Init();
        ItemSerializerRegistry.Register<SimpleItem>();
    }

    // Load all the types of an Adressable using its label into memory
    public static void GetAllTypes<T>(ref Dictionary<string, T> types, string label) where T : ScriptableObject {
        var temp = new Dictionary<string, T>();
        types = temp;

        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, (x) => {
            temp.TryAdd(x.name, x);
        });
    }

    // I love unity and interface!!
    public static bool IsNullOrDestroyed(this object value) {
        return ReferenceEquals(value, null) || value.Equals(null);
    }

    /*
    // Fetch an item type using its specific name
    public static ItemType GetItemType(string name) {
        return itemTypes.GetValueOrDefault(name, null);
    }
    */

    // Fetch a crafting recipe using its specific name
    public static CraftingRecipe GetCraftingRecipe(string name) {
        return craftingRecipes.GetValueOrDefault(name, null);
    }

    // Fetch all crafting recipes
    public static List<CraftingRecipe> GetAllCraftingRecipes() {
        return craftingRecipes.Values.ToList();
    }

    // Fetch all crafting recipes but with a generic
    public static List<T> GetAllCraftingRecipes<T>() where T : CraftingRecipe {
        List<T> result = new List<T>();
        foreach (KeyValuePair<string, CraftingRecipe> entry in craftingRecipes) {
            if (entry.Value.GetType() == typeof(T)) {
                result.Add((T)entry.Value);
            }
        }
        return result;
    }

    // Fetch the crafting recipe for a specific item type
    public static CraftingRecipe GetCraftingRecipeFromType(ItemType itemType) {
        return craftingRecipes.GetValueOrDefault(itemType.name, null);
    }

    // Jarvis, scan this guys balls
    public static void KillChildren(GameObject gameObject, Action<GameObject> method = null) {
        for (var i = gameObject.transform.childCount - 1; i >= 0; i--) {
            var obj = gameObject.transform.GetChild(i).gameObject;
            method?.Invoke(obj);
            UnityEngine.Object.Destroy(obj);
        }
    }
}
