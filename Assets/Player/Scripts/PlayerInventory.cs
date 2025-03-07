using UnityEngine.Events;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory : PlayerBehaviour {
    public bool keepUpdatingHolsterTransform;
    public GameObject viewModel;
    public ItemContainer hotbar = new ItemContainer(10);
    public ItemContainer backpack = new ItemContainer(16);
    public ItemContainer Inventory {
        get {
            return new ItemContainer(hotbar.Concat(backpack).ToList());
        } 
        private set {}
    }
    
    public ItemStack cursorItem;

    [SerializeField]
    private int equipped;
    public int Equipped {
        get {
            return equipped;
        }
        set {
            equipped = value;
            selectedEvent?.Invoke(equipped);
        }
    }
    [HideInInspector]
    public ItemStack EquippedItem { get { return hotbar[equipped]; } }

    [HideInInspector]
    public UnityEvent<int> selectedEvent;
    [HideInInspector]
    public UnityEvent<int, bool> slotUpdateEvent;

    public UnityEvent<List<ItemStack>> onInventoryUpdate;

    public UnityEvent initializedInventory;

    public void Start() {
        hotbar.Initialize();
        backpack.Initialize();
        hotbar.onUpdate.AddListener((container) => {
            onInventoryUpdate.Invoke(container);
        });
        backpack.onUpdate.AddListener((container) => {
            onInventoryUpdate.Invoke(container);
        });
        initializedInventory.Invoke();
        UIScriptMaster.Instance.inventory.Initialize(backpack);

        cursorItem = new ItemStack();

        cursorItem.onUpdate.AddListener(() => {
            UIScriptMaster.Instance.cursorItemDrag.Refresh(cursorItem);
            UIScriptMaster.Instance.cursorItemDrag.itemDisplay.index = -1000;
        });

        // Add temp items at start
        hotbar.PutItem(new ItemStack("snowball", 1));
        hotbar.PutItem(new ItemStack("battery", 1));
        hotbar.PutItem(new ItemStack("shovel", 1));
        hotbar.PutItem(new ItemStack("wires", 1));

        for (int i = 0; i < 10; i++) {
            ItemStack item = hotbar[i];
            int copy = i;
            item.onEmpty.AddListener(() => {
                if (this.equipped == copy) {
                    Unequip();
                }
            });

            item.onUpdate.AddListener(() => {
                if (this.equipped == copy) {
                    Unequip();
                    Equip();
                }
            });
        }

        /*
        container[4].SwapItem(container[0]);
        container[5].SwapItem(new ItemStack());
        */

        Unequip();
        Equip();
    }

    private void Update() {
        EquippedItem.logic?.EquippedUpdate(player);
        foreach (ItemStack item in Inventory) {
            item.logic?.Update(player);
        }

        if (viewModel != null && !EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty() && keepUpdatingHolsterTransform) {
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void InitializeBackpack() {
       
    }

    public void ToggleInventory() {
        UIScriptMaster.Instance.inGameHUD.ToggleInventory();
    }

    public void SelectSlot(int slotValue) {
        Unequip();
        Equipped = slotValue;
        Equip();
    }

    public void Scroll(float scroll) {
        Unequip();
        int newSelected = (Equipped + (int)scroll) % 10;
        Equipped = (newSelected < 0) ? 9 : newSelected;
        Equip();
    }

    public void PrimaryAction(InputAction.CallbackContext passthrough) {
        if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.PrimaryAction(passthrough, player);
        }
    }

    public void SecondaryAction(InputAction.CallbackContext passthrough) {
        if (!EquippedItem.IsEmpty()) {
            EquippedItem.logic.SecondaryAction(passthrough, player);
        }
    }
    
    private void Unequip() {
        EquippedItem.logic?.Unequipped(player);

        if (viewModel != null) {
            Destroy(viewModel);
        }
    }

    private void Equip() {
        EquippedItem.logic?.Equipped(player);

        if (!EquippedItem.IsNullOrDestroyed() && !EquippedItem.IsEmpty()) {
            viewModel = Instantiate(EquippedItem.Data.prefab, player.bobbing.viewModelHolster.transform);
            viewModel.transform.localPosition = EquippedItem.Data.viewModelPositionOffset;
            viewModel.transform.localRotation = EquippedItem.Data.viewModelRotationOffset;
            viewModel.transform.localScale = EquippedItem.Data.viewModelScaleOffset;
        }
    }

    public void DropItem() {
        ItemStack item = hotbar[equipped];
        if (item.Count > 0) {
            if (WorldItem.Spawn(hotbar[equipped].NewCount(1), player.camera.transform.position + player.camera.transform.forward, Quaternion.identity, player.movement.inner.Velocity))
                hotbar.RemoveItem(equipped, 1);
        }
    }
}