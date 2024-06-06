using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour {
    List<CraftingListEntry> craftingListEntries;
    public CraftingRecipe selectedRecipe;
    public Button craftingButton;
    Text title;
    Text description;
    Sprite sprite;

    void Start () {
        // load all necessary recipes
        // hook onto inventory change
    }

    void Update () {

    }

    void Refresh(List<Item> items) {
        foreach(CraftingListEntry entry in craftingListEntries) {
            entry.Refresh(entry.recipe.CheckForRequirements(items));
        }

        if(selectedRecipe != null) {
            // put the requirements in the visual slots
        }
    }

    void SelectRecipe(CraftingRecipe recipe) {
        selectedRecipe = recipe.Clone();
        // refresh the thingy stuff
        title.text = recipe.output.data.name;
        description.text = recipe.output.data.description;
        sprite = recipe.output.data.itemIcon;
        // this is when we put in requirements into the requirement slot
        // this is when we update the status of the crafting button (this will require that we scan the inventory)
    }
}