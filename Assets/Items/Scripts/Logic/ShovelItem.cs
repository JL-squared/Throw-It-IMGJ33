using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShovelItem : ToolItem {
    bool canPickupSnow;
    float durability;

    public override void SecondaryAction(InputAction.CallbackContext context, Player player) {
        base.SecondaryAction(context, player);

        if (canPickupSnow && !context.canceled) {
            ShovelItemData data = (ShovelItemData)player.EquippedItem.Data;
            if (VoxelTerrain.Instance != null) {
                VoxelTerrain.Instance.ApplyVoxelEdit(new AddVoxelEdit {
                    center = player.lookingAt.Value.point,
                    maskMaterial = true,
                    material = 0,
                    radius = data.digRadius,
                    strength = data.digStrength,
                    writeMaterial = false,
                    scale = new float3(1f),
                });
            }

            player.AddItem(new ItemStack("snowball", 1));
        }
    }

    public override void Unequipped(Player player) {
        base.Unequipped(player);
        canPickupSnow = false;
        UIMaster.Instance.inGameHUD.SetRightClickHint(false);
    }

    public override void EquippedUpdate(Player player) {
        canPickupSnow = false;
        if (player.lookingAt != null) {
            RaycastHit info = player.lookingAt.Value;
            VoxelChunk chunk = info.collider.GetComponent<VoxelChunk>();
            if (chunk != null) {
                canPickupSnow = chunk.GetTriangleIndexMaterialType(info.triangleIndex) == 0 && player.CanFitItem(new ItemStack("snowball", 1));
            }
        }

        UIMaster.Instance.inGameHUD.SetRightClickHint(canPickupSnow);
    }
}
