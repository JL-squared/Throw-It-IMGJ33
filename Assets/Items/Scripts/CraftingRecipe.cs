using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRecipe : ScriptableObject {
    public List<Item> requirements;
    public Item output;
    public int craftingTime;
}
