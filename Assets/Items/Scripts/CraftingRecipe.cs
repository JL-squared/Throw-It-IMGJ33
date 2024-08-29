using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "ScriptableObjects/New Crafting Recipe", order = 2)]
public class CraftingRecipe : ScriptableObject {
    public List<ItemStack> requirements;
    public ItemStack output;
    public int craftingTime;

    public bool CheckForRequirements(List<ItemStack> items) {
        List<ItemStack> tempRequirements = new List<ItemStack>(requirements);

        foreach(ItemStack item in items) {
            foreach(ItemStack requirement in requirements) {
                if(requirement.Equals(item)) {
                    tempRequirements.Remove(requirement);
                }
            }
        }

        return tempRequirements.Count == 0;
    }

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
