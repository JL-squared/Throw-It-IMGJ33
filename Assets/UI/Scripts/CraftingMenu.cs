using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour {
    [Header("List")]
    public GameObject content;

    [Header("Recipe Display")]
    public CraftingRecipe selectedRecipe;
    public Button craftingButton;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Image itemDisplay;
    public GameObject recipeRequirementsContent;
    public List<RecipeRequirement> recipeRequirements;
    public GameObject recipeRequirementPrefab;

    public float iconBumpDuration;
    public float iconBumpAmplitude;

    private void Awake() {
        UIScriptMaster.Instance.loadCall.AddListener(Load);
    }

    void Start () {
        // load all necessary recipes
        // hook onto inventory change
        Player.Instance.inventory.onInventoryUpdate.AddListener(Refresh);
    }

    public void Load() {
        foreach (CraftingRecipe definition in Registries.craftingRecipes.data.Values) {
            var thing = Instantiate(UIScriptMaster.Instance.recipeEntryPrefab);
            thing.transform.SetParent(content.transform);
            thing.transform.localScale = Vector3.one;
            thing.GetComponent<RecipeEntry>().Init(definition);
        }
    }

    void Refresh(List<ItemStack> items) { // fix this later
        var i = 0;
        foreach(RecipeRequirement entry in recipeRequirements) {
            entry.Refresh(Player.Instance.inventory.Inventory, selectedRecipe.requirements[i]);
            i++;
        }

        if(selectedRecipe != null) {
            craftingButton.interactable = Player.Instance.inventory.Inventory.CheckForItems(selectedRecipe.requirements);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe) {
        if (recipe == selectedRecipe)
            return;

        selectedRecipe = recipe;

        if(!itemDisplay.gameObject.activeSelf) {
            itemDisplay.gameObject.SetActive(true);
        }

        title.gameObject.CancelTweens();
        description.gameObject.CancelTweens();

        // refresh the thingy stuff
        title.text = selectedRecipe.m_name;
        Utils.WriteTextEffect(title);

        string text = selectedRecipe.output.Data.description;
        TypeInfo data = recipe.output.Data.GetType().GetTypeInfo();
        Attribute attribute = data.GetCustomAttribute(typeof(ItemAttribute));

        foreach (var prop in recipe.output.Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            Debug.Log(prop.Name);
            var attrs = (ItemAttribute[])prop.GetCustomAttributes(typeof(ItemAttribute), true);
            foreach (var attr in attrs)
            {
                text += string.Format("{0}\n", prop.GetValue(recipe.output.Data));
            }
        }

        description.text = text; 
        Utils.WriteTextEffect(description, 0.01f);

        itemDisplay.sprite = selectedRecipe.output.Data.icon;
        // this is when we put in requirements into the requirement slot
        // this is when we update the status of the crafting button (this will require that we scan the inventory) (we need an inventory reference probably)
        Utils.KillChildren(recipeRequirementsContent.transform);
        recipeRequirements.Clear();
        foreach (ItemStack stack in selectedRecipe.requirements) {
            var gO = Instantiate(recipeRequirementPrefab, recipeRequirementsContent.transform);
            var c = gO.GetComponent<RecipeRequirement>();
            c.Refresh(Player.Instance.inventory.Inventory, stack);
            recipeRequirements.Add(c);
        }

        craftingButton.interactable = Player.Instance.inventory.Inventory.CheckForItems(selectedRecipe.requirements);
    }

    public void CraftRecipeTemp() {
        if (selectedRecipe != null) {
            if (Player.Instance.inventory.Inventory.CheckForItems(selectedRecipe.requirements)) {
                Player.Instance.inventory.Inventory.TakeItems(selectedRecipe.requirements);

                Player.Instance.inventory.Inventory.PutItem(selectedRecipe.output); // Shrimple as that

                Utils.Bump(itemDisplay.gameObject, iconBumpDuration, iconBumpAmplitude);
            }
        }
    }
}