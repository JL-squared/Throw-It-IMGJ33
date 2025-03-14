using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Scriptable Objects/New Crafting Recipe", order = 2)]
public class CraftingRecipe : ScriptableObject {
    public List<ItemStack> requirements;
    public ItemStack output;
    public int craftingTime;

    public string m_name;

    // This should be running ONLY WHEN WE ACTUALLY KNOW IF THERE'S ALL THE REQUIREMENTS (pretty please)
    public void CraftItem(List<ItemStack> items) {
        foreach (ItemStack item in items) {
            foreach (ItemStack requirement in requirements) {
                if (requirement.Equals(item)) {
                    item.Count -= requirement.Count;
                }
            }
        }
    }

    public CraftingRecipe Clone() {
        return (CraftingRecipe)MemberwiseClone();
    }
}
