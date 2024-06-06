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
    Image image;

    void Start () {
        // load all necessary recipes
        // hook onto inventory change
        PlayerScript.singleton.inventoryUpdateEvent.AddListener(Refresh);
    }

    void Update () {

    }

    void Refresh(List<Item> items) {
        foreach(CraftingListEntry entry in craftingListEntries) {
            entry.Refresh(entry.recipe.CheckForRequirements(items));
        }

        if(selectedRecipe != null) {
            craftingButton.interactable = selectedRecipe.CheckForRequirements(items);
        }
    }

    void SelectRecipe(CraftingRecipe recipe) {
        if(!image.gameObject.activeSelf) {
            image.gameObject.SetActive(true);
        }

        selectedRecipe = recipe.Clone();
        // refresh the thingy stuff
        title.text = recipe.output.data.name;
        description.text = recipe.output.data.description;
        image.sprite = recipe.output.data.itemIcon;
        // this is when we put in requirements into the requirement slot
        // this is when we update the status of the crafting button (this will require that we scan the inventory) (we need an inventory reference probably)
        
    }
}