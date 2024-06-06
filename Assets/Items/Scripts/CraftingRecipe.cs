using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CraftingRecipe : ScriptableObject {
    public List<Item> requirements;
    public Item output;
    public int craftingTime;

    public bool CheckForRequirements(List<Item> items) {
        int requirementsLeft = items.Count;

        foreach(Item item in items) {
            for(int i = requirementsLeft; i >= 0; i--) { // Count down instead of up
                var req = requirements[i];
                if(item.data.id == req.data.id && item.Count == req.Count) {
                    requirementsLeft--;
                }
            }
        }

        return requirementsLeft == 0;
    }
}
