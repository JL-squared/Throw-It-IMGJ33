using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ItemUtils {
    private static Dictionary<string, ItemData> itemDatas;
    private static Dictionary<string, CraftingRecipe> craftingRecipes;
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() {
        // WARNING: This loads everything asset related during init. Makes it easy to code, but could bloat up the ram
        // for now this works but we're gonna need to find a way to dynamically unload unused assets later on (very easy to do with the adressables system)
        GetAllTypes(ref itemDatas, "Items");
        //GetAllTypes(ref craftingRecipes, "Item Recipes");

        foreach (var itemData in itemDatas.Values) {
        }
    }

    // Load all the types of an Adressable using its label into memory
    public static void GetAllTypes<T>(ref Dictionary<string, T> types, string label) where T : ScriptableObject {
        var temp = new Dictionary<string, T>();
        types = temp;

        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, (x) => {
            temp.TryAdd(x.name, x);
        });

        handle.WaitForCompletion();
    }


    // Fetch an item type using its specific name
    public static ItemData GetItemType(string name) {
        if (itemDatas == null)
            return null;

        if (itemDatas.TryGetValue(name, out var data)) {
            return data;
        } else {
            Debug.LogError($"Could not find item type '{name}'");
            return null;
        }
    }

    // Fetch a crafting recipe using its specific name
    public static CraftingRecipe GetCraftingRecipe(string name) {
        if (itemDatas == null)
            return null;

        if (craftingRecipes.TryGetValue(name, out var data)) {
            return data;
        } else {
            Debug.LogError($"Could not find item recipe '{name}'");
            return null;
        }
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
    public static CraftingRecipe GetCraftingRecipeFromType(ItemData itemData) {
        return craftingRecipes.GetValueOrDefault(itemData.name, null);
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
