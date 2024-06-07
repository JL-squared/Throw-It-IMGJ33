using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "ScriptableObjects/New Crafting Recipe", order = 2)]
public class CraftingRecipe : ScriptableObject {
    public List<Item> requirements;
    public Item output;
    public int craftingTime;

    public bool CheckForRequirements(List<Item> items) {
        List<Item> tempRequirements = new List<Item>(requirements);

        foreach(Item item in items) {
            foreach(Item requirement in requirements) {
                if(requirement.Equals(item)) {
                    tempRequirements.Remove(requirement);
                }
            }
        }

        return tempRequirements.Count == 0;
    }

    // This should be running ONLY WHEN WE ACTUALLY KNOW IF THERE'S ALL THE REQUIREMENTS (pretty please)
    public void CraftItem(List<Item> items) {
        foreach (Item item in items) {
            foreach (Item requirement in requirements) {
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
